using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public AuthController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var player = await _playerService.RegisterAsync(dto.Login, dto.Password);
            return Ok(new { 
                message = "Player registered successfully",
                playerId = player.Id,
                login = player.Login
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var player = await _playerService.LoginAsync(dto.Login, dto.Password);
            return Ok(new {
                playerId = player.Id,
                login = player.Login,
                wins = player.Wins,
                losses = player.Losses
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}

public class RegisterDto
{
    public string Login { get; set; }
    public string Password { get; set; }
}

public class LoginDto
{
    public string Login { get; set; }
    public string Password { get; set; }
}