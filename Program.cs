using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using AspNet.Security.OpenId.Steam;
using BacklogBasement.Data;
using BacklogBasement.Services;
using BacklogBasement.Middleware;

namespace BacklogBasement;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

                // Add CORS configuration to allow frontend requests
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
            {
                policy.WithOrigins(frontendUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Important for cookies/authentication
            });
        });

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add database context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add HTTP context accessor
        builder.Services.AddHttpContextAccessor();

        // Register services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IGameService, GameService>();
        builder.Services.AddScoped<ICollectionService, CollectionService>();
        builder.Services.AddScoped<IPlaySessionService, PlaySessionService>();

        // Configure cookie authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "backlog-basement-auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/auth/access-denied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId not configured");
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Google ClientSecret not configured");

                // Map Google claims to our user identity
                options.ClaimActions.MapJsonKey("sub", "sub");
                options.ClaimActions.MapJsonKey("email", "email");
                options.ClaimActions.MapJsonKey("name", "name");
            })
            .AddSteam(options =>
            {
                options.ApplicationKey = builder.Configuration["Steam:ApiKey"]
                    ?? throw new InvalidOperationException("Steam:ApiKey not configured");
                options.CallbackPath = "/signin-steam"; // Use default path, avoid conflict with controller

                // Configure correlation cookie for the OAuth flow
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            });

        // Add authorization
        builder.Services.AddAuthorization();

        // Use Mock IGDB Service if credentials are not configured
        var igdbClientId = builder.Configuration["Igdb:ClientId"];
        var igdbClientSecret = builder.Configuration["Igdb:ClientSecret"];

        if (string.IsNullOrEmpty(igdbClientId) || string.IsNullOrEmpty(igdbClientSecret))
        {
            builder.Services.AddScoped<IIgdbService, MockIgdbService>();
            builder.Services.AddHttpClient<IIgdbService, MockIgdbService>();
        }
        else
        {
            builder.Services.AddScoped<IIgdbService, IgdbService>();
            builder.Services.AddHttpClient<IIgdbService, IgdbService>();
        }

        // Register Steam services
        builder.Services.AddHttpClient<ISteamService, SteamService>();
        builder.Services.AddScoped<ISteamImportService, SteamImportService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // Create database on startup in development
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
            }
        }

        app.UseHttpsRedirection();

        // Add CORS middleware before authentication
        app.UseCors("AllowFrontend");

        // Add exception handling middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

