using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StatisticsService.Data;
using StatisticsService.Models;
using StatisticsService.Services;

namespace StatisticsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly StatisticsDbContext _context;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(
            StatisticsDbContext context,
            ILeaderboardService leaderboardService,
            ILogger<StatsController> logger)
        {
            _context = context;
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        /// <summary>
        /// Record match result
        /// </summary>
        [HttpPost("match")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RecordMatchResult([FromBody] MatchResult result)
        {
            try
            {
                if (result.PlayerId == Guid.Empty)
                    return BadRequest(new { error = "Invalid player ID" });

                _context.MatchResults.Add(result);
                await _context.SaveChangesAsync();

                // Update player stats
                await _leaderboardService.UpdatePlayerStatsAsync(
                    result.PlayerId,
                    result.PlayerId.ToString(),
                    result.IsWin,
                    result.UnitsKilled);

                return Ok(new { message = "Match result recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording match result");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get player's match history
        /// </summary>
        [HttpGet("history/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MatchResult>>> GetMatchHistory(Guid playerId, [FromQuery] int limit = 20)
        {
            try
            {
                var history = await _context.MatchResults
                    .Where(m => m.PlayerId == playerId)
                    .OrderByDescending(m => m.MatchDate)
                    .Take(limit)
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching match history");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}