using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameDbContext _dbContext;
    private readonly ILogger<GameController> _logger;

    public GameController(GameDbContext dbContext, ILogger<GameController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { 
            message = "Game server with Redis is working!",
            status = "OK",
            timestamp = DateTime.UtcNow,
            database = "Redis"
        });
    }

    [HttpPost("save/{playerId}")]
    public async Task<IActionResult> SaveGame(string playerId, [FromBody] GameState gameState)
    {
        if (gameState == null)
            return BadRequest(new { error = "Game state is required" });

        var success = await _dbContext.SaveGameStateAsync(playerId, gameState);
        
        return success 
            ? Ok(new { success = true, message = "Game saved to Redis", playerId })
            : StatusCode(500, new { success = false, message = "Failed to save game" });
    }

    [HttpGet("load/{playerId}")]
    public async Task<IActionResult> LoadGame(string playerId)
    {
        var gameState = await _dbContext.LoadGameStateAsync(playerId);
        
        if (gameState?.PlayerData == null)
            return NotFound(new { error = "Game not found", playerId });

        return Ok(gameState);
    }

    [HttpDelete("delete/{playerId}")]
    public async Task<IActionResult> DeleteGame(string playerId)
    {
        var success = await _dbContext.DeleteGameStateAsync(playerId);
        
        return success 
            ? Ok(new { success = true, message = "Game data deleted from Redis" })
            : StatusCode(500, new { success = false, message = "Failed to delete game" });
    }

    [HttpGet("exists/{playerId}")]
    public async Task<IActionResult> GameExists(string playerId)
    {
        var exists = await _dbContext.GameExistsAsync(playerId);
        return Ok(new { exists });
    }
}