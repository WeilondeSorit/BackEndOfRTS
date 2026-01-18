using StackExchange.Redis;
using Newtonsoft.Json;
using System.Text;

public class GameDbContext
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<GameDbContext> _logger;

    public GameDbContext(IConnectionMultiplexer redis, ILogger<GameDbContext> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    // Ключи Redis
    private string PlayerKey(string playerId) => $"player:{playerId}";
    private string UnitsKey(string playerId) => $"player:{playerId}:units";
    private string BuildingsKey(string playerId) => $"player:{playerId}:buildings";
    private string MapKey(string playerId) => $"player:{playerId}:map";

    // Сохранение игрового состояния
    public async Task<bool> SaveGameStateAsync(string playerId, GameState state)
    {
        try
        {
            // Сохраняем данные игрока
            await _db.StringSetAsync(PlayerKey(playerId), 
                JsonConvert.SerializeObject(state.PlayerData), 
                TimeSpan.FromHours(24));

            // Сохраняем юнитов
            if (state.Units != null)
            {
                await _db.StringSetAsync(UnitsKey(playerId),
                    JsonConvert.SerializeObject(state.Units),
                    TimeSpan.FromHours(24));
            }

            // Сохраняем здания
            if (state.Buildings != null)
            {
                await _db.StringSetAsync(BuildingsKey(playerId),
                    JsonConvert.SerializeObject(state.Buildings),
                    TimeSpan.FromHours(24));
            }

            // Сохраняем карту
            if (state.Map != null)
            {
                await _db.StringSetAsync(MapKey(playerId),
                    JsonConvert.SerializeObject(state.Map),
                    TimeSpan.FromHours(24));
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving game state for player {PlayerId}", playerId);
            return false;
        }
    }

    // Загрузка игрового состояния
    public async Task<GameState> LoadGameStateAsync(string playerId)
    {
        try
        {
            var playerDataJson = await _db.StringGetAsync(PlayerKey(playerId));
            var unitsJson = await _db.StringGetAsync(UnitsKey(playerId));
            var buildingsJson = await _db.StringGetAsync(BuildingsKey(playerId));
            var mapJson = await _db.StringGetAsync(MapKey(playerId));

            return new GameState
            {
                PlayerData = !playerDataJson.IsNullOrEmpty 
                    ? JsonConvert.DeserializeObject<PlayerDataEntity>(playerDataJson) 
                    : null,
                Units = !unitsJson.IsNullOrEmpty 
                    ? JsonConvert.DeserializeObject<List<UnitEntity>>(unitsJson) 
                    : new List<UnitEntity>(),
                Buildings = !buildingsJson.IsNullOrEmpty 
                    ? JsonConvert.DeserializeObject<List<BuildingEntity>>(buildingsJson) 
                    : new List<BuildingEntity>(),
                Map = !mapJson.IsNullOrEmpty 
                    ? JsonConvert.DeserializeObject<GameMap>(mapJson) 
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game state for player {PlayerId}", playerId);
            return null;
        }
    }

    // Удаление всех данных игрока (при завершении игры)
    public async Task<bool> DeleteGameStateAsync(string playerId)
    {
        try
        {
            var keys = new RedisKey[]
            {
                PlayerKey(playerId),
                UnitsKey(playerId),
                BuildingsKey(playerId),
                MapKey(playerId)
            };

            await _db.KeyDeleteAsync(keys);
            _logger.LogInformation("Deleted game state for player {PlayerId}", playerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game state for player {PlayerId}", playerId);
            return false;
        }
    }

    // Проверка существования игры
    public async Task<bool> GameExistsAsync(string playerId)
    {
        return await _db.KeyExistsAsync(PlayerKey(playerId));
    }
}

// Модель полного игрового состояния
public class GameState
{
    public PlayerDataEntity PlayerData { get; set; }
    public List<UnitEntity> Units { get; set; }
    public List<BuildingEntity> Buildings { get; set; }
    public GameMap Map { get; set; }
}

// Модели для Unity клиента
public class UnitEntity
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string UnitType { get; set; } // "Villager", "Archer", "Healer"
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public Dictionary<string, object> Properties { get; set; } // Для специфических свойств
}

public class GameMap
{
    public string MapName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<MapCell> Cells { get; set; }
}

public class MapCell
{
    public int X { get; set; }
    public int Y { get; set; }
    public string TerrainType { get; set; }
    public string ResourceType { get; set; }
    public int ResourceAmount { get; set; }
    public bool IsPassable { get; set; }
}