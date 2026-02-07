using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatisticsService.Services;

namespace StatisticsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoggingController : ControllerBase
    {
        private readonly ILoggingService _loggingService;
        private readonly ILogger<LoggingController> _logger;

        public LoggingController(ILoggingService loggingService, ILogger<LoggingController> logger)
        {
            _loggingService = loggingService;
            _logger = logger;
        }

        /// <summary>
        /// Log an error from another service
        /// </summary>
        [HttpPost("error")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> LogError([FromBody] ErrorLogRequest request)
        {
            try
            {
                await _loggingService.LogErrorAsync(
                    request.ErrorMessage,
                    request.ServiceName,
                    request.StackTrace,
                    request.Endpoint);

                return Ok(new { message = "Error logged successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogError endpoint");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Log a warning from another service
        /// </summary>
        [HttpPost("warning")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> LogWarning([FromBody] LogRequest request)
        {
            try
            {
                await _loggingService.LogWarningAsync(request.Message, request.ServiceName);
                return Ok(new { message = "Warning logged successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogWarning endpoint");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Log an info message from another service
        /// </summary>
        [HttpPost("info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> LogInfo([FromBody] LogRequest request)
        {
            try
            {
                await _loggingService.LogInfoAsync(request.Message, request.ServiceName);
                return Ok(new { message = "Info logged successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogInfo endpoint");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class ErrorLogRequest
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? Endpoint { get; set; }
    }

    public class LogRequest
    {
        public string Message { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }
}