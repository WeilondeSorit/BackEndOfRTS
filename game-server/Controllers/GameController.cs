using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    // Тестовый метод
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { 
            message = "Game server is working!",
            status = "OK",
            timestamp = DateTime.UtcNow,
            port = "8080"
        });
    }
    
    // Простой метод для сохранения (упрощенный)
    [HttpPost("save")]
    public IActionResult SaveGame([FromBody] object gameData)
    {
        return Ok(new { 
            success = true, 
            message = "Game saved!",
            received = DateTime.UtcNow
        });
    }
    
    // Простой метод для загрузки
    [HttpGet("load/{playerId}")]
    public IActionResult LoadGame(string playerId)
    {
        return Ok(new { 
            playerId = playerId,
            playerName = "TestPlayer",
            units = 100,
            food = 50,
            wood = 75,
            rock = 25,
            buildings = new[] {
                new { id = "1", buildingType = "Farm", coordX = 10, coordY = 10 }
            }
        });
    }
}