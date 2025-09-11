using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HL7ResultsGateway.Domain.Models;

/// <summary>
/// JSON input model for converting to HL7 v2 messages
/// Represents patient demographics and observations
/// </summary>
public class JsonHL7Input
{
    /// <summary>
    /// Patient demographic information
    /// </summary>
    [Required]
    [JsonPropertyName("patient")]
    public JsonPatientData Patient { get; set; } = new();

    /// <summary>
    /// Collection of laboratory observations
    /// </summary>
    [JsonPropertyName("observations")]
    public List<JsonObservationData> Observations { get; set; } = new();

    /// <summary>
    /// Message metadata
    /// </summary>
    [JsonPropertyName("messageInfo")]
    public JsonMessageInfo MessageInfo { get; set; } = new();
}

/// <summary>
/// Patient demographic data from JSON input
/// </summary>
public class JsonPatientData
{
    /// <summary>
    /// Unique patient identifier
    /// </summary>
    [Required]
    [JsonPropertyName("patientId")]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Patient's first name
    /// </summary>
    [Required]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Patient's last name
    /// </summary>
    [Required]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Patient's middle name (optional)
    /// </summary>
    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Patient's date of birth in ISO format (YYYY-MM-DD)
    /// </summary>
    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Patient's gender (M/F/O/U)
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Patient's address
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

/// <summary>
/// Laboratory observation data from JSON input
/// </summary>
public class JsonObservationData
{
    /// <summary>
    /// Unique observation identifier
    /// </summary>
    [Required]
    [JsonPropertyName("observationId")]
    public string ObservationId { get; set; } = string.Empty;

    /// <summary>
    /// Observation description/name
    /// </summary>
    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Observation value
    /// </summary>
    [Required]
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Units of measurement
    /// </summary>
    [JsonPropertyName("units")]
    public string? Units { get; set; }

    /// <summary>
    /// Reference range for the observation
    /// </summary>
    [JsonPropertyName("referenceRange")]
    public string? ReferenceRange { get; set; }

    /// <summary>
    /// Observation status (N/A/C/P)
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Type of observation value (NM/ST/TX)
    /// </summary>
    [JsonPropertyName("valueType")]
    public string? ValueType { get; set; }
}

/// <summary>
/// Message metadata information
/// </summary>
public class JsonMessageInfo
{
    /// <summary>
    /// Sending facility identifier
    /// </summary>
    [JsonPropertyName("sendingFacility")]
    public string? SendingFacility { get; set; }

    /// <summary>
    /// Receiving facility identifier
    /// </summary>
    [JsonPropertyName("receivingFacility")]
    public string? ReceivingFacility { get; set; }

    /// <summary>
    /// Message control ID for tracking
    /// </summary>
    [JsonPropertyName("messageControlId")]
    public string? MessageControlId { get; set; }

    /// <summary>
    /// Message timestamp (will use current time if not provided)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}
