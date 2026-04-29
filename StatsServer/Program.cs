using StatsServer.Data;
using StatsServer.Services;
using Microsoft.EntityFrameworkCore;
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

// ... после var app = builder.Build();

// Ждём готовности БД перед первым запросом
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StatsDbContext>();
    var retries = 10;
    var delay = TimeSpan.FromSeconds(3);
    for (int i = 0; i < retries; i++)
    {
        try
        {
            if (await db.Database.CanConnectAsync())
            {
                Console.WriteLine("[INIT] Successfully connected to PostgreSQL.");
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[INIT] Waiting for DB... ({i + 1}/{retries}): {ex.Message}");
        }
        await Task.Delay(delay);
    }
}

// Теперь безопасно инициализируем Redis из БД
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StatsDbContext>();
    var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
    var redisDb = redis.GetDatabase();

    var allPlayers = await db.Players.ToListAsync();
    foreach (var p in allPlayers)
    {
        double score = p.Wins;
        await redisDb.SortedSetAddAsync("leaderboard", p.Id.ToString(), score);
    }
    Console.WriteLine($"[INIT] Leaderboard populated with {allPlayers.Count} players.");
}


// Подключаем gRPC-сервис
app.MapGrpcService<StatsServiceImpl>();

// Обычные REST-ендпоинты
app.MapGet("/ping", () => "pong");
app.MapGet("/leaderboard", async (IConnectionMultiplexer redis, StatsDbContext db) =>
{
    try
    {
        var redisDb = redis.GetDatabase();
        const int limit = 4;

        var entries = await redisDb.SortedSetRangeByRankWithScoresAsync(
            "leaderboard",
            order: Order.Descending,
            start: 0,
            stop: limit - 1
        );

        var playerIds = entries
            .Select(e => Guid.TryParse(e.Element, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var players = await db.Players
            .Where(p => playerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var result = entries
            .Select(e => Guid.TryParse(e.Element, out var id) ? players.GetValueOrDefault(id) : null)
            .Where(p => p != null)
            .OrderByDescending(p => p.Wins)
            .ThenByDescending(p => p.Experience)
            .Take(limit)
            .Select(p => new
            {
                login = p.Login,
                wins = p.Wins,
                experience = p.Experience
            })
            .ToList();

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] /leaderboard: {ex.Message}");
        return Results.Ok(new List<object>());
    }
});
app.Run();