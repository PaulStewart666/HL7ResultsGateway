using System.ComponentModel.DataAnnotations;

namespace HL7ResultsGateway.Client.Features.JsonToHL7.Models;

/// <summary>
/// Client-side model for JSON to HL7 conversion requests
/// </summary>
public class JsonToHL7Request
{
    [Required]
    public JsonPatientData Patient { get; set; } = new();

    public List<JsonObservationData> Observations { get; set; } = new();

    public JsonMessageInfo MessageInfo { get; set; } = new();
}

/// <summary>
/// Patient data for JSON to HL7 conversion
/// </summary>
public class JsonPatientData
{
    [Required(ErrorMessage = "Patient ID is required")]
    public string PatientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = "U"; // U=Unknown, M=Male, F=Female, O=Other

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Observation data for JSON to HL7 conversion
/// </summary>
public class JsonObservationData
{
    [Required(ErrorMessage = "Observation ID is required")]
    public string ObservationId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    public string Value { get; set; } = string.Empty;

    public string? Units { get; set; }

    public string? ReferenceRange { get; set; }

    public string Status { get; set; } = "N"; // N=Normal, A=Abnormal, C=Critical, P=Pending

    public string ValueType { get; set; } = "ST"; // ST=String Text, NM=Numeric, CE=Coded Entry
}

/// <summary>
/// Message information for JSON to HL7 conversion
/// </summary>
public class JsonMessageInfo
{
    public string SendingApplication { get; set; } = "HL7Gateway";

    public string SendingFacility { get; set; } = "LAB";

    public string ReceivingApplication { get; set; } = "EMR";

    public string ReceivingFacility { get; set; } = "HOSPITAL";

    public string MessageControlId { get; set; } = Guid.NewGuid().ToString();

    public DateTime Timestamp { get; set; } = DateTime.Now;
}
