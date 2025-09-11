using System.Text.Json.Serialization;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Models;

/// <summary>
/// Represents the response from the HL7 processing API
/// </summary>
public class HL7ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }

    [JsonPropertyName("messageType")]
    public string? MessageType { get; set; }

    [JsonPropertyName("patientId")]
    public string? PatientId { get; set; }

    [JsonPropertyName("observationCount")]
    public int ObservationCount { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
