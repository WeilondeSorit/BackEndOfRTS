using Microsoft.EntityFrameworkCore;
using MainServer.Models;

namespace MainServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<ShopItem> ShopItems { get; set; } = null!;
        public DbSet<PurchasedItem> PurchasedItems { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Unit> Units { get; set; } = null!;
        public DbSet<PlayerUnitUpgrade> PlayerUnitUpgrades { get; set; } = null!;
        public DbSet<Building> Buildings { get; set; } = null!;
        public DbSet<PlayerBuildingUpgrade> PlayerBuildingUpgrades { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;
        public DbSet<PlayerAchievement> PlayerAchievements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Имена таблиц
            modelBuilder.Entity<Player>().ToTable("Players");
            modelBuilder.Entity<ShopItem>().ToTable("ShopItems");
            modelBuilder.Entity<PurchasedItem>().ToTable("PurchasedItems");
            modelBuilder.Entity<Match>().ToTable("Matches");
            modelBuilder.Entity<Unit>().ToTable("Units");
            modelBuilder.Entity<PlayerUnitUpgrade>().ToTable("PlayerUnitUpgrades");
            modelBuilder.Entity<Building>().ToTable("Buildings");
            modelBuilder.Entity<PlayerBuildingUpgrade>().ToTable("PlayerBuildingUpgrades");
            modelBuilder.Entity<Achievement>().ToTable("Achievements");
            modelBuilder.Entity<PlayerAchievement>().ToTable("PlayerAchievements");

            // Составные ключи
            modelBuilder.Entity<PurchasedItem>().HasKey(p => new { p.PlayerId, p.ItemId });
            modelBuilder.Entity<PlayerUnitUpgrade>().HasKey(p => new { p.PlayerId, p.UnitId });
            modelBuilder.Entity<PlayerBuildingUpgrade>().HasKey(p => new { p.PlayerId, p.BuildingId });
            modelBuilder.Entity<PlayerAchievement>().HasKey(p => new { p.PlayerId, p.AchievementId });

            // Связи многие-ко-многим (игрок ←→ товар)
            modelBuilder.Entity<PurchasedItem>()
                .HasOne(pi => pi.Player)
                .WithMany(p => p.PurchasedItems)
                .HasForeignKey(pi => pi.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedItem>()
                .HasOne(pi => pi.Item)
                .WithMany(si => si.PurchasedItems)
                .HasForeignKey(pi => pi.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Игрок ←→ юнит
            modelBuilder.Entity<PlayerUnitUpgrade>()
                .HasOne(pu => pu.Player)
                .WithMany(p => p.PlayerUnitUpgrades)
                .HasForeignKey(pu => pu.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerUnitUpgrade>()
                .HasOne(pu => pu.Unit)
                .WithMany(u => u.PlayerUnitUpgrades)
                .HasForeignKey(pu => pu.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Игрок ←→ здание
            modelBuilder.Entity<PlayerBuildingUpgrade>()
                .HasOne(pb => pb.Player)
                .WithMany(p => p.PlayerBuildingUpgrades)
                .HasForeignKey(pb => pb.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerBuildingUpgrade>()
                .HasOne(pb => pb.Building)
                .WithMany(b => b.PlayerBuildingUpgrades)
                .HasForeignKey(pb => pb.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Игрок ←→ достижение
            modelBuilder.Entity<PlayerAchievement>()
                .HasOne(pa => pa.Player)
                .WithMany(p => p.PlayerAchievements)
                .HasForeignKey(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerAchievement>()
                .HasOne(pa => pa.Achievement)
                .WithMany(a => a.PlayerAchievements)
                .HasForeignKey(pa => pa.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Матчи (один-к-многим)
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player)
                .WithMany(p => p.Matches)
                .HasForeignKey(m => m.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}