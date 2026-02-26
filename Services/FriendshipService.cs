using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ISteamService _steamService;
        private readonly ILogger<FriendshipService> _logger;

        public FriendshipService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ISteamService steamService,
            ILogger<FriendshipService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _steamService = steamService;
            _logger = logger;
        }

        public async Task<List<PlayerSearchResultDto>> SearchPlayersAsync(string query, Guid currentUserId)
        {
            var lowerQuery = query.ToLowerInvariant();

            return await _context.Users
                .Where(u => u.Id != currentUserId
                    && u.Username != null
                    && (u.Username.ToLower().Contains(lowerQuery)
                        || u.DisplayName.ToLower().Contains(lowerQuery)))
                .Take(20)
                .Select(u => new PlayerSearchResultDto
                {
                    UserId = u.Id,
                    Username = u.Username!,
                    DisplayName = u.DisplayName,
                    TotalGames = u.UserGames.Count
                })
                .ToListAsync();
        }

        public async Task<FriendshipStatusDto> GetFriendshipStatusAsync(Guid currentUserId, Guid otherUserId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == currentUserId && f.AddresseeId == otherUserId) ||
                    (f.RequesterId == otherUserId && f.AddresseeId == currentUserId));

            if (friendship == null)
                return new FriendshipStatusDto { Status = "none" };

            if (friendship.Status == "accepted")
                return new FriendshipStatusDto { Status = "friends", FriendshipId = friendship.Id };

            if (friendship.Status == "pending")
            {
                var direction = friendship.RequesterId == currentUserId
                    ? "pending_outgoing"
                    : "pending_incoming";
                return new FriendshipStatusDto { Status = direction, FriendshipId = friendship.Id };
            }

            // declined — treat as no relationship
            return new FriendshipStatusDto { Status = "none" };
        }

        public async Task<FriendRequestDto> SendFriendRequestAsync(Guid requesterId, Guid addresseeId)
        {
            if (requesterId == addresseeId)
                throw new BadRequestException("You cannot send a friend request to yourself.");

            var addressee = await _context.Users.FindAsync(addresseeId);
            if (addressee == null)
                throw new NotFoundException("User not found.");

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

            if (existing != null)
            {
                if (existing.Status == "accepted")
                    throw new BadRequestException("You are already friends with this user.");
                if (existing.Status == "pending")
                    throw new BadRequestException("A friend request already exists between you and this user.");
                // If declined, remove and allow re-request
                _context.Friendships.Remove(existing);
            }

            var requester = await _context.Users.FindAsync(requesterId);

            var friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                addresseeId,
                "friend_request",
                $"{requester?.DisplayName ?? "Someone"} sent you a friend request.",
                requesterId);

            return new FriendRequestDto
            {
                FriendshipId = friendship.Id,
                UserId = addresseeId,
                Username = addressee.Username ?? string.Empty,
                DisplayName = addressee.DisplayName,
                SentAt = friendship.CreatedAt,
                Direction = "outgoing"
            };
        }

        public async Task AcceptFriendRequestAsync(Guid currentUserId, Guid friendshipId)
        {
            var friendship = await _context.Friendships
                .Include(f => f.Addressee)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                throw new NotFoundException("Friend request not found.");

            if (friendship.AddresseeId != currentUserId)
                throw new BadRequestException("You can only accept friend requests sent to you.");

            if (friendship.Status != "pending")
                throw new BadRequestException("This friend request is no longer pending.");

            friendship.Status = "accepted";
            friendship.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                friendship.RequesterId,
                "friend_accepted",
                $"{friendship.Addressee.DisplayName} accepted your friend request.",
                currentUserId);
        }

        public async Task DeclineFriendRequestAsync(Guid currentUserId, Guid friendshipId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                throw new NotFoundException("Friend request not found.");

            if (friendship.AddresseeId != currentUserId)
                throw new BadRequestException("You can only decline friend requests sent to you.");

            if (friendship.Status != "pending")
                throw new BadRequestException("This friend request is no longer pending.");

            friendship.Status = "declined";
            friendship.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriendAsync(Guid currentUserId, Guid friendshipId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                throw new NotFoundException("Friendship not found.");

            if (friendship.RequesterId != currentUserId && friendship.AddresseeId != currentUserId)
                throw new BadRequestException("You are not part of this friendship.");

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(Guid userId)
        {
            return await _context.Friendships
                .Where(f => f.Status == "accepted" &&
                    (f.RequesterId == userId || f.AddresseeId == userId))
                .Select(f => new FriendDto
                {
                    UserId = f.RequesterId == userId ? f.AddresseeId : f.RequesterId,
                    Username = f.RequesterId == userId ? (f.Addressee.Username ?? string.Empty) : (f.Requester.Username ?? string.Empty),
                    DisplayName = f.RequesterId == userId ? f.Addressee.DisplayName : f.Requester.DisplayName,
                    FriendsSince = f.RespondedAt ?? f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
        {
            return await _context.Friendships
                .Where(f => f.Status == "pending" &&
                    (f.RequesterId == userId || f.AddresseeId == userId))
                .Select(f => new FriendRequestDto
                {
                    FriendshipId = f.Id,
                    UserId = f.RequesterId == userId ? f.AddresseeId : f.RequesterId,
                    Username = f.RequesterId == userId ? (f.Addressee.Username ?? string.Empty) : (f.Requester.Username ?? string.Empty),
                    DisplayName = f.RequesterId == userId ? f.Addressee.DisplayName : f.Requester.DisplayName,
                    SentAt = f.CreatedAt,
                    Direction = f.RequesterId == userId ? "outgoing" : "incoming"
                })
                .ToListAsync();
        }

        public async Task<SteamFriendSuggestionsDto> GetSteamFriendSuggestionsAsync(Guid currentUserId)
        {
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null || string.IsNullOrEmpty(user.SteamId))
                throw new BadRequestException("Steam account not linked.");

            var friendSteamIds = await _steamService.GetSteamFriendsAsync(user.SteamId);
            if (friendSteamIds == null)
                return new SteamFriendSuggestionsDto { IsPrivate = true };

            if (friendSteamIds.Count == 0)
                return new SteamFriendSuggestionsDto { IsPrivate = false };

            // Get existing friendship user IDs to exclude
            var existingFriendshipUserIds = await _context.Friendships
                .Where(f => f.RequesterId == currentUserId || f.AddresseeId == currentUserId)
                .Select(f => f.RequesterId == currentUserId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            var suggestions = await _context.Users
                .Where(u => u.SteamId != null
                    && friendSteamIds.Contains(u.SteamId)
                    && u.Id != currentUserId
                    && !existingFriendshipUserIds.Contains(u.Id))
                .Select(u => new PlayerSearchResultDto
                {
                    UserId = u.Id,
                    Username = u.Username!,
                    DisplayName = u.DisplayName,
                    TotalGames = u.UserGames.Count
                })
                .ToListAsync();

            return new SteamFriendSuggestionsDto { IsPrivate = false, Suggestions = suggestions };
        }
    }
}
