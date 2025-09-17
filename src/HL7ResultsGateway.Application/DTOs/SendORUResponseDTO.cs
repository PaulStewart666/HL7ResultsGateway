using System.Text.Json.Serialization;

using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.DTOs;

/// <summary>
/// Data Transfer Object for ORU message transmission API responses
/// Provides structured response data for successful and failed transmissions
/// </summary>
public class SendORUResponseDTO
{
    /// <summary>
    /// Indicates whether the transmission was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Unique identifier for this transmission attempt
    /// </summary>
    [JsonPropertyName("transmissionId")]
    public string TransmissionId { get; set; } = string.Empty;

    /// <summary>
    /// Error message if transmission failed, null if successful
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when the transmission was processed
    /// </summary>
    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Response time for the transmission in milliseconds
    /// </summary>
    [JsonPropertyName("responseTimeMs")]
    public double? ResponseTimeMs { get; set; }

    /// <summary>
    /// Acknowledgment message received from the endpoint if available
    /// </summary>
    [JsonPropertyName("acknowledgmentMessage")]
    public string? AcknowledgmentMessage { get; set; }

    /// <summary>
    /// Target endpoint that was contacted for this transmission
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Protocol used for transmission
    /// </summary>
    [JsonPropertyName("protocol")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransmissionProtocol? Protocol { get; set; }

    /// <summary>
    /// Source of the transmission request
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Identifier for the created audit log entry
    /// </summary>
    [JsonPropertyName("auditLogId")]
    public string? AuditLogId { get; set; }

    /// <summary>
    /// Unique request ID for correlation and tracking
    /// </summary>
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Additional metadata about the transmission
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Patient identifier from the transmitted message (for tracking purposes)
    /// </summary>
    [JsonPropertyName("patientId")]
    public string? PatientId { get; set; }

    /// <summary>
    /// HL7 message type that was transmitted
    /// </summary>
    [JsonPropertyName("messageType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HL7MessageType? MessageType { get; set; }

    /// <summary>
    /// Number of observations included in the transmitted message
    /// </summary>
    [JsonPropertyName("observationCount")]
    public int? ObservationCount { get; set; }

    /// <summary>
    /// Creates a successful response DTO from a result
    /// </summary>
    /// <param name="result">Send ORU message result</param>
    /// <param name="patientId">Patient identifier</param>
    /// <param name="messageType">HL7 message type</param>
    /// <param name="observationCount">Number of observations</param>
    /// <returns>Successful SendORUResponseDTO</returns>
    public static SendORUResponseDTO FromSuccessResult(
        UseCases.SendORUMessage.SendORUMessageResult result,
        string? patientId = null,
        HL7MessageType? messageType = null,
        int? observationCount = null)
    {
        return new SendORUResponseDTO
        {
            Success = result.Success,
            TransmissionId = result.TransmissionId,
            ErrorMessage = result.ErrorMessage,
            ProcessedAt = result.ProcessedAt,
            ResponseTimeMs = result.ResponseTime?.TotalMilliseconds,
            AcknowledgmentMessage = result.AcknowledgmentMessage,
            Endpoint = result.Endpoint,
            Protocol = result.Protocol,
            Source = result.Source,
            AuditLogId = result.AuditLogId,
            PatientId = patientId,
            MessageType = messageType,
            ObservationCount = observationCount,
            Metadata = new Dictionary<string, object>
            {
                ["hasTransmissionResult"] = result.TransmissionResult != null,
                ["hasAuditLog"] = !string.IsNullOrEmpty(result.AuditLogId)
            }
        };
    }

    /// <summary>
    /// Creates a failed response DTO from a result
    /// </summary>
    /// <param name="result">Send ORU message result</param>
    /// <param name="patientId">Patient identifier</param>
    /// <param name="messageType">HL7 message type</param>
    /// <param name="observationCount">Number of observations</param>
    /// <returns>Failed SendORUResponseDTO</returns>
    public static SendORUResponseDTO FromFailureResult(
        UseCases.SendORUMessage.SendORUMessageResult result,
        string? patientId = null,
        HL7MessageType? messageType = null,
        int? observationCount = null)
    {
        return new SendORUResponseDTO
        {
            Success = result.Success,
            TransmissionId = result.TransmissionId,
            ErrorMessage = result.ErrorMessage,
            ProcessedAt = result.ProcessedAt,
            ResponseTimeMs = result.ResponseTime?.TotalMilliseconds,
            AcknowledgmentMessage = result.AcknowledgmentMessage,
            Endpoint = result.Endpoint,
            Protocol = result.Protocol,
            Source = result.Source,
            AuditLogId = result.AuditLogId,
            PatientId = patientId,
            MessageType = messageType,
            ObservationCount = observationCount,
            Metadata = new Dictionary<string, object>
            {
                ["hasTransmissionResult"] = result.TransmissionResult != null,
                ["hasAuditLog"] = !string.IsNullOrEmpty(result.AuditLogId),
                ["failureReason"] = result.ErrorMessage ?? "Unknown error"
            }
        };
    }
}
