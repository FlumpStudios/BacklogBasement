using System;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface ISteamImportService
    {
        Task<SteamImportResult> ImportLibraryAsync(Guid userId, bool includePlaytime);
        Task<SteamStatusDto> GetSteamStatusAsync(Guid userId);
    }
}
