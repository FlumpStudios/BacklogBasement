using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileByUsernameAsync(string username);
    }
}
