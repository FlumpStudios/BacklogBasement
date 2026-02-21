using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class GameClubService : IGameClubService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public GameClubService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // --- Club Management ---

        public async Task<GameClubDetailDto> CreateClubAsync(Guid userId, CreateGameClubRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Club name is required.");

            var user = await _context.Users.FindAsync(userId)
                ?? throw new NotFoundException("User not found.");

            var club = new GameClub
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                IsPublic = request.IsPublic,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameClubs.Add(club);

            var member = new GameClubMember
            {
                Id = Guid.NewGuid(),
                ClubId = club.Id,
                UserId = userId,
                Role = "owner",
                JoinedAt = DateTime.UtcNow
            };

            _context.GameClubMembers.Add(member);
            await _context.SaveChangesAsync();

            return await GetClubDetailAsync(userId, club.Id);
        }

        public async Task<List<GameClubDto>> GetPublicClubsAsync()
        {
            return await _context.GameClubs
                .Where(gc => gc.IsPublic)
                .OrderByDescending(gc => gc.CreatedAt)
                .Select(gc => new GameClubDto
                {
                    Id = gc.Id,
                    Name = gc.Name,
                    Description = gc.Description,
                    IsPublic = gc.IsPublic,
                    OwnerDisplayName = gc.Owner.DisplayName,
                    OwnerUsername = gc.Owner.Username ?? string.Empty,
                    MemberCount = gc.Members.Count,
                    CurrentRound = gc.Rounds
                        .Where(r => r.Status != "completed")
                        .OrderByDescending(r => r.RoundNumber)
                        .Select(r => new GameClubRoundDto
                        {
                            Id = r.Id,
                            RoundNumber = r.RoundNumber,
                            Status = r.Status,
                            GameId = r.GameId,
                            GameName = r.Game != null ? r.Game.Name : null,
                            GameCoverUrl = r.Game != null ? r.Game.CoverUrl : null,
                            NominatingDeadline = r.NominatingDeadline,
                            VotingDeadline = r.VotingDeadline,
                            PlayingDeadline = r.PlayingDeadline,
                            ReviewingDeadline = r.ReviewingDeadline,
                            CompletedAt = r.CompletedAt
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<List<GameClubDto>> GetMyClubsAsync(Guid userId)
        {
            return await _context.GameClubMembers
                .Where(gcm => gcm.UserId == userId)
                .Select(gcm => new GameClubDto
                {
                    Id = gcm.Club.Id,
                    Name = gcm.Club.Name,
                    Description = gcm.Club.Description,
                    IsPublic = gcm.Club.IsPublic,
                    OwnerDisplayName = gcm.Club.Owner.DisplayName,
                    OwnerUsername = gcm.Club.Owner.Username ?? string.Empty,
                    MemberCount = gcm.Club.Members.Count,
                    CurrentRound = gcm.Club.Rounds
                        .Where(r => r.Status != "completed")
                        .OrderByDescending(r => r.RoundNumber)
                        .Select(r => new GameClubRoundDto
                        {
                            Id = r.Id,
                            RoundNumber = r.RoundNumber,
                            Status = r.Status,
                            GameId = r.GameId,
                            GameName = r.Game != null ? r.Game.Name : null,
                            GameCoverUrl = r.Game != null ? r.Game.CoverUrl : null,
                            NominatingDeadline = r.NominatingDeadline,
                            VotingDeadline = r.VotingDeadline,
                            PlayingDeadline = r.PlayingDeadline,
                            ReviewingDeadline = r.ReviewingDeadline,
                            CompletedAt = r.CompletedAt
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<GameClubDetailDto> GetClubDetailAsync(Guid userId, Guid clubId)
        {
            var club = await _context.GameClubs
                .Include(gc => gc.Owner)
                .Include(gc => gc.Members).ThenInclude(m => m.User)
                .Include(gc => gc.Rounds).ThenInclude(r => r.Game)
                .Include(gc => gc.Rounds).ThenInclude(r => r.Nominations).ThenInclude(n => n.Game)
                .Include(gc => gc.Rounds).ThenInclude(r => r.Nominations).ThenInclude(n => n.NominatedByUser)
                .Include(gc => gc.Rounds).ThenInclude(r => r.Nominations).ThenInclude(n => n.Votes)
                .Include(gc => gc.Rounds).ThenInclude(r => r.Reviews)
                .Include(gc => gc.Invites).ThenInclude(i => i.InvitedByUser)
                .Include(gc => gc.Invites).ThenInclude(i => i.Invitee)
                .FirstOrDefaultAsync(gc => gc.Id == clubId)
                ?? throw new NotFoundException("Club not found.");

            var currentMember = club.Members.FirstOrDefault(m => m.UserId == userId);
            var isMember = currentMember != null;

            if (!club.IsPublic && !isMember)
                throw new NotFoundException("Club not found.");

            var pendingInvites = isMember && (currentMember!.Role == "owner" || currentMember.Role == "admin")
                ? club.Invites.Where(i => i.Status == "pending").ToList()
                : new List<GameClubInvite>();

            return new GameClubDetailDto
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                IsPublic = club.IsPublic,
                OwnerDisplayName = club.Owner.DisplayName,
                OwnerUsername = club.Owner.Username ?? string.Empty,
                MemberCount = club.Members.Count,
                CurrentUserRole = currentMember?.Role,
                Members = club.Members
                    .OrderBy(m => m.JoinedAt)
                    .Select(m => new GameClubMemberDto
                    {
                        UserId = m.UserId,
                        DisplayName = m.User.DisplayName,
                        Username = m.User.Username ?? string.Empty,
                        Role = m.Role,
                        JoinedAt = m.JoinedAt
                    })
                    .ToList(),
                PendingInvites = pendingInvites
                    .Select(i => new GameClubInviteDto
                    {
                        Id = i.Id,
                        ClubId = i.ClubId,
                        ClubName = club.Name,
                        InvitedByUserId = i.InvitedByUserId,
                        InvitedByDisplayName = i.InvitedByUser.DisplayName,
                        InviteeUserId = i.InviteeUserId,
                        InviteeDisplayName = i.Invitee.DisplayName,
                        Status = i.Status,
                        CreatedAt = i.CreatedAt
                    })
                    .ToList(),
                CurrentRound = BuildRoundDto(club.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault(r => r.Status != "completed"), userId),
                Rounds = club.Rounds
                    .OrderByDescending(r => r.RoundNumber)
                    .Select(r => BuildRoundDto(r, userId)!)
                    .ToList()
            };
        }

        private static GameClubRoundDto? BuildRoundDto(GameClubRound? round, Guid userId)
        {
            if (round == null) return null;

            var userVote = round.Votes.FirstOrDefault(v => v.UserId == userId);
            var completedReviews = round.Reviews;
            var avgScore = completedReviews.Any() ? completedReviews.Average(r => (double)r.Score) : (double?)null;

            return new GameClubRoundDto
            {
                Id = round.Id,
                RoundNumber = round.RoundNumber,
                Status = round.Status,
                GameId = round.GameId,
                GameName = round.Game?.Name,
                GameCoverUrl = round.Game?.CoverUrl,
                UserHasVoted = userVote != null,
                UserHasReviewed = round.Reviews.Any(r => r.UserId == userId),
                UserVotedNominationId = userVote?.NominationId,
                NominatingDeadline = round.NominatingDeadline,
                VotingDeadline = round.VotingDeadline,
                PlayingDeadline = round.PlayingDeadline,
                ReviewingDeadline = round.ReviewingDeadline,
                CompletedAt = round.CompletedAt,
                AverageScore = avgScore,
                Nominations = round.Nominations
                    .OrderByDescending(n => n.Votes.Count)
                    .Select(n => new GameClubNominationDto
                    {
                        Id = n.Id,
                        GameId = n.GameId,
                        GameName = n.Game.Name,
                        GameCoverUrl = n.Game.CoverUrl,
                        NominatedByUserId = n.NominatedByUserId,
                        NominatedByDisplayName = n.NominatedByUser.DisplayName,
                        VoteCount = n.Votes.Count,
                        CreatedAt = n.CreatedAt
                    })
                    .ToList()
            };
        }

        public async Task<GameClubInviteDto> InviteMemberAsync(Guid userId, Guid clubId, Guid inviteeUserId)
        {
            var member = await GetMemberOrThrowAsync(userId, clubId);
            if (member.Role == "member")
                throw new BadRequestException("Only admins and owners can invite members.");

            if (userId == inviteeUserId)
                throw new BadRequestException("You cannot invite yourself.");

            var invitee = await _context.Users.FindAsync(inviteeUserId)
                ?? throw new NotFoundException("User not found.");

            var alreadyMember = await _context.GameClubMembers
                .AnyAsync(m => m.ClubId == clubId && m.UserId == inviteeUserId);
            if (alreadyMember)
                throw new BadRequestException("This user is already a member of the club.");

            var pendingInvite = await _context.GameClubInvites
                .AnyAsync(i => i.ClubId == clubId && i.InviteeUserId == inviteeUserId && i.Status == "pending");
            if (pendingInvite)
                throw new BadRequestException("This user already has a pending invite.");

            var inviter = await _context.Users.FindAsync(userId)!;
            var club = await _context.GameClubs.FindAsync(clubId)!;

            var invite = new GameClubInvite
            {
                Id = Guid.NewGuid(),
                ClubId = clubId,
                InvitedByUserId = userId,
                InviteeUserId = inviteeUserId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.GameClubInvites.Add(invite);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                inviteeUserId,
                "club_invite",
                $"{inviter!.DisplayName} invited you to join \"{club!.Name}\"",
                userId);

            return new GameClubInviteDto
            {
                Id = invite.Id,
                ClubId = clubId,
                ClubName = club.Name,
                InvitedByUserId = userId,
                InvitedByDisplayName = inviter.DisplayName,
                InviteeUserId = inviteeUserId,
                InviteeDisplayName = invitee.DisplayName,
                Status = invite.Status,
                CreatedAt = invite.CreatedAt
            };
        }

        public async Task RespondToInviteAsync(Guid userId, Guid inviteId, bool accept)
        {
            var invite = await _context.GameClubInvites
                .Include(i => i.Club)
                .FirstOrDefaultAsync(i => i.Id == inviteId)
                ?? throw new NotFoundException("Invite not found.");

            if (invite.InviteeUserId != userId)
                throw new BadRequestException("This invite is not for you.");

            if (invite.Status != "pending")
                throw new BadRequestException("This invite has already been responded to.");

            invite.Status = accept ? "accepted" : "declined";
            invite.RespondedAt = DateTime.UtcNow;

            if (accept)
            {
                var alreadyMember = await _context.GameClubMembers
                    .AnyAsync(m => m.ClubId == invite.ClubId && m.UserId == userId);

                if (!alreadyMember)
                {
                    var member = new GameClubMember
                    {
                        Id = Guid.NewGuid(),
                        ClubId = invite.ClubId,
                        UserId = userId,
                        Role = "member",
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.GameClubMembers.Add(member);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task JoinPublicClubAsync(Guid userId, Guid clubId)
        {
            var club = await _context.GameClubs.FindAsync(clubId)
                ?? throw new NotFoundException("Club not found.");

            if (!club.IsPublic)
                throw new BadRequestException("This club is invite-only.");

            var alreadyMember = await _context.GameClubMembers
                .AnyAsync(m => m.ClubId == clubId && m.UserId == userId);
            if (alreadyMember)
                throw new BadRequestException("You are already a member of this club.");

            var member = new GameClubMember
            {
                Id = Guid.NewGuid(),
                ClubId = clubId,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            _context.GameClubMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(Guid userId, Guid clubId, Guid targetUserId)
        {
            var currentMember = await GetMemberOrThrowAsync(userId, clubId);
            if (currentMember.Role == "member")
                throw new BadRequestException("Only admins and owners can remove members.");

            if (userId == targetUserId)
                throw new BadRequestException("You cannot remove yourself. Transfer ownership or leave the club.");

            var targetMember = await _context.GameClubMembers
                .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == targetUserId)
                ?? throw new NotFoundException("Member not found.");

            if (targetMember.Role == "owner")
                throw new BadRequestException("Cannot remove the club owner.");

            if (currentMember.Role == "admin" && targetMember.Role == "admin")
                throw new BadRequestException("Admins cannot remove other admins.");

            _context.GameClubMembers.Remove(targetMember);
            await _context.SaveChangesAsync();
        }

        public async Task TransferOwnershipAsync(Guid userId, Guid clubId, Guid newOwnerId)
        {
            var currentMember = await GetMemberOrThrowAsync(userId, clubId);
            if (currentMember.Role != "owner")
                throw new BadRequestException("Only the owner can transfer ownership.");

            var newOwnerMember = await _context.GameClubMembers
                .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == newOwnerId)
                ?? throw new NotFoundException("New owner must be a club member.");

            var club = await _context.GameClubs.FindAsync(clubId)!;
            club!.OwnerId = newOwnerId;
            currentMember.Role = "admin";
            newOwnerMember.Role = "owner";

            await _context.SaveChangesAsync();
        }

        public async Task UpdateMemberRoleAsync(Guid userId, Guid clubId, Guid targetUserId, string role)
        {
            if (role != "admin" && role != "member")
                throw new BadRequestException("Role must be 'admin' or 'member'.");

            var currentMember = await GetMemberOrThrowAsync(userId, clubId);
            if (currentMember.Role != "owner")
                throw new BadRequestException("Only the owner can change member roles.");

            var targetMember = await _context.GameClubMembers
                .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == targetUserId)
                ?? throw new NotFoundException("Member not found.");

            if (targetMember.Role == "owner")
                throw new BadRequestException("Cannot change the owner's role. Use transfer ownership instead.");

            targetMember.Role = role;
            await _context.SaveChangesAsync();
        }

        public async Task<List<GameClubInviteDto>> GetMyPendingInvitesAsync(Guid userId)
        {
            return await _context.GameClubInvites
                .Where(i => i.InviteeUserId == userId && i.Status == "pending")
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new GameClubInviteDto
                {
                    Id = i.Id,
                    ClubId = i.ClubId,
                    ClubName = i.Club.Name,
                    InvitedByUserId = i.InvitedByUserId,
                    InvitedByDisplayName = i.InvitedByUser.DisplayName,
                    InviteeUserId = i.InviteeUserId,
                    InviteeDisplayName = i.Invitee.DisplayName,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task DeleteClubAsync(Guid userId, Guid clubId)
        {
            var member = await GetMemberOrThrowAsync(userId, clubId);
            if (member.Role != "owner")
                throw new BadRequestException("Only the owner can delete a club.");

            var club = await _context.GameClubs
                .Include(gc => gc.Members)
                .FirstOrDefaultAsync(gc => gc.Id == clubId)
                ?? throw new NotFoundException("Club not found.");

            var otherMemberIds = club.Members
                .Where(m => m.UserId != userId)
                .Select(m => m.UserId)
                .ToList();

            _context.GameClubs.Remove(club);
            await _context.SaveChangesAsync();

            foreach (var memberId in otherMemberIds)
            {
                await _notificationService.CreateNotificationAsync(
                    memberId,
                    "club_deleted",
                    $"The club \"{club.Name}\" has been closed by its owner.");
            }
        }

        // --- Round Management ---

        public async Task<GameClubRoundDto> StartNewRoundAsync(Guid userId, Guid clubId, StartRoundRequest request)
        {
            var member = await GetMemberOrThrowAsync(userId, clubId);
            if (member.Role == "member")
                throw new BadRequestException("Only admins and owners can start rounds.");

            var hasActiveRound = await _context.GameClubRounds
                .AnyAsync(r => r.ClubId == clubId && r.Status != "completed");
            if (hasActiveRound)
                throw new BadRequestException("There is already an active round. Complete it before starting a new one.");

            var roundNumber = await _context.GameClubRounds
                .Where(r => r.ClubId == clubId)
                .MaxAsync(r => (int?)r.RoundNumber) ?? 0;

            var round = new GameClubRound
            {
                Id = Guid.NewGuid(),
                ClubId = clubId,
                RoundNumber = roundNumber + 1,
                Status = "nominating",
                NominatingDeadline = request.NominatingDeadline,
                VotingDeadline = request.VotingDeadline,
                PlayingDeadline = request.PlayingDeadline,
                ReviewingDeadline = request.ReviewingDeadline,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameClubRounds.Add(round);
            await _context.SaveChangesAsync();

            // Notify all members
            var memberIds = await _context.GameClubMembers
                .Where(m => m.ClubId == clubId && m.UserId != userId)
                .Select(m => m.UserId)
                .ToListAsync();

            var club = await _context.GameClubs.FindAsync(clubId)!;
            foreach (var memberId in memberIds)
            {
                await _notificationService.CreateNotificationAsync(
                    memberId,
                    "club_round_started",
                    $"Round {round.RoundNumber} has started in \"{club!.Name}\" — nominate your games!");
            }

            return BuildRoundDto(round, userId)!;
        }

        public async Task<GameClubRoundDto> AdvanceRoundStatusAsync(Guid userId, Guid roundId)
        {
            var round = await _context.GameClubRounds
                .Include(r => r.Nominations).ThenInclude(n => n.Votes)
                .Include(r => r.Nominations).ThenInclude(n => n.Game)
                .Include(r => r.Nominations).ThenInclude(n => n.NominatedByUser)
                .Include(r => r.Votes)
                .Include(r => r.Reviews)
                .Include(r => r.Game)
                .FirstOrDefaultAsync(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found.");

            var member = await GetMemberOrThrowAsync(userId, round.ClubId);
            if (member.Role == "member")
                throw new BadRequestException("Only admins and owners can advance round status.");

            var club = await _context.GameClubs.FindAsync(round.ClubId)!;
            var memberIds = await _context.GameClubMembers
                .Where(m => m.ClubId == round.ClubId && m.UserId != userId)
                .Select(m => m.UserId)
                .ToListAsync();

            switch (round.Status)
            {
                case "nominating":
                    if (!round.Nominations.Any())
                        throw new BadRequestException("Cannot advance to voting: no nominations yet.");
                    round.Status = "voting";
                    foreach (var memberId in memberIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            memberId, "club_voting_started",
                            $"Voting is open in \"{club!.Name}\" Round {round.RoundNumber} — cast your vote!");
                    }
                    break;

                case "voting":
                    // Resolve votes: pick winner
                    var winner = round.Nominations
                        .OrderByDescending(n => n.Votes.Count)
                        .ThenBy(n => n.CreatedAt)
                        .FirstOrDefault();
                    if (winner != null)
                        round.GameId = winner.GameId;
                    round.Status = "playing";
                    foreach (var memberId in memberIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            memberId, "club_game_selected",
                            $"The game for Round {round.RoundNumber} in \"{club!.Name}\" has been selected: \"{winner?.Game.Name}\"",
                            null, winner?.GameId);
                    }
                    break;

                case "playing":
                    round.Status = "reviewing";
                    foreach (var memberId in memberIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            memberId, "club_reviewing_started",
                            $"Time to submit your review for Round {round.RoundNumber} in \"{club!.Name}\"!",
                            null, round.GameId);
                    }
                    break;

                case "reviewing":
                    round.Status = "completed";
                    round.CompletedAt = DateTime.UtcNow;
                    foreach (var memberId in memberIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            memberId, "club_round_completed",
                            $"Round {round.RoundNumber} in \"{club!.Name}\" is complete — see the results!",
                            null, round.GameId);
                    }
                    break;

                default:
                    throw new BadRequestException("Round is already completed.");
            }

            await _context.SaveChangesAsync();

            // Reload with full includes for response
            round = await _context.GameClubRounds
                .Include(r => r.Nominations).ThenInclude(n => n.Game)
                .Include(r => r.Nominations).ThenInclude(n => n.NominatedByUser)
                .Include(r => r.Nominations).ThenInclude(n => n.Votes)
                .Include(r => r.Votes)
                .Include(r => r.Reviews)
                .Include(r => r.Game)
                .FirstAsync(r => r.Id == roundId);

            return BuildRoundDto(round, userId)!;
        }

        // --- Nominations & Voting ---

        public async Task<GameClubNominationDto> NominateGameAsync(Guid userId, Guid roundId, Guid gameId)
        {
            var round = await _context.GameClubRounds
                .Include(r => r.Nominations)
                .FirstOrDefaultAsync(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found.");

            await GetMemberOrThrowAsync(userId, round.ClubId);

            if (round.Status != "nominating")
                throw new BadRequestException("Nominations are closed for this round.");

            var game = await _context.Games.FindAsync(gameId)
                ?? throw new NotFoundException("Game not found.");

            var alreadyNominated = round.Nominations.Any(n => n.GameId == gameId);
            if (alreadyNominated)
                throw new BadRequestException("This game has already been nominated for this round.");

            var nomination = new GameClubNomination
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                NominatedByUserId = userId,
                GameId = gameId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameClubNominations.Add(nomination);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId)!;

            return new GameClubNominationDto
            {
                Id = nomination.Id,
                GameId = game.Id,
                GameName = game.Name,
                GameCoverUrl = game.CoverUrl,
                NominatedByUserId = userId,
                NominatedByDisplayName = user!.DisplayName,
                VoteCount = 0,
                CreatedAt = nomination.CreatedAt
            };
        }

        public async Task<GameClubVoteDto> VoteAsync(Guid userId, Guid roundId, Guid nominationId)
        {
            var round = await _context.GameClubRounds
                .Include(r => r.Nominations)
                .Include(r => r.Votes)
                .FirstOrDefaultAsync(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found.");

            await GetMemberOrThrowAsync(userId, round.ClubId);

            if (round.Status != "voting")
                throw new BadRequestException("Voting is not open for this round.");

            var nomination = round.Nominations.FirstOrDefault(n => n.Id == nominationId)
                ?? throw new NotFoundException("Nomination not found.");

            var existingVote = round.Votes.FirstOrDefault(v => v.UserId == userId);
            if (existingVote != null)
            {
                // Change vote
                existingVote.NominationId = nominationId;
                await _context.SaveChangesAsync();
                return new GameClubVoteDto
                {
                    Id = existingVote.Id,
                    NominationId = nominationId,
                    CreatedAt = existingVote.CreatedAt
                };
            }

            var vote = new GameClubVote
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                UserId = userId,
                NominationId = nominationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameClubVotes.Add(vote);
            await _context.SaveChangesAsync();

            return new GameClubVoteDto
            {
                Id = vote.Id,
                NominationId = nominationId,
                CreatedAt = vote.CreatedAt
            };
        }

        // --- Reviews ---

        public async Task<GameClubReviewDto> SubmitReviewAsync(Guid userId, Guid roundId, SubmitReviewRequest request)
        {
            if (request.Score < 0 || request.Score > 100)
                throw new BadRequestException("Score must be between 0 and 100.");

            var round = await _context.GameClubRounds
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found.");

            await GetMemberOrThrowAsync(userId, round.ClubId);

            if (round.Status != "reviewing")
                throw new BadRequestException("This round is not in the reviewing phase.");

            var existing = round.Reviews.FirstOrDefault(r => r.UserId == userId);
            if (existing != null)
            {
                existing.Score = request.Score;
                existing.Comment = request.Comment;
                existing.SubmittedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(userId)!;
                return new GameClubReviewDto
                {
                    Id = existing.Id,
                    UserId = userId,
                    DisplayName = user!.DisplayName,
                    Username = user.Username ?? string.Empty,
                    Score = existing.Score,
                    Comment = existing.Comment,
                    SubmittedAt = existing.SubmittedAt
                };
            }

            var review = new GameClubReview
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                UserId = userId,
                Score = request.Score,
                Comment = request.Comment,
                SubmittedAt = DateTime.UtcNow
            };

            _context.GameClubReviews.Add(review);
            await _context.SaveChangesAsync();

            var reviewer = await _context.Users.FindAsync(userId)!;
            return new GameClubReviewDto
            {
                Id = review.Id,
                UserId = userId,
                DisplayName = reviewer!.DisplayName,
                Username = reviewer.Username ?? string.Empty,
                Score = review.Score,
                Comment = review.Comment,
                SubmittedAt = review.SubmittedAt
            };
        }

        public async Task<List<GameClubReviewDto>> GetRoundReviewsAsync(Guid userId, Guid roundId)
        {
            var round = await _context.GameClubRounds
                .FirstOrDefaultAsync(r => r.Id == roundId)
                ?? throw new NotFoundException("Round not found.");

            await GetMemberOrThrowAsync(userId, round.ClubId);

            return await _context.GameClubReviews
                .Where(r => r.RoundId == roundId)
                .OrderByDescending(r => r.SubmittedAt)
                .Select(r => new GameClubReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    DisplayName = r.User.DisplayName,
                    Username = r.User.Username ?? string.Empty,
                    Score = r.Score,
                    Comment = r.Comment,
                    SubmittedAt = r.SubmittedAt
                })
                .ToListAsync();
        }

        // --- Game Page Integration ---

        public async Task<GameClubScoreDto?> GetClubScoreForGameAsync(Guid gameId)
        {
            var reviews = await _context.GameClubReviews
                .Where(r => r.Round.GameId == gameId && r.Round.Status == "completed")
                .ToListAsync();

            if (!reviews.Any()) return null;

            var roundCount = await _context.GameClubRounds
                .Where(r => r.GameId == gameId && r.Status == "completed")
                .CountAsync();

            return new GameClubScoreDto
            {
                GameId = gameId,
                AverageScore = Math.Round(reviews.Average(r => (double)r.Score), 1),
                ReviewCount = reviews.Count,
                RoundCount = roundCount
            };
        }

        // --- Helpers ---

        private async Task<GameClubMember> GetMemberOrThrowAsync(Guid userId, Guid clubId)
        {
            return await _context.GameClubMembers
                .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == userId)
                ?? throw new BadRequestException("You are not a member of this club.");
        }
    }
}
