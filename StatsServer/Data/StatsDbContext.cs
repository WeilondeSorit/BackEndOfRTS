using Microsoft.EntityFrameworkCore;
using StatsServer.Models;

namespace StatsServer.Data
{
    public class StatsDbContext : DbContext
    {
        public StatsDbContext(DbContextOptions<StatsDbContext> options) : base(options) { }
        
        public DbSet<Player> Players { get; set; }
    }
}