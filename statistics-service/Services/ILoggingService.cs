namespace StatisticsService.Services
{
    public interface ILoggingService
    {
        Task LogInfoAsync(string message, string serviceName);
        Task LogWarningAsync(string message, string serviceName);
        Task LogErrorAsync(string errorMessage, string serviceName, string? stackTrace = null, string? endpoint = null);
        Task LogCriticalAsync(string errorMessage, string serviceName, string? stackTrace = null);
    }
}