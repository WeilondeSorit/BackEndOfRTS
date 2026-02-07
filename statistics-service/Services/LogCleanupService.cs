using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StatisticsService.Data;

namespace StatisticsService.Services
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LogCleanupService> _logger;
        private readonly int _retentionDays;

        public LogCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<LogCleanupService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _retentionDays = configuration.GetValue<int>("Logging:RetentionDays", 7);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Log cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<StatisticsDbContext>();

                    var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

                    // Delete old server logs
                    var oldServerLogs = await dbContext.ServerLogs
                        .Where(l => l.Timestamp < cutoffDate)
                        .ToListAsync(stoppingToken);

                    dbContext.ServerLogs.RemoveRange(oldServerLogs);

                    // Delete old error logs
                    var oldErrorLogs = await dbContext.ErrorLogs
                        .Where(l => l.Timestamp < cutoffDate)
                        .ToListAsync(stoppingToken);

                    dbContext.ErrorLogs.RemoveRange(oldErrorLogs);

                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Cleaned up {ServerLogsCount} server logs and {ErrorLogsCount} error logs older than {RetentionDays} days",
                        oldServerLogs.Count, oldErrorLogs.Count, _retentionDays);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during log cleanup");
                }

                // Run cleanup once per day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Log cleanup service stopped");
            await base.StopAsync(stoppingToken);
        }
    }
}