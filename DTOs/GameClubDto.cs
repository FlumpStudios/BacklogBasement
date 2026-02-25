using System;
using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class GameClubDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string? DiscordLink { get; set; }
        public string? WhatsAppLink { get; set; }
        public string? RedditLink { get; set; }
        public string? YouTubeLink { get; set; }
        public string OwnerDisplayName { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public GameClubRoundDto? CurrentRound { get; set; }
    }

    public class GameClubDetailDto : GameClubDto
    {
        public List<GameClubMemberDto> Members { get; set; } = new();
        public List<GameClubInviteDto> PendingInvites { get; set; } = new();
        public List<GameClubRoundDto> Rounds { get; set; } = new();
        public string? CurrentUserRole { get; set; }
    }

    public class GameClubRoundDto
    {
        public Guid Id { get; set; }
        public int RoundNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? GameId { get; set; }
        public string? GameName { get; set; }
        public string? GameCoverUrl { get; set; }
        public List<GameClubNominationDto> Nominations { get; set; } = new();
        public bool UserHasVoted { get; set; }
        public bool UserHasReviewed { get; set; }
        public bool UserHasNominated { get; set; }
        public Guid? UserVotedNominationId { get; set; }
        public DateTime? NominatingDeadline { get; set; }
        public DateTime? VotingDeadline { get; set; }
        public DateTime? PlayingDeadline { get; set; }
        public DateTime? ReviewingDeadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? AverageScore { get; set; }
    }

    public class GameClubNominationDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? GameCoverUrl { get; set; }
        public Guid NominatedByUserId { get; set; }
        public string NominatedByDisplayName { get; set; } = string.Empty;
        public string NominatedByUsername { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GameClubMemberDto
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class GameClubReviewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class GameClubInviteDto
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public Guid InvitedByUserId { get; set; }
        public string InvitedByDisplayName { get; set; } = string.Empty;
        public Guid InviteeUserId { get; set; }
        public string InviteeDisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GameClubScoreDto
    {
        public Guid GameId { get; set; }
        public double AverageScore { get; set; }
        public int ReviewCount { get; set; }
        public int RoundCount { get; set; }
    }

    public class GameClubReviewsForGameDto
    {
        public Guid ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public double AverageScore { get; set; }
        public int ReviewCount { get; set; }
        public bool IsCurrentUserMember { get; set; }
        public List<GameClubReviewDto> Reviews { get; set; } = new();
    }

    // Request DTOs
    public class CreateGameClubRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string? DiscordLink { get; set; }
        public string? WhatsAppLink { get; set; }
        public string? RedditLink { get; set; }
        public string? YouTubeLink { get; set; }
    }

    public class StartRoundRequest
    {
        public DateTime? NominatingDeadline { get; set; }
        public DateTime? VotingDeadline { get; set; }
        public DateTime? PlayingDeadline { get; set; }
        public DateTime? ReviewingDeadline { get; set; }
    }

    public class SubmitReviewRequest
    {
        public int Score { get; set; }
        public string? Comment { get; set; }
    }

    public class RespondToInviteRequest
    {
        public bool Accept { get; set; }
    }

    public class InviteMemberRequest
    {
        public Guid InviteeUserId { get; set; }
    }

    public class UpdateMemberRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class TransferOwnershipRequest
    {
        public Guid NewOwnerId { get; set; }
    }
}
