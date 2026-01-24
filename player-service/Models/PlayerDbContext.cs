using System.Data;
using Dapper;
using Npgsql;

public class PlayerDbContext
{
    private readonly string _connectionString;

    public PlayerDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public void InitializeDatabase()
    {
        using var connection = CreateConnection();
        
        // Таблицы будут созданы из init.sql
        // Здесь можно добавить дополнительную инициализацию если нужно
        
        Console.WriteLine("Database initialized successfully");
    }
}