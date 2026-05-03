using MainServer.Data;
using MainServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Подключение к PostgreSQL
var connectionString = builder.Configuration["ConnectionStrings:Postgres"];
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

// ===== JWT конфигурация =====
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDev_D0N0T_USE_IN_PROD_12345678";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "game-server";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "game-client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// ========== ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ==========

// Генератор JWT
string GenerateJwtToken(Guid playerId)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
        new Claim("playerId", playerId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// Проверка, что playerId из URL совпадает с токеном
bool IsPlayerAuthorized(HttpContext httpContext, Guid requestedPlayerId)
{
    var playerIdClaim = httpContext.User.FindFirst("playerId")?.Value;
    return playerIdClaim != null && Guid.TryParse(playerIdClaim, out var tokenPlayerId) && tokenPlayerId == requestedPlayerId;
}

// ========== АВТОРИЗАЦИЯ ==========

// Регистрация
app.MapPost("/auth/register", async (RegisterRequest req, AppDbContext db) =>
{
    if (await db.Players.AnyAsync(p => p.Login == req.Login))
        return Results.BadRequest("Login exists");

    var player = new Player
    {
        Id = Guid.NewGuid(),
        Login = req.Login,
        PasswordHash = req.Password,
        Experience = 0,
        Currency = 100,
        Wins = 0,
        Losses = 0
    };
    db.Players.Add(player);
    await db.SaveChangesAsync();

    var token = GenerateJwtToken(player.Id);
    return Results.Ok(new { player.Id, Token = token });
});

// Логин
app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var player = await db.Players.FirstOrDefaultAsync(p => p.Login == req.Login && p.PasswordHash == req.Password);
    if (player == null) return Results.Unauthorized();

    var token = GenerateJwtToken(player.Id);
    return Results.Ok(new { player.Id, Token = token });
});

// ========== ДОСТИЖЕНИЯ И КВЕСТЫ (полностью сохранены) ==========

// GET /achievements – список всех достижений (открытый)
app.MapGet("/achievements", async (AppDbContext db) =>
{
    var achievements = await db.Achievements
        .OrderBy(a => a.Id)
        .Select(a => new { a.Id, a.Key, a.Name, a.Description, a.RequiredValue, a.RewardCurrency, a.RewardExperience })
        .ToListAsync();
    return Results.Ok(achievements);
});

// GET /player/{id}/achievements – достижения игрока с прогрессом (защищён)
app.MapGet("/player/{id:guid}/achievements", async (Guid id, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    var player = await db.Players.FindAsync(id);
    if (player == null) return Results.NotFound("Игрок не найден");

    var achievements = await db.Achievements.OrderBy(a => a.Id).ToListAsync();
    var playerAchievements = await db.PlayerAchievements
        .Where(pa => pa.PlayerId == id)
        .ToDictionaryAsync(pa => pa.AchievementId, pa => pa);

    var result = achievements.Select(a => new
    {
        a.Id,
        a.Name,
        a.Description,
        a.RequiredValue,
        a.RewardCurrency,
        a.RewardExperience,
        Progress = playerAchievements.ContainsKey(a.Id) ? playerAchievements[a.Id].Progress : 0,
        IsCompleted = playerAchievements.ContainsKey(a.Id) && playerAchievements[a.Id].Progress >= a.RequiredValue,
        IsRewardClaimed = playerAchievements.ContainsKey(a.Id) && playerAchievements[a.Id].IsRewardClaimed
    });

    return Results.Ok(result);
}).RequireAuthorization();

// POST /player/{id}/achievement/{achievementId}/progress – добавить прогресс (защищён)
app.MapPost("/player/{id:guid}/achievement/{achievementId:int}/progress", async (Guid id, int achievementId, AddProgressRequest req, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    var player = await db.Players.FindAsync(id);
    if (player == null) return Results.NotFound();

    var achievement = await db.Achievements.FindAsync(achievementId);
    if (achievement == null) return Results.NotFound("Достижение не найдено");

    var playerAch = await db.PlayerAchievements
        .FirstOrDefaultAsync(pa => pa.PlayerId == id && pa.AchievementId == achievementId);

    if (playerAch == null)
    {
        playerAch = new PlayerAchievement
        {
            PlayerId = id,
            AchievementId = achievementId,
            Progress = 0,
            IsRewardClaimed = false,
            UnlockedAt = DateTime.UtcNow
        };
        db.PlayerAchievements.Add(playerAch);
    }

    if (playerAch.Progress >= achievement.RequiredValue)
        return Results.Ok(new { message = "Достижение уже выполнено", progress = playerAch.Progress, completed = true });

    playerAch.Progress += req.Increment;
    if (playerAch.Progress > achievement.RequiredValue)
        playerAch.Progress = achievement.RequiredValue;

    await db.SaveChangesAsync();

    bool justCompleted = playerAch.Progress >= achievement.RequiredValue;
    return Results.Ok(new
    {
        progress = playerAch.Progress,
        completed = justCompleted,
        message = justCompleted ? "Достижение выполнено! Заберите награду в меню." : "Прогресс обновлён"
    });
}).RequireAuthorization();

// POST /player/{id}/achievement/{achievementId}/claim – забрать награду (защищён)
app.MapPost("/player/{id:guid}/achievement/{achievementId:int}/claim", async (Guid id, int achievementId, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    var player = await db.Players.FindAsync(id);
    if (player == null) return Results.NotFound();

    var achievement = await db.Achievements.FindAsync(achievementId);
    if (achievement == null) return Results.NotFound();

    var playerAch = await db.PlayerAchievements
        .FirstOrDefaultAsync(pa => pa.PlayerId == id && pa.AchievementId == achievementId);

    if (playerAch == null || playerAch.Progress < achievement.RequiredValue)
        return Results.BadRequest("Достижение ещё не выполнено");

    if (playerAch.IsRewardClaimed)
        return Results.BadRequest("Награда уже получена");

    // Выдаём награду
    player.Currency += achievement.RewardCurrency;
    player.Experience += achievement.RewardExperience;
    playerAch.IsRewardClaimed = true;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        player.Currency,
        player.Experience,
        message = $"Награда получена: +{achievement.RewardCurrency} монет, +{achievement.RewardExperience} опыта"
    });
}).RequireAuthorization();

// GET /player/{id}/quest – текущий активный квест (защищён)
app.MapGet("/player/{id:guid}/quest", async (Guid id, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    var player = await db.Players.FindAsync(id);
    if (player == null) return Results.NotFound();

    var allAchievements = await db.Achievements.OrderBy(a => a.Id).ToListAsync();
    var playerProgress = await db.PlayerAchievements
        .Where(pa => pa.PlayerId == id)
        .ToDictionaryAsync(pa => pa.AchievementId, pa => pa);

    Achievement firstIncomplete = null;
    int currentProgress = 0;

    foreach (var ach in allAchievements)
    {
        if (!playerProgress.ContainsKey(ach.Id))
        {
            firstIncomplete = ach;
            currentProgress = 0;
            break;
        }
        var prog = playerProgress[ach.Id];
        if (prog.Progress < ach.RequiredValue)
        {
            firstIncomplete = ach;
            currentProgress = prog.Progress;
            break;
        }
    }

    if (firstIncomplete == null)
        return Results.Ok(new { message = "Все достижения выполнены!" });

    return Results.Ok(new
    {
        firstIncomplete.Id,
        firstIncomplete.Name,
        firstIncomplete.Description,
        RequiredValue = firstIncomplete.RequiredValue,
        CurrentProgress = currentProgress,
        RewardCurrency = firstIncomplete.RewardCurrency,
        RewardExperience = firstIncomplete.RewardExperience
    });
}).RequireAuthorization();

// ========== ДАННЫЕ ИГРОКА ==========

// GET /player/{id} – получение данных игрока (защищён)
app.MapGet("/player/{id:guid}", async (Guid id, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    var player = await db.Players.FindAsync(id);
    if (player == null) return Results.NotFound();

    return Results.Ok(new
    {
        player.Experience,
        player.Currency,
        player.Wins,
        player.Losses,
        PurchasedItems = new List<int>(),      // временно
        UnitUpgrades = new Dictionary<string, int>() // временно
    });
}).RequireAuthorization();

// POST /player/{id}/buy – покупки временно отключены (защищён)
app.MapPost("/player/{id:guid}/buy", (Guid id, BuyRequest req, HttpContext httpContext, AppDbContext db) =>
{
    if (!IsPlayerAuthorized(httpContext, id))
        return Results.Forbid();

    return Results.BadRequest("Покупки временно отключены");
}).RequireAuthorization();

// ========== МАГАЗИН (открытый) ==========

app.MapGet("/shop", async (AppDbContext db) =>
{
    var items = await db.ShopItems.Select(i => new { i.Id, i.Name, i.Price, i.ImagePath }).ToListAsync();
    return Results.Ok(items);
});

// ========== ПРОВЕРКА СЕРВЕРА ==========

app.MapGet("/ping", () => "pong");

app.UseStaticFiles();
app.Run();

// ========== RECORDS ==========

public record RegisterRequest(string Login, string Password);
public record LoginRequest(string Login, string Password);
public record BuyRequest(int ItemId);
public record AddProgressRequest(int Increment);