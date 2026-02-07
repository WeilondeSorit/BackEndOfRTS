using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StatisticsService.Data;
using StatisticsService.Models;

namespace StatisticsService.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly StatisticsDbContext _context;

        public LeaderboardService(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderboardEntry>> GetGlobalLeaderboardAsync(int limit = 100, int offset = 0)
        {
            var stats = await _context.PlayerStats
                .OrderByDescending(s => s.Wins)
                .ThenByDescending(s => s.TotalMatches)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var leaderboard = stats.Select((stat, index) => new LeaderboardEntry
            {
                Rank = offset + index + 1,
                PlayerId = stat.PlayerId,
                Username = stat.Username,
                Wins = stat.Wins,
                Losses = stat.Losses,
                TotalMatches = stat.TotalMatches,
                WinRate = stat.WinRate,
                Kills = stat.Kills,
                WinStreak = stat.WinStreak,
                LastUpdated = stat.LastUpdated
            }).ToList();

            return leaderboard;
        }

        public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(int count = 10)
        {
            return await GetGlobalLeaderboardAsync(count, 0);
        }

        public async Task<LeaderboardEntry?> GetPlayerRankAsync(Guid playerId)
        {
            var playerStat = await _context.PlayerStats.FirstOrDefaultAsync(s => s.PlayerId == playerId);
            if (playerStat == null)
                return null;

            var betterPlayersCount = await _context.PlayerStats
                .CountAsync(s => s.Wins > playerStat.Wins || 
                               (s.Wins == playerStat.Wins && s.TotalMatches > playerStat.TotalMatches));

            return new LeaderboardEntry
            {
                Rank = betterPlayersCount + 1,
                PlayerId = playerStat.PlayerId,
                Username = playerStat.Username,
                Wins = playerStat.Wins,
                Losses = playerStat.Losses,
                TotalMatches = playerStat.TotalMatches,
                WinRate = playerStat.WinRate,
                Kills = playerStat.Kills,
                WinStreak = playerStat.WinStreak,
                LastUpdated = playerStat.LastUpdated
            };
        }

        public async Task UpdatePlayerStatsAsync(Guid playerId, string username, bool isWin, int kills = 0)
        {
            var playerStat = await _context.PlayerStats.FirstOrDefaultAsync(s => s.PlayerId == playerId);

            if (playerStat == null)
            {
                playerStat = new PlayerStats
                {
                    PlayerId = playerId,
                    Username = username,
                    Wins = 0,
                    Losses = 0,
                    TotalMatches = 0,
                    Kills = 0,
                    WinStreak = 0,
                    MaxWinStreak = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.PlayerStats.Add(playerStat);
            }

            playerStat.Username = username;
            playerStat.TotalMatches++;
            
            if (isWin)
            {
                playerStat.Wins++;
                playerStat.WinStreak++;
                if (playerStat.WinStreak > playerStat.MaxWinStreak)
                    playerStat.MaxWinStreak = playerStat.WinStreak;
            }
            else
            {
                playerStat.Losses++;
                playerStat.WinStreak = 0;
            }

            playerStat.Kills += kills;
            playerStat.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalPlayersAsync()
        {
            return await _context.PlayerStats.CountAsync();
        }
    }
}