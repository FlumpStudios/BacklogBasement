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
        public DbSet<GameClub> GameClubs { get; set; } = null!;
        public DbSet<GameClubMember> GameClubMembers { get; set; } = null!;
        public DbSet<GameClubRound> GameClubRounds { get; set; } = null!;
        public DbSet<GameClubNomination> GameClubNominations { get; set; } = null!;
        public DbSet<GameClubVote> GameClubVotes { get; set; } = null!;
        public DbSet<GameClubReview> GameClubReviews { get; set; } = null!;
        public DbSet<GameClubInvite> GameClubInvites { get; set; } = null!;
        public DbSet<DirectMessage> DirectMessages { get; set; } = null!;
        public DbSet<XpGrant> XpGrants { get; set; } = null!;
        public DbSet<DailyPoll> DailyPolls { get; set; } = null!;
        public DbSet<DailyPollGame> DailyPollGames { get; set; } = null!;
        public DbSet<DailyPollVote> DailyPollVotes { get; set; } = null!;
        public DbSet<DailyQuiz> DailyQuizzes { get; set; } = null!;
        public DbSet<DailyQuizOption> DailyQuizOptions { get; set; } = null!;
        public DbSet<DailyQuizAnswer> DailyQuizAnswers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleSubjectId)
                .IsUnique()
                .HasFilter("[GoogleSubjectId] IS NOT NULL");

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

            // Configure GameClub entity
            modelBuilder.Entity<GameClub>()
                .HasOne(gc => gc.Owner)
                .WithMany()
                .HasForeignKey(gc => gc.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GameClubMember entity
            modelBuilder.Entity<GameClubMember>()
                .HasIndex(gcm => new { gcm.ClubId, gcm.UserId })
                .IsUnique();

            modelBuilder.Entity<GameClubMember>()
                .HasOne(gcm => gcm.Club)
                .WithMany(gc => gc.Members)
                .HasForeignKey(gcm => gcm.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubMember>()
                .HasOne(gcm => gcm.User)
                .WithMany(u => u.GameClubMemberships)
                .HasForeignKey(gcm => gcm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure GameClubRound entity
            modelBuilder.Entity<GameClubRound>()
                .HasOne(gcr => gcr.Club)
                .WithMany(gc => gc.Rounds)
                .HasForeignKey(gcr => gcr.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubRound>()
                .HasOne(gcr => gcr.Game)
                .WithMany(g => g.GameClubRounds)
                .HasForeignKey(gcr => gcr.GameId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure GameClubNomination entity
            modelBuilder.Entity<GameClubNomination>()
                .HasIndex(gcn => new { gcn.RoundId, gcn.GameId })
                .IsUnique();

            modelBuilder.Entity<GameClubNomination>()
                .HasOne(gcn => gcn.Round)
                .WithMany(gcr => gcr.Nominations)
                .HasForeignKey(gcn => gcn.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubNomination>()
                .HasOne(gcn => gcn.NominatedByUser)
                .WithMany()
                .HasForeignKey(gcn => gcn.NominatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameClubNomination>()
                .HasOne(gcn => gcn.Game)
                .WithMany(g => g.GameClubNominations)
                .HasForeignKey(gcn => gcn.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GameClubVote entity
            modelBuilder.Entity<GameClubVote>()
                .HasIndex(gcv => new { gcv.RoundId, gcv.UserId })
                .IsUnique();

            modelBuilder.Entity<GameClubVote>()
                .HasOne(gcv => gcv.Round)
                .WithMany(gcr => gcr.Votes)
                .HasForeignKey(gcv => gcv.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubVote>()
                .HasOne(gcv => gcv.User)
                .WithMany()
                .HasForeignKey(gcv => gcv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameClubVote>()
                .HasOne(gcv => gcv.Nomination)
                .WithMany(gcn => gcn.Votes)
                .HasForeignKey(gcv => gcv.NominationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GameClubReview entity
            modelBuilder.Entity<GameClubReview>()
                .HasIndex(gcrev => new { gcrev.RoundId, gcrev.UserId })
                .IsUnique();

            modelBuilder.Entity<GameClubReview>()
                .HasOne(gcrev => gcrev.Round)
                .WithMany(gcr => gcr.Reviews)
                .HasForeignKey(gcrev => gcrev.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubReview>()
                .HasOne(gcrev => gcrev.User)
                .WithMany()
                .HasForeignKey(gcrev => gcrev.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GameClubInvite entity
            modelBuilder.Entity<GameClubInvite>()
                .HasOne(gci => gci.Club)
                .WithMany(gc => gc.Invites)
                .HasForeignKey(gci => gci.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameClubInvite>()
                .HasOne(gci => gci.InvitedByUser)
                .WithMany(u => u.SentClubInvites)
                .HasForeignKey(gci => gci.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameClubInvite>()
                .HasOne(gci => gci.Invitee)
                .WithMany(u => u.ReceivedClubInvites)
                .HasForeignKey(gci => gci.InviteeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure DirectMessage entity
            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(dm => dm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Recipient)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(dm => dm.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DirectMessage>()
                .HasIndex(dm => new { dm.SenderId, dm.RecipientId });

            modelBuilder.Entity<DirectMessage>()
                .HasIndex(dm => new { dm.RecipientId, dm.IsRead });

            // Configure XpGrant entity
            modelBuilder.Entity<XpGrant>()
                .HasOne(xg => xg.User)
                .WithMany()
                .HasForeignKey(xg => xg.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<XpGrant>()
                .HasIndex(xg => new { xg.UserId, xg.Reason, xg.ReferenceId })
                .IsUnique();

            modelBuilder.Entity<XpGrant>()
                .HasIndex(xg => xg.UserId);

            // Configure DailyPoll entity
            modelBuilder.Entity<DailyPoll>()
                .HasIndex(dp => dp.PollDate)
                .IsUnique();

            // Configure DailyPollGame entity
            modelBuilder.Entity<DailyPollGame>()
                .HasOne(dpg => dpg.Poll)
                .WithMany(dp => dp.Games)
                .HasForeignKey(dpg => dpg.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyPollGame>()
                .HasOne(dpg => dpg.Game)
                .WithMany()
                .HasForeignKey(dpg => dpg.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure DailyPollVote entity
            modelBuilder.Entity<DailyPollVote>()
                .HasIndex(dpv => new { dpv.PollId, dpv.UserId })
                .IsUnique();

            modelBuilder.Entity<DailyPollVote>()
                .HasOne(dpv => dpv.Poll)
                .WithMany(dp => dp.Votes)
                .HasForeignKey(dpv => dpv.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyPollVote>()
                .HasOne(dpv => dpv.User)
                .WithMany(u => u.PollVotes)
                .HasForeignKey(dpv => dpv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DailyQuiz entity
            modelBuilder.Entity<DailyQuiz>()
                .HasIndex(dq => dq.QuizDate)
                .IsUnique();

            // Configure DailyQuizOption entity
            modelBuilder.Entity<DailyQuizOption>()
                .HasOne(dqo => dqo.Quiz)
                .WithMany(dq => dq.Options)
                .HasForeignKey(dqo => dqo.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DailyQuizAnswer entity
            modelBuilder.Entity<DailyQuizAnswer>()
                .HasIndex(dqa => new { dqa.QuizId, dqa.UserId })
                .IsUnique();

            modelBuilder.Entity<DailyQuizAnswer>()
                .HasOne(dqa => dqa.Quiz)
                .WithMany(dq => dq.Answers)
                .HasForeignKey(dqa => dqa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyQuizAnswer>()
                .HasOne(dqa => dqa.User)
                .WithMany(u => u.QuizAnswers)
                .HasForeignKey(dqa => dqa.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}