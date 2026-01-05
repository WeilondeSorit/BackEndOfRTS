public class BuildingEntity
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string BuildingType { get; set; }
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Level { get; set; }
}