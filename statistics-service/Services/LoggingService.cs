using StatisticsService.Data;
using StatisticsService.Models;

namespace StatisticsService.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly StatisticsDbContext _context;

        public LoggingService(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task LogInfoAsync(string message, string serviceName)
        {
            await LogAsync("Info", message, serviceName);
        }

        public async Task LogWarningAsync(string message, string serviceName)
        {
            await LogAsync("Warning", message, serviceName);
        }

        public async Task LogErrorAsync(string errorMessage, string serviceName, string? stackTrace = null, string? endpoint = null)
        {
            var errorLog = new ErrorLog
            {
                Timestamp = DateTime.UtcNow,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace ?? string.Empty,
                ServiceName = serviceName,
                Endpoint = endpoint
            };

            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();

            await LogAsync("Error", errorMessage, serviceName);
        }

        public async Task LogCriticalAsync(string errorMessage, string serviceName, string? stackTrace = null)
        {
            var errorLog = new ErrorLog
            {
                Timestamp = DateTime.UtcNow,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace ?? string.Empty,
                ServiceName = serviceName
            };

            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();

            await LogAsync("Critical", errorMessage, serviceName);
        }

        private async Task LogAsync(string level, string message, string serviceName)
        {
            var log = new ServerLog
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                ServiceName = serviceName
            };

            _context.ServerLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}