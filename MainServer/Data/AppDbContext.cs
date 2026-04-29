using Microsoft.EntityFrameworkCore;
using MainServer.Models;

namespace MainServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }
    }
}