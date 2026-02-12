using Microsoft.EntityFrameworkCore;
using StatisticsService.Models;

namespace StatisticsService.Data
{
    public class StatisticsDbContext : DbContext
    {
        public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : base(options) { }

        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<MatchResult> MatchResults { get; set; }
        public DbSet<ServerLog> ServerLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("statistics");

            // Явно указываем имена таблиц в формате snake_case
            modelBuilder.Entity<PlayerStats>().ToTable("player_stats");
            modelBuilder.Entity<MatchResult>().ToTable("match_results");
            modelBuilder.Entity<ServerLog>().ToTable("server_logs");
            modelBuilder.Entity<ErrorLog>().ToTable("error_logs");

            // PlayerStats configuration
            modelBuilder.Entity<PlayerStats>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Wins).HasDefaultValue(0);
                entity.Property(e => e.Losses).HasDefaultValue(0);
                entity.Property(e => e.TotalMatches).HasDefaultValue(0);
                entity.Property(e => e.Kills).HasDefaultValue(0);
                entity.Property(e => e.Deaths).HasDefaultValue(0);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasIndex(e => e.Wins).IsDescending();
                entity.HasIndex(e => e.PlayerId).IsUnique();
            });

            // MatchResults configuration
            modelBuilder.Entity<MatchResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MatchId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.IsWin).IsRequired();
                entity.Property(e => e.MatchDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DurationSeconds).IsRequired();
                entity.Property(e => e.UnitsKilled).HasDefaultValue(0);
                entity.Property(e => e.UnitsLost).HasDefaultValue(0);
                entity.Property(e => e.BaseDestroyed).HasDefaultValue(false);
                
                entity.HasIndex(e => e.PlayerId);
                entity.HasIndex(e => e.MatchDate);
            });

            // ServerLogs configuration
            modelBuilder.Entity<ServerLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.ServiceName).HasMaxLength(50);
                entity.Property(e => e.StackTrace);
                
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Level);
            });

            // ErrorLogs configuration
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ErrorMessage).IsRequired();
                entity.Property(e => e.StackTrace).IsRequired();
                entity.Property(e => e.ServiceName).HasMaxLength(50);
                entity.Property(e => e.Endpoint).HasMaxLength(200);
                
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ServiceName);
            });
        }
    }
}