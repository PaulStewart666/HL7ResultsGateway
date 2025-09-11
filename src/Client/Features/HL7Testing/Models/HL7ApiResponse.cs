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

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("messageType")]
    public string? MessageType { get; set; }

    [JsonPropertyName("patient")]
    public PatientInfo? Patient { get; set; }

    [JsonPropertyName("observations")]
    public List<ObservationInfo>? Observations { get; set; }

    [JsonPropertyName("observationCount")]
    public int ObservationCount { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Patient information from the HL7 API response
/// </summary>
public class PatientInfo
{
    [JsonPropertyName("patientId")]
    public string PatientId { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string MiddleName { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Observation information from the HL7 API response
/// </summary>
public class ObservationInfo
{
    [JsonPropertyName("observationId")]
    public string ObservationId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("units")]
    public string Units { get; set; } = string.Empty;

    [JsonPropertyName("referenceRange")]
    public string ReferenceRange { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("valueType")]
    public string ValueType { get; set; } = string.Empty;

    [JsonPropertyName("displayText")]
    public string DisplayText { get; set; } = string.Empty;
}
