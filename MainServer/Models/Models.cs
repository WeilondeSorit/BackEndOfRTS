using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Models
{
public class Player
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Experience { get; set; }
    public int Currency { get; set; } = 100;
    public int Wins { get; set; }
    public int Losses { get; set; }

    public virtual ICollection<PurchasedItem> PurchasedItems { get; set; } = new List<PurchasedItem>();
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<PlayerUnitUpgrade> PlayerUnitUpgrades { get; set; } = new List<PlayerUnitUpgrade>();        
    public virtual ICollection<PlayerBuildingUpgrade> PlayerBuildingUpgrades { get; set; } = new List<PlayerBuildingUpgrade>();
    public virtual ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
}

    public class ShopItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public virtual ICollection<PurchasedItem> PurchasedItems { get; set; } = new List<PurchasedItem>();
    }

    public class PurchasedItem
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public int ItemId { get; set; }
        public virtual ShopItem Item { get; set; } = null!;
    }

    public class Match
    {
        public long Id { get; set; }
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public bool IsWin { get; set; }
        public int ExperienceGained { get; set; }
        public int CurrencyGained { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BaseHealth { get; set; }
        public int BaseDamage { get; set; }
        public virtual ICollection<PlayerUnitUpgrade> PlayerUnitUpgrades { get; set; } = new List<PlayerUnitUpgrade>();
    }

    public class PlayerUnitUpgrade
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; } = null!;
        public int Level { get; set; } = 1;
    }

    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BaseHp { get; set; }
        public int BaseProduction { get; set; }
        public int UpgradeCost { get; set; }
        public virtual ICollection<PlayerBuildingUpgrade> PlayerBuildingUpgrades { get; set; } = new List<PlayerBuildingUpgrade>();
    }

    public class PlayerBuildingUpgrade
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public int BuildingId { get; set; }
        public virtual Building Building { get; set; } = null!;
        public int Level { get; set; } = 1;
    }

    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RequiredValue { get; set; }
        public int RewardCurrency { get; set; }
        public int RewardExperience { get; set; }
        public string Key { get; set; }
        public virtual ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
    }

    public class PlayerAchievement
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public int AchievementId { get; set; }
        public virtual Achievement Achievement { get; set; } = null!;
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
        public int Progress { get; set; }
        public bool IsRewardClaimed { get; set; }
    }

   
}