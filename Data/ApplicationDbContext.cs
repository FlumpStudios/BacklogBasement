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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleSubjectId)
                .IsUnique();

            // Configure Game entity
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.IgdbId)
                .IsUnique();

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
        }
    }
}