using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Logging;

/// <summary>
/// Serilog-based implementation of ILoggingService
/// Provides structured logging with swappable implementation following SOLID principles
/// </summary>
public class SerilogLoggingService : ILoggingService
{
    private readonly ILogger<SerilogLoggingService> _logger;

    public SerilogLoggingService(ILogger<SerilogLoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public IDisposable BeginScope(string messageFormat, params object[] args)
    {
        return _logger.BeginScope(messageFormat, args);
    }
}