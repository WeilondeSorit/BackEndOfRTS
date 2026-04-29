using MainServer.Data;
using MainServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Подключение к PostgreSQL
var connectionString = builder.Configuration["ConnectionStrings:Postgres"];
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ✅ Регистрация (без JSON-полей)
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
    return Results.Ok(new { player.Id });
});

// ✅ Логин
app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var player = await db.Players.FirstOrDefaultAsync(p => p.Login == req.Login && p.PasswordHash == req.Password);
    if (player == null) return Results.Unauthorized();
    return Results.Ok(new { player.Id });
});

// ✅ Получение данных игрока (без покупок/улучшений)
app.MapGet("/player/{id:guid}", async (Guid id, AppDbContext db) =>
{
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
});

// ⚠️ Покупки временно отключены (чтобы не ломать остальное)
app.MapPost("/player/{id:guid}/buy", async (Guid id, BuyRequest req, AppDbContext db) =>
{
    return Results.BadRequest("Покупки временно отключены");
});

// ✅ Магазин (работает, если таблица ShopItems существует)
app.MapGet("/shop", async (AppDbContext db) =>
{
    var items = await db.ShopItems.Select(i => new { i.Id, i.Name, i.Price, i.ImagePath }).ToListAsync();
    return Results.Ok(items);
});

app.MapGet("/ping", () => "pong");

app.UseStaticFiles();
app.Run();

public record RegisterRequest(string Login, string Password);
public record LoginRequest(string Login, string Password);
public record BuyRequest(int ItemId);