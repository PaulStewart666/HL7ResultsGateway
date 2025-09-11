using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Logging;

/// <summary>
/// Abstraction for logging services following Interface Segregation Principle
/// Provides structured logging capabilities with swappable implementations
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Log information level message with optional structured data
    /// </summary>
    /// <param name="message">The message template</param>
    /// <param name="args">Arguments for the message template</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Log warning level message with optional structured data
    /// </summary>
    /// <param name="message">The message template</param>
    /// <param name="args">Arguments for the message template</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log error level message with exception details
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="message">The message template</param>
    /// <param name="args">Arguments for the message template</param>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Log debug level message with optional structured data
    /// </summary>
    /// <param name="message">The message template</param>
    /// <param name="args">Arguments for the message template</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Begin a logical operation scope for correlation
    /// </summary>
    /// <param name="messageFormat">Format string for the scope</param>
    /// <param name="args">Arguments for the format string</param>
    /// <returns>Disposable scope</returns>
    IDisposable BeginScope(string messageFormat, params object[] args);
}