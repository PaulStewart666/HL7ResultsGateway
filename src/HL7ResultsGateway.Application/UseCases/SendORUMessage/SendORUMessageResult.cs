using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.UseCases.SendORUMessage;

/// <summary>
/// Result of an HL7 ORU message transmission operation
/// Contains comprehensive information about the transmission outcome
/// </summary>
/// <param name="Success">Indicates whether the transmission was successful</param>
/// <param name="TransmissionId">Unique identifier for this transmission attempt</param>
/// <param name="ErrorMessage">Error message if transmission failed, null if successful</param>
/// <param name="ProcessedAt">Timestamp when the transmission was processed</param>
/// <param name="TransmissionResult">Detailed transmission result from the provider</param>
/// <param name="AuditLogId">Identifier for the created audit log entry</param>
public record SendORUMessageResult(
    bool Success,
    string TransmissionId,
    string? ErrorMessage,
    DateTime ProcessedAt,
    TransmissionResult? TransmissionResult = null,
    string? AuditLogId = null)
{
    /// <summary>
    /// Gets the response time for the transmission if available
    /// </summary>
    public TimeSpan? ResponseTime => TransmissionResult?.ResponseTime;

    /// <summary>
    /// Gets the acknowledgment message received from the endpoint if available
    /// </summary>
    public string? AcknowledgmentMessage => TransmissionResult?.AcknowledgmentMessage;

    /// <summary>
    /// Gets the endpoint that was contacted for this transmission
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets the protocol used for transmission
    /// </summary>
    public TransmissionProtocol? Protocol { get; init; }

    /// <summary>
    /// Gets the source of the transmission request
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Creates a successful transmission result
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="transmissionResult">Detailed transmission result</param>
    /// <param name="auditLogId">Audit log identifier</param>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="protocol">Transmission protocol</param>
    /// <param name="source">Request source</param>
    /// <returns>Successful SendORUMessageResult</returns>
    public static SendORUMessageResult CreateSuccess(
        string transmissionId,
        TransmissionResult transmissionResult,
        string auditLogId,
        string endpoint,
        TransmissionProtocol protocol,
        string source) =>
        new(
            Success: true,
            TransmissionId: transmissionId,
            ErrorMessage: null,
            ProcessedAt: DateTime.UtcNow,
            TransmissionResult: transmissionResult,
            AuditLogId: auditLogId)
        {
            Endpoint = endpoint,
            Protocol = protocol,
            Source = source
        };

    /// <summary>
    /// Creates a failed transmission result
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <param name="auditLogId">Audit log identifier (optional)</param>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="protocol">Transmission protocol</param>
    /// <param name="source">Request source</param>
    /// <param name="transmissionResult">Partial transmission result if available</param>
    /// <returns>Failed SendORUMessageResult</returns>
    public static SendORUMessageResult CreateFailure(
        string transmissionId,
        string errorMessage,
        string endpoint,
        TransmissionProtocol protocol,
        string source,
        string? auditLogId = null,
        TransmissionResult? transmissionResult = null) =>
        new(
            Success: false,
            TransmissionId: transmissionId,
            ErrorMessage: errorMessage,
            ProcessedAt: DateTime.UtcNow,
            TransmissionResult: transmissionResult,
            AuditLogId: auditLogId)
        {
            Endpoint = endpoint,
            Protocol = protocol,
            Source = source
        };
}
