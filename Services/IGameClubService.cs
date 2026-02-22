using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IGameClubService
    {
        // Club management
        Task<GameClubDetailDto> CreateClubAsync(Guid userId, CreateGameClubRequest request);
        Task<List<GameClubDto>> GetPublicClubsAsync();
        Task<List<GameClubDto>> GetMyClubsAsync(Guid userId);
        Task<GameClubDetailDto> GetClubDetailAsync(Guid userId, Guid clubId);
        Task<GameClubInviteDto> InviteMemberAsync(Guid userId, Guid clubId, Guid inviteeUserId);
        Task RespondToInviteAsync(Guid userId, Guid inviteId, bool accept);
        Task JoinPublicClubAsync(Guid userId, Guid clubId);
        Task RemoveMemberAsync(Guid userId, Guid clubId, Guid targetUserId);
        Task TransferOwnershipAsync(Guid userId, Guid clubId, Guid newOwnerId);
        Task UpdateMemberRoleAsync(Guid userId, Guid clubId, Guid targetUserId, string role);
        Task<List<GameClubInviteDto>> GetMyPendingInvitesAsync(Guid userId);
        Task DeleteClubAsync(Guid userId, Guid clubId);

        // Round management
        Task<GameClubRoundDto> StartNewRoundAsync(Guid userId, Guid clubId, StartRoundRequest request);
        Task<GameClubRoundDto> AdvanceRoundStatusAsync(Guid userId, Guid roundId);

        // Nominations & voting
        Task<GameClubNominationDto> NominateGameAsync(Guid userId, Guid roundId, Guid gameId);
        Task<GameClubVoteDto> VoteAsync(Guid userId, Guid roundId, Guid nominationId);

        // Reviews
        Task<GameClubReviewDto> SubmitReviewAsync(Guid userId, Guid roundId, SubmitReviewRequest request);
        Task<List<GameClubReviewDto>> GetRoundReviewsAsync(Guid userId, Guid roundId);

        // Game page integration
        Task<GameClubScoreDto?> GetClubScoreForGameAsync(Guid gameId);
        Task<List<GameClubReviewsForGameDto>> GetClubReviewsForGameAsync(Guid gameId, Guid? currentUserId);
    }

    public class GameClubVoteDto
    {
        public Guid Id { get; set; }
        public Guid NominationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
