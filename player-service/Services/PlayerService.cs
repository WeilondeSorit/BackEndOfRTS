using System.Data;
using Dapper;
using BCrypt.Net;

public class PlayerService : IPlayerService
{
    private readonly PlayerDbContext _context;

    public PlayerService(PlayerDbContext context)
    {
        _context = context;
    }

    public async Task<Player> RegisterAsync(string login, string password)
    {
        using var connection = _context.CreateConnection();
        
        // Проверка существования пользователя
        var existing = await connection.QueryFirstOrDefaultAsync<Player>(
            "SELECT * FROM Player WHERE login = @Login", new { Login = login });
        
        if (existing != null)
            throw new Exception("Player already exists");

        // Хеширование пароля
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Создание игрока
        var player = new Player
        {
            Login = login,
            Password = passwordHash,
            Wins = 0,
            Losses = 0,
            CreatedAt = DateTime.UtcNow
        };

        var sql = @"
            INSERT INTO Player (login, password, wins, losses, created_at)
            VALUES (@Login, @Password, @Wins, @Losses, @CreatedAt)
            RETURNING id";

        player.Id = await connection.ExecuteScalarAsync<int>(sql, player);

        // Создание настроек по умолчанию
        await connection.ExecuteAsync(
            "INSERT INTO Settings (player_id, sound_on, volume) VALUES (@PlayerId, true, 100.0)",
            new { PlayerId = player.Id });

        // Создание данных игрока по умолчанию
        await connection.ExecuteAsync(
            "INSERT INTO player_data (player_id, units, food, wood, rock) VALUES (@PlayerId, 0, 0, 0, 0)",
            new { PlayerId = player.Id });

        return player;
    }

    public async Task<Player> LoginAsync(string login, string password)
    {
        using var connection = _context.CreateConnection();
        
        var player = await connection.QueryFirstOrDefaultAsync<Player>(
            "SELECT * FROM Player WHERE login = @Login", new { Login = login });
        
        if (player == null)
            throw new Exception("Player not found");

        if (!BCrypt.Net.BCrypt.Verify(password, player.Password))
            throw new Exception("Invalid password");

        return player;
    }

    public async Task<Player> GetPlayerAsync(int id)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Player>(
            "SELECT * FROM Player WHERE id = @Id", new { Id = id });
    }

    public async Task<PlayerData> GetPlayerDataAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PlayerData>(
            "SELECT * FROM player_data WHERE player_id = @PlayerId", new { PlayerId = playerId });
    }

    public async Task<PlayerData> UpdatePlayerDataAsync(int playerId, PlayerData data)
    {
        using var connection = _context.CreateConnection();
        
        var sql = @"
            UPDATE player_data 
            SET units = @Units, food = @Food, wood = @Wood, rock = @Rock, updated_at = @UpdatedAt
            WHERE player_id = @PlayerId
            RETURNING *";
        
        data.UpdatedAt = DateTime.UtcNow;
        
        return await connection.QueryFirstOrDefaultAsync<PlayerData>(sql, new {
            PlayerId = playerId,
            data.Units,
            data.Food,
            data.Wood,
            data.Rock,
            data.UpdatedAt
        });
    }

    public async Task<Settings> GetSettingsAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Settings>(
            "SELECT * FROM Settings WHERE player_id = @PlayerId", new { PlayerId = playerId });
    }

    public async Task<Settings> UpdateSettingsAsync(int playerId, Settings settings)
    {
        using var connection = _context.CreateConnection();
        
        var sql = @"
            UPDATE Settings 
            SET sound_on = @SoundOn, volume = @Volume
            WHERE player_id = @PlayerId
            RETURNING *";
        
        return await connection.QueryFirstOrDefaultAsync<Settings>(sql, new {
            PlayerId = playerId,
            settings.SoundOn,
            settings.Volume
        });
    }

    public async Task<List<Achievement>> GetAchievementsAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        var achievements = await connection.QueryAsync<Achievement>(
            "SELECT * FROM Achievement WHERE player_id = @PlayerId", new { PlayerId = playerId });
        
        return achievements.ToList();
    }

    public async Task<Achievement> UnlockAchievementAsync(int playerId, string achievementName)
    {
        using var connection = _context.CreateConnection();
        
        var existing = await connection.QueryFirstOrDefaultAsync<Achievement>(
            "SELECT * FROM Achievement WHERE player_id = @PlayerId AND achievement_name = @AchievementName",
            new { PlayerId = playerId, AchievementName = achievementName });
        
        if (existing != null)
        {
            // Обновляем существующее достижение
            var sql = @"
                UPDATE Achievement 
                SET is_achieved = true, achieved_at = @AchievedAt
                WHERE id = @Id
                RETURNING *";
            
            return await connection.QueryFirstOrDefaultAsync<Achievement>(sql, new {
                existing.Id,
                AchievedAt = DateTime.UtcNow
            });
        }
        else
        {
            // Создаем новое достижение
            var sql = @"
                INSERT INTO Achievement (player_id, achievement_name, is_achieved, achieved_at)
                VALUES (@PlayerId, @AchievementName, true, @AchievedAt)
                RETURNING *";
            
            return await connection.QueryFirstOrDefaultAsync<Achievement>(sql, new {
                PlayerId = playerId,
                AchievementName = achievementName,
                AchievedAt = DateTime.UtcNow
            });
        }
    }

    public async Task UpdateStatsAsync(int playerId, bool isWin)
    {
        using var connection = _context.CreateConnection();
        
        if (isWin)
        {
            await connection.ExecuteAsync(
                "UPDATE Player SET wins = wins + 1 WHERE id = @PlayerId",
                new { PlayerId = playerId });
        }
        else
        {
            await connection.ExecuteAsync(
                "UPDATE Player SET losses = losses + 1 WHERE id = @PlayerId",
                new { PlayerId = playerId });
        }
    }

    public async Task SaveGameProgressAsync(int playerId, GameProgress progress)
    {
        using var connection = _context.CreateConnection();
        
        // Сохраняем ресурсы
        if (progress.Resources != null)
        {
            await UpdatePlayerDataAsync(playerId, progress.Resources);
        }
        
        // Сохраняем юнитов (очищаем старые)
        await connection.ExecuteAsync(
            "DELETE FROM Unit WHERE player_id = @PlayerId",
            new { PlayerId = playerId });
        
        if (progress.Units != null && progress.Units.Any())
        {
            foreach (var unit in progress.Units)
            {
                unit.PlayerId = playerId;
                await connection.ExecuteAsync(@"
                    INSERT INTO Unit (player_id, unit_type, coord_x, coord_y, current_health, max_health, level, created_at)
                    VALUES (@PlayerId, @UnitType, @CoordX, @CoordY, @CurrentHealth, @MaxHealth, @Level, @CreatedAt)",
                    unit);
            }
        }
        
        // Сохраняем здания (очищаем старые)
        await connection.ExecuteAsync(
            "DELETE FROM Building WHERE player_id = @PlayerId",
            new { PlayerId = playerId });
        
        if (progress.Buildings != null && progress.Buildings.Any())
        {
            foreach (var building in progress.Buildings)
            {
                building.PlayerId = playerId;
                await connection.ExecuteAsync(@"
                    INSERT INTO Building (player_id, building_type, coord_x, coord_y, current_health, max_health, level, built_at)
                    VALUES (@PlayerId, @BuildingType, @CoordX, @CoordY, @CurrentHealth, @MaxHealth, @Level, @BuiltAt)",
                    building);
            }
        }
    }

    public async Task<GameProgress> LoadGameProgressAsync(int playerId)
    {
        using var connection = _context.CreateConnection();
        
        var resources = await GetPlayerDataAsync(playerId);
        
        var units = await connection.QueryAsync<Unit>(
            "SELECT * FROM Unit WHERE player_id = @PlayerId",
            new { PlayerId = playerId });
        
        var buildings = await connection.QueryAsync<Building>(
            "SELECT * FROM Building WHERE player_id = @PlayerId",
            new { PlayerId = playerId });
        
        return new GameProgress
        {
            Resources = resources,
            Units = units.ToList(),
            Buildings = buildings.ToList()
        };
    }
}