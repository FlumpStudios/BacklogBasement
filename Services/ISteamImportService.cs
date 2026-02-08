using System;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface ISteamImportService
    {
        Task<SteamImportResult> ImportLibraryAsync(Guid userId, bool includePlaytime);
        Task<SteamStatusDto> GetSteamStatusAsync(Guid userId);
        Task<SteamPlaytimeSyncResult> SyncGamePlaytimeAsync(Guid userId, Guid gameId);
        Task<SteamBulkPlaytimeSyncResult> SyncAllPlaytimesAsync(Guid userId);
    }
}
