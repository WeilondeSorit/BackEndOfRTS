using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayerController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _playerService.GetPlayerAsync(id);
        if (player == null)
            return NotFound(new { error = "Player not found" });
        
        return Ok(new {
            player.Id,
            player.Login,
            player.Wins,
            player.Losses,
            player.CreatedAt
        });
    }

    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(int id)
    {
        var player = await _playerService.GetPlayerAsync(id);
        if (player == null)
            return NotFound(new { error = "Player not found" });
        
        return Ok(new {
            wins = player.Wins,
            losses = player.Losses
        });
    }

    [HttpPost("{id}/update-stats")]
    public async Task<IActionResult> UpdateStats(int id, [FromBody] UpdateStatsDto dto)
    {
        try
        {
            await _playerService.UpdateStatsAsync(id, dto.IsWin);
            return Ok(new { message = "Stats updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/settings")]
    public async Task<IActionResult> GetSettings(int id)
    {
        var settings = await _playerService.GetSettingsAsync(id);
        if (settings == null)
            return NotFound(new { error = "Settings not found" });
        
        return Ok(settings);
    }

    [HttpPut("{id}/settings")]
    public async Task<IActionResult> UpdateSettings(int id, [FromBody] Settings settings)
    {
        try
        {
            var updated = await _playerService.UpdateSettingsAsync(id, settings);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/achievements")]
    public async Task<IActionResult> GetAchievements(int id)
    {
        var achievements = await _playerService.GetAchievementsAsync(id);
        return Ok(achievements);
    }

    [HttpPost("{id}/achievements/unlock")]
    public async Task<IActionResult> UnlockAchievement(int id, [FromBody] UnlockAchievementDto dto)
    {
        try
        {
            var achievement = await _playerService.UnlockAchievementAsync(id, dto.AchievementName);
            return Ok(new {
                message = "Achievement unlocked",
                achievement
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class UpdateStatsDto
{
    public bool IsWin { get; set; }
}

public class UnlockAchievementDto
{
    public string AchievementName { get; set; }
}