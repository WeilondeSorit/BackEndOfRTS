using System;
using System.Text.Json;
using GameShared;
using Grpc.Net.Client;
using StackExchange.Redis;
using SessionServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Redis
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// gRPC клиент к StatsServer
builder.Services.AddSingleton(provider =>
{
    var channel = GrpcChannel.ForAddress(builder.Configuration["StatsServerUrl"]);
    return new StatsService.StatsServiceClient(channel);
});

var app = builder.Build();

// Начать сессию
app.MapPost("/session/start", async (StartRequest req, IConnectionMultiplexer redis) =>
{
    var sessionId = Guid.NewGuid().ToString();
    var db = redis.GetDatabase();
    var session = new SessionData { PlayerId = req.PlayerId };
    await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session), TimeSpan.FromHours(1));
    return Results.Ok(new { SessionId = sessionId });
});

// Получить состояние сессии
app.MapGet("/session/{sessionId}/state", async (string sessionId, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    return Results.Ok(new { session.Wood, session.Stone, session.Food, session.TowerHp, session.EnemyHp });
});

// Действие (упрощённо – только атака)
app.MapPost("/session/{sessionId}/action", async (string sessionId, ActionRequest req, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (req.Type == "attack")
    {
        session.EnemyHp -= req.Damage;
        if (session.EnemyHp <= 0) session.EnemyHp = 0;
    }
    await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session));
    return Results.Ok();
});

// Завершить сессию (победа/поражение)
app.MapPost("/session/{sessionId}/end", async (string sessionId, EndRequest req, IConnectionMultiplexer redis, StatsService.StatsServiceClient statsClient) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    
    int expGained = req.IsWin ? 50 : 10;
    var grpcReq = new GameResultRequest
    {
        PlayerId = session.PlayerId.ToString(),
        IsWin = req.IsWin,
        ExperienceGained = expGained
    };
    await statsClient.ReportGameResultAsync(grpcReq);
    
    await db.KeyDeleteAsync($"session:{sessionId}");
    return Results.Ok();
});

app.Run();

public record StartRequest(Guid PlayerId);
public record ActionRequest(string Type, int Damage);
public record EndRequest(bool IsWin);