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

// Альтернативный способ чтения строки подключения (работает всегда)
var connectionString = builder.Configuration["ConnectionStrings:Postgres"];
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

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
        Losses = 0,
        PurchasedItemsJson = "[]",
        UnitUpgradesJson = "{}"
    };
    db.Players.Add(player);
    await db.SaveChangesAsync();
    return Results.Ok(new { player.Id });
});

// Вход
app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var player = await db.Players.FirstOrDefaultAsync(p => p.Login == req.Login && p.PasswordHash == req.Password);
    if (player == null) return Results.Unauthorized();
    return Results.Ok(new { player.Id });
});

// Получить данные игрока
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
        PurchasedItems = JsonSerializer.Deserialize<List<int>>(player.PurchasedItemsJson),
        UnitUpgrades = JsonSerializer.Deserialize<Dictionary<string, int>>(player.UnitUpgradesJson)
    });
});

// Купить предмет
app.MapPost("/player/{id:guid}/buy", async (Guid id, BuyRequest req, AppDbContext db) =>
{
    var player = await db.Players.FindAsync(id);
    var item = await db.ShopItems.FindAsync(req.ItemId);
    if (player == null || item == null) return Results.NotFound();
    if (player.Currency < item.Price) return Results.BadRequest("Not enough currency");
    var purchased = JsonSerializer.Deserialize<List<int>>(player.PurchasedItemsJson);
    if (purchased.Contains(item.Id)) return Results.BadRequest("Already bought");
    purchased.Add(item.Id);
    player.PurchasedItemsJson = JsonSerializer.Serialize(purchased);
    player.Currency -= item.Price;
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Список магазина
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