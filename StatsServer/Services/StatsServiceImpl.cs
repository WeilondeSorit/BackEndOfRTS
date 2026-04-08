using System;
using System.Threading.Tasks;
using GameShared;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StatsServer.Data;

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
            var playerId = Guid.Parse(request.PlayerId);
            var player = await _db.Players.FindAsync(playerId);
            if (player == null) return new Empty();
            
            if (request.IsWin)
                player.Wins++;
            else
                player.Losses++;
            
            player.Experience += request.ExperienceGained;
            await _db.SaveChangesAsync();
            
            var db = _redis.GetDatabase();
            double score = player.Wins * 1_000_000 + player.Experience;
            await db.SortedSetAddAsync("leaderboard", playerId.ToString(), score);
            
            return new Empty();
        }
    }
}