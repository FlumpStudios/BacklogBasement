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
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IProfanityService _profanityService;

        public MessageService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IProfanityService profanityService)
        {
            _context = context;
            _notificationService = notificationService;
            _profanityService = profanityService;
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
        {
            // Get all accepted friend IDs for this user
            var friendIds = await _context.Friendships
                .Where(f => f.Status == "accepted" &&
                    (f.RequesterId == userId || f.AddresseeId == userId))
                .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            if (friendIds.Count == 0)
                return new List<ConversationDto>();

            // Load all messages involving this user with accepted friends
            var messages = await _context.DirectMessages
                .Where(m => (m.SenderId == userId && friendIds.Contains(m.RecipientId)) ||
                            (m.RecipientId == userId && friendIds.Contains(m.SenderId)))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Get friend user info
            var friendUsers = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            // Group by conversation partner and build DTOs
            var conversations = messages
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g =>
                {
                    var friendId = g.Key;
                    var lastMessage = g.First(); // already ordered descending
                    friendUsers.TryGetValue(friendId, out var friend);
                    return new ConversationDto
                    {
                        FriendUserId = friendId,
                        FriendUsername = friend?.Username ?? string.Empty,
                        FriendDisplayName = friend?.DisplayName ?? string.Empty,
                        LastMessageContent = lastMessage.Content,
                        LastMessageIsFromMe = lastMessage.SenderId == userId,
                        LastMessageAt = lastMessage.CreatedAt,
                        UnreadCount = g.Count(m => m.RecipientId == userId && !m.IsRead)
                    };
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return conversations;
        }

        public async Task<List<DirectMessageDto>> GetMessagesAsync(Guid userId, Guid friendId)
        {
            // Validate friendship
            var isFriend = await _context.Friendships
                .AnyAsync(f => f.Status == "accepted" &&
                    ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                     (f.RequesterId == friendId && f.AddresseeId == userId)));

            if (!isFriend)
                throw new BadRequestException("You can only view messages with friends.");

            // Mark incoming messages as read
            await _context.DirectMessages
                .Where(m => m.SenderId == friendId && m.RecipientId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

            // Fetch thread oldest-first
            var messages = await _context.DirectMessages
                .Where(m => (m.SenderId == userId && m.RecipientId == friendId) ||
                            (m.SenderId == friendId && m.RecipientId == userId))
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new DirectMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderDisplayName = m.Sender.DisplayName,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return messages;
        }

        public async Task<DirectMessageDto> SendMessageAsync(Guid senderId, Guid recipientId, SendMessageRequest request)
        {
            if (senderId == recipientId)
                throw new BadRequestException("You cannot send a message to yourself.");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new BadRequestException("Message content cannot be empty.");

            if (request.Content.Length > 2000)
                throw new BadRequestException("Message content cannot exceed 2000 characters.");

            // Check accepted friendship
            var isFriend = await _context.Friendships
                .AnyAsync(f => f.Status == "accepted" &&
                    ((f.RequesterId == senderId && f.AddresseeId == recipientId) ||
                     (f.RequesterId == recipientId && f.AddresseeId == senderId)));

            if (!isFriend)
                throw new BadRequestException("You can only send messages to friends.");

            // Profanity check
            _profanityService.AssertClean(request.Content, "Message");

            var sender = await _context.Users.FindAsync(senderId)
                ?? throw new NotFoundException("Sender not found.");

            var message = new DirectMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                Content = request.Content.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.DirectMessages.Add(message);
            await _context.SaveChangesAsync();

            // Fire notification only if no existing unread direct_message notification from this sender
            var existingNotification = await _context.Notifications
                .AnyAsync(n => n.UserId == recipientId
                    && n.Type == "direct_message"
                    && n.RelatedUserId == senderId
                    && !n.IsRead);

            if (!existingNotification)
            {
                await _notificationService.CreateNotificationAsync(
                    recipientId,
                    "direct_message",
                    $"{sender.DisplayName} sent you a message.",
                    relatedUserId: senderId);
            }

            return new DirectMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderDisplayName = sender.DisplayName,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };
        }

        public async Task MarkConversationAsReadAsync(Guid userId, Guid friendId)
        {
            await _context.DirectMessages
                .Where(m => m.SenderId == friendId && m.RecipientId == userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid userId)
        {
            return await _context.DirectMessages
                .CountAsync(m => m.RecipientId == userId && !m.IsRead);
        }
    }
}
