using System;
namespace SessionServer.Models{ 
public class SessionData
{
    public Guid PlayerId { get; set; }
    public int Wood { get; set; } = 100;
    public int Stone { get; set; } = 100;
    public int Food { get; set; } = 100;
    public string UnitsJson { get; set; } = "{}";// позиции юнитов
    public int TowerHp { get; set; } = 1000;
    public int EnemyHp { get; set; } = 1000;
}
}