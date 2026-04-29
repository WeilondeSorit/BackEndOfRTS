using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using StatsServer.Data;
using StatsServer.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
var connectionString = builder.Configuration["ConnectionStrings:Postgres"];
builder.Services.AddDbContext<StatsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Redis
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// gRPC сервис
app.MapGrpcService<StatsServiceImpl>();

// Тестовый endpoint для проверки работы сервера
app.MapGet("/ping", () => "pong");

// REST leaderboard — возвращает только 4 лучших игрока
app.MapGet("/leaderboard", async (IConnectionMultiplexer redis, StatsDbContext db) =>
{
    try
    {
        const int limit = 4;
        var redisDb = redis.GetDatabase();
        var entries = await redisDb.SortedSetRangeByRankWithScoresAsync("leaderboard", order: Order.Descending, stop: limit - 1);
        var result = new List<object>();
        foreach (var entry in entries)
        {
            if (Guid.TryParse(entry.Element, out var playerId))
            {
                var player = await db.Players.FindAsync(playerId);
                if (player != null)
                {
                    result.Add(new { player.Login, player.Wins, player.Experience });
                }
            }
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] /leaderboard: {ex.Message}");
        // Возвращаем пустой массив, чтобы клиент не падал
        return Results.Ok(new List<object>());
    }
});

app.Run();