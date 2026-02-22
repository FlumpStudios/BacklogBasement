using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IMessageService
    {
        Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
        Task<List<DirectMessageDto>> GetMessagesAsync(Guid userId, Guid friendId);
        Task<DirectMessageDto> SendMessageAsync(Guid senderId, Guid recipientId, SendMessageRequest request);
        Task MarkConversationAsReadAsync(Guid userId, Guid friendId);
        Task<int> GetUnreadMessageCountAsync(Guid userId);
    }
}
