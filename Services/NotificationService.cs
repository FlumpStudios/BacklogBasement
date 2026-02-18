using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BacklogBasement.Data;
using BacklogBasement.DTOs;
using BacklogBasement.Models;

namespace BacklogBasement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int limit = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Message = n.Message,
                    RelatedUserId = n.RelatedUserId,
                    RelatedGameId = n.RelatedGameId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            // Resolve related usernames
            var relatedUserIds = notifications
                .Where(n => n.RelatedUserId.HasValue)
                .Select(n => n.RelatedUserId!.Value)
                .Distinct()
                .ToList();

            if (relatedUserIds.Count > 0)
            {
                var usernames = await _context.Users
                    .Where(u => relatedUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Username);

                foreach (var notification in notifications)
                {
                    if (notification.RelatedUserId.HasValue &&
                        usernames.TryGetValue(notification.RelatedUserId.Value, out var username))
                    {
                        notification.RelatedUsername = username;
                    }
                }
            }

            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task CreateNotificationAsync(Guid userId, string type, string message, Guid? relatedUserId = null, Guid? relatedGameId = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Message = message,
                RelatedUserId = relatedUserId,
                RelatedGameId = relatedGameId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
