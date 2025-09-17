using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Services;

/// <summary>
/// Repository interface for HL7 transmission audit logging
/// Provides data persistence abstraction for transmission history and compliance
/// </summary>
public interface IHL7TransmissionRepository
{
    /// <summary>
    /// Saves a transmission log entry to persistent storage
    /// </summary>
    /// <param name="log">Transmission log entry to save</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The saved transmission log with any generated identifiers</returns>
    /// <exception cref="ArgumentNullException">Thrown when log is null</exception>
    Task<HL7TransmissionLog> SaveTransmissionLogAsync(
        HL7TransmissionLog log,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific transmission log by its unique identifier
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Transmission log if found, null otherwise</returns>
    Task<HL7TransmissionLog?> GetTransmissionLogAsync(
        string transmissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves transmission history filtered by criteria
    /// </summary>
    /// <param name="patientId">Optional patient identifier filter</param>
    /// <param name="from">Optional start date filter</param>
    /// <param name="to">Optional end date filter</param>
    /// <param name="protocol">Optional protocol filter</param>
    /// <param name="success">Optional success status filter</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of transmission logs matching the criteria</returns>
    Task<IEnumerable<HL7TransmissionLog>> GetTransmissionHistoryAsync(
        string? patientId = null,
        DateTime? from = null,
        DateTime? to = null,
        TransmissionProtocol? protocol = null,
        bool? success = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transmission statistics for a specific time period
    /// </summary>
    /// <param name="from">Start date for statistics</param>
    /// <param name="to">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Transmission statistics</returns>
    Task<TransmissionStatistics> GetTransmissionStatisticsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old transmission logs based on retention policy
    /// </summary>
    /// <param name="retentionDays">Number of days to retain logs</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Number of logs deleted</returns>
    Task<int> DeleteOldLogsAsync(
        int retentionDays,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about HL7 transmissions for a specific time period
/// </summary>
public record TransmissionStatistics(
    int TotalTransmissions,
    int SuccessfulTransmissions,
    int FailedTransmissions,
    double SuccessRate,
    TimeSpan AverageResponseTime,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Dictionary<TransmissionProtocol, int> TransmissionsByProtocol);
