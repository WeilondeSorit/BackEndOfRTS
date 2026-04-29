using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StatsServer.Data;
using GameShared; // это пространство имён из proto

namespace StatsServer.Services
{
    public class StatsServiceImpl : StatsService.StatsServiceBase
    {
        private readonly StatsDbContext _db;
        private readonly IConnectionMultiplexer _redis;

        public StatsServiceImpl(StatsDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis;
        }

        public override async Task<Empty> ReportGameResult(GameResultRequest request, ServerCallContext context)
        {
            try
            {
                var playerId = Guid.Parse(request.PlayerId);
                var player = await _db.Players.FindAsync(playerId);
                if (player == null) return new Empty();

                if (request.IsWin) player.Wins++;
                else player.Losses++;

                player.Experience += request.ExperienceGained;
                player.Currency += request.ExperienceGained / 10; // или другая логика

                await _db.SaveChangesAsync();

                // Обновляем Redis
                var redisDb = _redis.GetDatabase();
                double score = player.Wins * 100.0 + player.Experience;
                await redisDb.SortedSetAddAsync("leaderboard", player.Id.ToString(), score);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ReportGameResult: {ex.Message}");
            }
            return new Empty();
        }
    }
}