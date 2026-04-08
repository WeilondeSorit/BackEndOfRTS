using Microsoft.EntityFrameworkCore;
using MainServer.Models;
namespace MainServer.Data    // <-- обязательно MainServer.Data
{
public class AppDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<ShopItem> ShopItems { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
}