using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IFriendshipService
    {
        Task<List<PlayerSearchResultDto>> SearchPlayersAsync(string query, Guid currentUserId);
        Task<FriendshipStatusDto> GetFriendshipStatusAsync(Guid currentUserId, Guid otherUserId);
        Task<FriendRequestDto> SendFriendRequestAsync(Guid requesterId, Guid addresseeId);
        Task AcceptFriendRequestAsync(Guid currentUserId, Guid friendshipId);
        Task DeclineFriendRequestAsync(Guid currentUserId, Guid friendshipId);
        Task RemoveFriendAsync(Guid currentUserId, Guid friendshipId);
        Task<List<FriendDto>> GetFriendsAsync(Guid userId);
        Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
    }
}
