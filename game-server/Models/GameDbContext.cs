using System.Data;
using Dapper;
using Npgsql;

public class GameDbContext
{
    private readonly string _connectionString;

    public GameDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Метод для получения подключения
    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    // Метод для создания таблиц
    public void InitializeDatabase()
    {
        using var connection = CreateConnection();
        
        // Таблица игроков
        var playerTableSql = @"
            CREATE TABLE IF NOT EXISTS player_data (
                player_id VARCHAR(50) PRIMARY KEY,
                player_name VARCHAR(100),
                units INTEGER DEFAULT 0,
                food INTEGER DEFAULT 0,
                wood INTEGER DEFAULT 0,
                rock INTEGER DEFAULT 0,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )";
        
        // Таблица зданий
        var buildingTableSql = @"
            CREATE TABLE IF NOT EXISTS buildings (
                id VARCHAR(50) PRIMARY KEY,
                player_id VARCHAR(50) REFERENCES player_data(player_id) ON DELETE CASCADE,
                building_type VARCHAR(50),
                coord_x INTEGER,
                coord_y INTEGER,
                current_health INTEGER DEFAULT 100,
                max_health INTEGER DEFAULT 100,
                level INTEGER DEFAULT 1,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )";
        
        // Создаем таблицы
        connection.Execute(playerTableSql);
        connection.Execute(buildingTableSql);
        
        // Создаем индекс
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_buildings_player ON buildings(player_id)");
    }
}