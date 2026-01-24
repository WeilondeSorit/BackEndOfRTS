public class Player
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Settings
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public bool SoundOn { get; set; }
    public decimal Volume { get; set; }
}

public class Achievement
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string AchievementName { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime? AchievedAt { get; set; }
}

public class PlayerData
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int Units { get; set; }
    public int Food { get; set; }
    public int Wood { get; set; }
    public int Rock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Unit
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string UnitType { get; set; }
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Level { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Building
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string BuildingType { get; set; }
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Level { get; set; }
    public DateTime BuiltAt { get; set; }
}