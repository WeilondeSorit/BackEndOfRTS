using System;
using System.Text.Json;
using Grpc.Net.Client;
using StackExchange.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection; // необходимо для AddSingleton
using GameShared;   

var builder = WebApplication.CreateBuilder(args);

// Redis
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "localhost");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// gRPC клиент к StatsServer
builder.Services.AddSingleton(provider =>
{
    var channel = GrpcChannel.ForAddress(builder.Configuration["StatsServerUrl"] ?? "http://localhost:5001");
    return new StatsService.StatsServiceClient(channel);
});

var app = builder.Build();

// Начать сессию (создать пустое состояние)
app.MapPost("/session/start", async (StartRequest req, IConnectionMultiplexer redis) =>
{
    if (!redis.IsConnected)
        return Results.Problem("Redis not available", statusCode: 503);

    var db = redis.GetDatabase();
    var sessionId = Guid.NewGuid().ToString();
    var session = new SessionData
    {
        PlayerId = req.PlayerId,
        Wood = 50,
        Stone = 50,
        Food = 50,
        Villagers = 5,
        Archers = 5,
        Enemies = 0,
        PlayerBaseHp = 1000,
        EnemyBaseHp = 1000
    };
    bool saved = await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session), TimeSpan.FromHours(1));
    if (!saved)
        return Results.Problem("Failed to save session", statusCode: 500);
    
    return Results.Ok(new { SessionId = sessionId });
});

// Загрузить состояние сессии (полное)
app.MapGet("/session/{sessionId}/load", async (string sessionId, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (session == null) return Results.NotFound();
    
    return Results.Ok(new
    {
        session.Wood,
        session.Stone,
        session.Food,
        session.Villagers,
        session.Archers,
        session.Enemies,
        session.PlayerBaseHp,
        session.EnemyBaseHp
    });
});

// Сохранить полное состояние сессии (перезаписать)
app.MapPost("/session/{sessionId}/save", async (string sessionId, SaveStateRequest req, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (session == null) return Results.NotFound();

    session.Wood = req.Wood;
    session.Stone = req.Stone;
    session.Food = req.Food;
    session.Villagers = req.Villagers;
    session.Archers = req.Archers;
    session.Enemies = req.Enemies;
    session.PlayerBaseHp = req.PlayerBaseHp;
    session.EnemyBaseHp = req.EnemyBaseHp;

    await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session), TimeSpan.FromHours(1));
    return Results.Ok();
});

// Обновить только часть данных
app.MapPost("/session/{sessionId}/update", async (string sessionId, UpdateRequest req, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (session == null) return Results.NotFound();
    
    if (req.Wood.HasValue) session.Wood = req.Wood.Value;
    if (req.Stone.HasValue) session.Stone = req.Stone.Value;
    if (req.Food.HasValue) session.Food = req.Food.Value;
    if (req.Villagers.HasValue) session.Villagers = req.Villagers.Value;
    if (req.Archers.HasValue) session.Archers = req.Archers.Value;
    if (req.Enemies.HasValue) session.Enemies = req.Enemies.Value;
    if (req.PlayerBaseHp.HasValue) session.PlayerBaseHp = req.PlayerBaseHp.Value;
    if (req.EnemyBaseHp.HasValue) session.EnemyBaseHp = req.EnemyBaseHp.Value;
    
    await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session), TimeSpan.FromHours(1));
    return Results.Ok();
});

// Действие атаки
app.MapPost("/session/{sessionId}/action", async (string sessionId, ActionRequest req, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (session == null) return Results.NotFound();
    
    if (req.Type == "attack")
    {
        session.EnemyBaseHp -= req.Damage;
        if (session.EnemyBaseHp < 0) session.EnemyBaseHp = 0;
    }
    await db.StringSetAsync($"session:{sessionId}", JsonSerializer.Serialize(session), TimeSpan.FromHours(1));
    return Results.Ok();
});

// Завершить сессию
app.MapPost("/session/{sessionId}/end", async (string sessionId, EndRequest req, IConnectionMultiplexer redis, StatsService.StatsServiceClient statsClient, ILogger<Program> logger) =>
{
    var db = redis.GetDatabase();
    var json = await db.StringGetAsync($"session:{sessionId}");
    if (json.IsNullOrEmpty) return Results.NotFound();
    var session = JsonSerializer.Deserialize<SessionData>(json);
    if (session == null) return Results.NotFound();

    // Попытка отправить результат в gRPC (некритично)
    try
    {
        int expGained = req.IsWin ? 50 : 10;
        var grpcReq = new GameResultRequest
        {
            PlayerId = session.PlayerId.ToString(),
            IsWin = req.IsWin,
            ExperienceGained = expGained
        };
        await statsClient.ReportGameResultAsync(grpcReq);
        logger.LogInformation("gRPC report success for session {SessionId}", sessionId);
    }
    catch (Exception ex)
    {
        // Логируем, но не прерываем завершение сессии
        logger.LogWarning(ex, "Failed to report game result via gRPC for session {SessionId}", sessionId);
    }

    await db.KeyDeleteAsync($"session:{sessionId}");
    return Results.Ok();
});

app.Run();

// ===== МОДЕЛИ ДАННЫХ =====
public record StartRequest(Guid PlayerId);
public record ActionRequest(string Type, int Damage);
public record EndRequest(bool IsWin);
public record SaveStateRequest(
    int Wood, int Stone, int Food,
    int Villagers, int Archers, int Enemies,
    int PlayerBaseHp, int EnemyBaseHp
);
public record UpdateRequest(
    int? Wood, int? Stone, int? Food,
    int? Villagers, int? Archers, int? Enemies,
    int? PlayerBaseHp, int? EnemyBaseHp
);

public class SessionData
{
    public Guid PlayerId { get; set; }
    public int Wood { get; set; }
    public int Stone { get; set; }
    public int Food { get; set; }
    public int Villagers { get; set; }
    public int Archers { get; set; }
    public int Enemies { get; set; }
    public int PlayerBaseHp { get; set; }
    public int EnemyBaseHp { get; set; }
}