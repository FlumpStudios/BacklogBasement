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
    }
}