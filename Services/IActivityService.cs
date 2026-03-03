using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IActivityService
    {
        Task LogAsync(Guid userId, string eventType, Guid? gameId = null, Guid? clubId = null, int? intValue = null);
        Task<IEnumerable<ActivityEventDto>> GetFeedAsync(int limit = 50);
    }
}
