using Microsoft.EntityFrameworkCore;
using StatsServer.Models;  // у нас будет модель Player

namespace StatsServer.Data
{
    public class StatsDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        
        public StatsDbContext(DbContextOptions<StatsDbContext> options) : base(options) { }
    }
}