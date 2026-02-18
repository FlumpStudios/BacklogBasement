using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int limit = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid userId, Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task CreateNotificationAsync(Guid userId, string type, string message, Guid? relatedUserId = null, Guid? relatedGameId = null);
    }
}
