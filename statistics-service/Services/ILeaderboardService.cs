using StatisticsService.Models;

namespace StatisticsService.Services
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int limit = 100, int offset = 0);
        Task<List<LeaderboardEntry>> GetTopPlayersAsync(int count = 10);
        Task<LeaderboardEntry?> GetPlayerRankAsync(Guid playerId);
        Task UpdatePlayerStatsAsync(Guid playerId, string username, bool isWin, int kills = 0);
        Task<int> GetTotalPlayersAsync();
    }
}