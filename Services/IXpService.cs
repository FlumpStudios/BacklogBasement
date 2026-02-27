using System;
using System.Threading.Tasks;
using BacklogBasement.DTOs;

namespace BacklogBasement.Services
{
    public interface IXpService
    {
        const int XP_DAILY_LOGIN   = 10;
        const int XP_ADD_GAME      = 15;
        const int XP_FIRST_SESSION = 20;
        const int XP_COMPLETE_GAME = 50;
        const int XP_CLUB_NOMINATE  = 10;
        const int XP_CLUB_VOTE      = 10;
        const int XP_CLUB_REVIEW    = 25;
        const int XP_STEAM_IMPORT    = 50;
        const int XP_CREATE_CLUB      = 50;
        const int XP_SEND_SUGGESTION       = 15;
        const int XP_SEND_FRIEND_REQUEST   = 20;
        const int XP_ACCEPT_FRIEND_REQUEST = 20;
        const int XP_ADD_TO_BACKLOG        = 25;
        const int XP_DAILY_POLL            = 20;
        const int XP_QUIZ_CORRECT          = 30;
        const int XP_QUIZ_INCORRECT        = 5;

        Task<bool> TryGrantAsync(Guid userId, string reason, string referenceId, int amount);
        XpInfoDto ComputeLevel(int xpTotal);
    }
}
