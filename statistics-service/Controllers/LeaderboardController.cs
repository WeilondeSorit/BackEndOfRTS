using Microsoft.AspNetCore.Mvc;
using StatisticsService.Models;
using StatisticsService.Services;

namespace StatisticsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get global leaderboard (public access)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard(
            [FromQuery] int limit = 100,
            [FromQuery] int offset = 0)
        {
            try
            {
                var leaderboard = await _leaderboardService.GetGlobalLeaderboardAsync(limit, offset);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get top players (public access)
        /// </summary>
        [HttpGet("top")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LeaderboardEntry>>> GetTopPlayers([FromQuery] int count = 10)
        {
            try
            {
                var topPlayers = await _leaderboardService.GetTopPlayersAsync(count);
                return Ok(topPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top players");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get player's rank and stats (public access)
        /// </summary>
        [HttpGet("player/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeaderboardEntry>> GetPlayerRank(Guid playerId)
        {
            try
            {
                var playerRank = await _leaderboardService.GetPlayerRankAsync(playerId);

                if (playerRank == null)
                    return NotFound(new { error = "Player not found" });

                return Ok(playerRank);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching player rank");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get total number of players (public access)
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetTotalPlayers()
        {
            try
            {
                var count = await _leaderboardService.GetTotalPlayersAsync();
                return Ok(new { totalPlayers = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching total players count");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}