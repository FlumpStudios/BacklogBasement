using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface ICollectionService
    {
        Task<IEnumerable<CollectionItemDto>> GetUserCollectionAsync(Guid userId);
        Task<CollectionItemDto?> AddGameToCollectionAsync(Guid userId, AddToCollectionRequest request);
        Task<bool> RemoveGameFromCollectionAsync(Guid userId, Guid gameId);
        Task<CollectionItemDto?> GetCollectionItemAsync(Guid userId, Guid gameId);
        Task<CollectionItemDto?> UpdateGameStatusAsync(Guid userId, Guid gameId, string? status);
        Task<(int Added, int AlreadyOwned)> BulkAddGamesAsync(Guid userId, IEnumerable<Guid> gameIds);
        Task<PagedCollectionDto> GetPagedCollectionAsync(Guid userId, int skip, int take, string? search, string? status, string? source, string? playStatus, string sortBy, string sortDir);
        Task<CollectionStatsDto> GetCollectionStatsAsync(Guid userId);
    }
}