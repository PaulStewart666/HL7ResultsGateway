using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Entities;

/// <summary>
/// Entity representing a logged HL7 transmission attempt for audit trail purposes
/// Used for compliance tracking and transmission history
/// </summary>
public class HL7TransmissionLog
{
    /// <summary>
    /// Gets or sets the unique identifier for this transmission log entry
    /// </summary>
    public string TransmissionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target endpoint URL or address
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HL7 message type (e.g., ORU_R01)
    /// </summary>
    public string HL7MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient identifier from the transmitted message
    /// </summary>
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the transmission was initiated
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Gets or sets whether the transmission was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if transmission failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the time taken to complete the transmission
    /// </summary>
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the source of the transmission request
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the acknowledgment message received from the endpoint
    /// </summary>
    public string? AcknowledgmentMessage { get; set; }

    /// <summary>
    /// Gets or sets the transmission protocol used
    /// </summary>
    public TransmissionProtocol Protocol { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code for HTTP-based transmissions
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON string
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this log entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
