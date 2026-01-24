using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public ProgressController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet("{playerId}/data")]
    public async Task<IActionResult> GetPlayerData(int playerId)
    {
        var data = await _playerService.GetPlayerDataAsync(playerId);
        if (data == null)
            return NotFound(new { error = "Player data not found" });
        
        return Ok(data);
    }

    [HttpPut("{playerId}/data")]
    public async Task<IActionResult> UpdatePlayerData(int playerId, [FromBody] PlayerData data)
    {
        try
        {
            var updated = await _playerService.UpdatePlayerDataAsync(playerId, data);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{playerId}/save-progress")]
    public async Task<IActionResult> SaveGameProgress(int playerId, [FromBody] GameProgress progress)
    {
        try
        {
            await _playerService.SaveGameProgressAsync(playerId, progress);
            return Ok(new { message = "Game progress saved" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{playerId}/load-progress")]
    public async Task<IActionResult> LoadGameProgress(int playerId)
    {
        try
        {
            var progress = await _playerService.LoadGameProgressAsync(playerId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}