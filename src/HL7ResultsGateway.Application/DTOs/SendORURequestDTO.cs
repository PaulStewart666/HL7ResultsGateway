using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.DTOs;

/// <summary>
/// Data Transfer Object for ORU message transmission API requests
/// Used for deserializing incoming HTTP requests for message transmission
/// </summary>
public class SendORURequestDTO
{
    /// <summary>
    /// Target endpoint URL or address for transmission
    /// </summary>
    [JsonPropertyName("destinationEndpoint")]
    [Required(ErrorMessage = "Destination endpoint is required")]
    [Url(ErrorMessage = "Destination endpoint must be a valid URL")]
    public string DestinationEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Complete HL7 message data including patient and observations
    /// </summary>
    [JsonPropertyName("messageData")]
    [Required(ErrorMessage = "Message data is required")]
    public HL7Result MessageData { get; set; } = new();

    /// <summary>
    /// Transmission protocol to use for sending the message
    /// </summary>
    [JsonPropertyName("protocol")]
    [Required(ErrorMessage = "Protocol is required")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransmissionProtocol Protocol { get; set; } = TransmissionProtocol.HTTPS;

    /// <summary>
    /// Additional headers required for transmission (optional)
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Maximum time to wait for transmission completion in seconds (optional, defaults to 30)
    /// </summary>
    [JsonPropertyName("timeoutSeconds")]
    [Range(5, 300, ErrorMessage = "Timeout must be between 5 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Source identifier for audit trail purposes (optional, will use default if not provided)
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Additional metadata for the transmission request (optional)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Whether to perform validation before transmission (optional, defaults to true)
    /// </summary>
    [JsonPropertyName("validateBeforeSend")]
    public bool ValidateBeforeSend { get; set; } = true;

    /// <summary>
    /// Priority level for the transmission (optional, for future use)
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransmissionPriority Priority { get; set; } = TransmissionPriority.Normal;
}

/// <summary>
/// Priority levels for message transmission
/// </summary>
public enum TransmissionPriority
{
    /// <summary>
    /// Low priority transmission - can be queued or delayed
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority transmission - standard processing
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority transmission - expedited processing
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority transmission - immediate processing required
    /// </summary>
    Critical = 3
}
