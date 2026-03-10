using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IGamePasswordService
    {
        Task<List<GamePasswordDto>> GetPasswordsAsync(Guid userId, Guid gameId);
        Task<List<GamePasswordDto>> GetPublicPasswordsAsync(Guid gameId);
        Task<GamePasswordDto> AddPasswordAsync(Guid userId, Guid gameId, CreateGamePasswordRequest request);
        Task DeletePasswordAsync(Guid userId, Guid passwordId);
    }
}
