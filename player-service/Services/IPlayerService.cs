public interface IPlayerService
{
    Task<Player> RegisterAsync(string login, string password);
    Task<Player> LoginAsync(string login, string password);
    Task<Player> GetPlayerAsync(int id);
    Task<PlayerData> GetPlayerDataAsync(int playerId);
    Task<PlayerData> UpdatePlayerDataAsync(int playerId, PlayerData data);
    Task<Settings> GetSettingsAsync(int playerId);
    Task<Settings> UpdateSettingsAsync(int playerId, Settings settings);
    Task<List<Achievement>> GetAchievementsAsync(int playerId);
    Task<Achievement> UnlockAchievementAsync(int playerId, string achievementName);
    Task UpdateStatsAsync(int playerId, bool isWin);
    Task SaveGameProgressAsync(int playerId, GameProgress progress);
    Task<GameProgress> LoadGameProgressAsync(int playerId);
}

public class GameProgress
{
    public List<Unit> Units { get; set; }
    public List<Building> Buildings { get; set; }
    public PlayerData Resources { get; set; }
}