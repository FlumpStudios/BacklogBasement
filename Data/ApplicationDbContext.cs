using Microsoft.EntityFrameworkCore;
using BacklogBasement.Models;

namespace BacklogBasement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<UserGame> UserGames { get; set; } = null!;
        public DbSet<PlaySession> PlaySessions { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<GameSuggestion> GameSuggestions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleSubjectId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.SteamId)
                .IsUnique()
                .HasFilter("[SteamId] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique()
                .HasFilter("[Username] IS NOT NULL");

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(30);

            // Configure Game entity
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.IgdbId)
                .IsUnique()
                .HasFilter("[IgdbId] IS NOT NULL");

            modelBuilder.Entity<Game>()
                .HasIndex(g => g.SteamAppId)
                .IsUnique()
                .HasFilter("[SteamAppId] IS NOT NULL");

            // Configure UserGame entity
            modelBuilder.Entity<UserGame>()
                .HasIndex(ug => new { ug.UserId, ug.GameId })
                .IsUnique();

            modelBuilder.Entity<UserGame>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGames)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGame>()
                .HasOne(ug => ug.Game)
                .WithMany(g => g.UserGames)
                .HasForeignKey(ug => ug.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PlaySession entity
            modelBuilder.Entity<PlaySession>()
                .HasOne(ps => ps.UserGame)
                .WithMany(ug => ug.PlaySessions)
                .HasForeignKey(ps => ps.UserGameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Friendship entity
            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.AddresseeId })
                .IsUnique();

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Addressee)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Notification entity
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            // Configure GameSuggestion entity
            modelBuilder.Entity<GameSuggestion>()
                .HasOne(gs => gs.Sender)
                .WithMany(u => u.SentSuggestions)
                .HasForeignKey(gs => gs.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSuggestion>()
                .HasOne(gs => gs.Recipient)
                .WithMany(u => u.ReceivedSuggestions)
                .HasForeignKey(gs => gs.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSuggestion>()
                .HasOne(gs => gs.Game)
                .WithMany(g => g.GameSuggestions)
                .HasForeignKey(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameSuggestion>()
                .HasIndex(gs => new { gs.RecipientUserId, gs.IsDismissed });
        }
    }
}