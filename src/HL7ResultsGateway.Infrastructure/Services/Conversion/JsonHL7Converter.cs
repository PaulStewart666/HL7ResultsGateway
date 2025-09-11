using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.ValueObjects;
using HL7ResultsGateway.Infrastructure.Logging;

namespace HL7ResultsGateway.Infrastructure.Services.Conversion;

/// <summary>
/// Implementation of JSON to HL7 v2 message converter
/// Converts JSON input data to HL7 ORU^R01 messages following HL7 v2.5 standards
/// </summary>
public class JsonHL7Converter : IJsonHL7Converter
{
    private readonly ILoggingService _logger;

    public JsonHL7Converter(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<HL7Result> ConvertToHL7Async(JsonHL7Input jsonInput)
    {
        ArgumentNullException.ThrowIfNull(jsonInput);

        _logger.LogInformation("Starting JSON to HL7 conversion", new { PatientId = jsonInput.Patient?.PatientId });

        var validationResult = await ValidateInputAsync(jsonInput);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"JSON input validation failed: {string.Join(", ", validationResult.Errors)}";
            _logger.LogError(new InvalidOperationException(errorMessage), errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            var hl7Result = new HL7Result
            {
                MessageType = HL7MessageType.ORU_R01,
                Patient = ConvertPatient(jsonInput.Patient),
                Observations = ConvertObservations(jsonInput.Observations)
            };

            _logger.LogInformation("JSON to HL7 conversion completed successfully",
                new { PatientId = hl7Result.Patient.PatientId, ObservationCount = hl7Result.Observations.Count });

            return hl7Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error converting JSON to HL7: {ex.Message}");
            throw new InvalidOperationException($"Conversion failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> ConvertToHL7StringAsync(JsonHL7Input jsonInput)
    {
        ArgumentNullException.ThrowIfNull(jsonInput);

        _logger.LogInformation("Starting JSON to HL7 string conversion", new { PatientId = jsonInput.Patient?.PatientId });

        var hl7Result = await ConvertToHL7Async(jsonInput);

        try
        {
            var hl7String = BuildHL7Message(jsonInput, hl7Result);

            _logger.LogInformation("JSON to HL7 string conversion completed successfully",
                new { MessageLength = hl7String.Length });

            return hl7String;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error building HL7 string: {ex.Message}");
            throw new InvalidOperationException($"HL7 string generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Domain.Services.Conversion.ValidationResult> ValidateInputAsync(JsonHL7Input jsonInput)
    {
        ArgumentNullException.ThrowIfNull(jsonInput);

        _logger.LogDebug("Validating JSON input", new { PatientId = jsonInput.Patient?.PatientId });

        var errors = new List<string>();

        // Validate using data annotations
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new ValidationContext(jsonInput);

        if (!Validator.TryValidateObject(jsonInput, validationContext, validationResults, validateAllProperties: true))
        {
            errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Validation error"));
        }

        // Additional business logic validation
        if (jsonInput.Patient != null)
        {
            ValidatePatientData(jsonInput.Patient, errors);
        }
        ValidateObservations(jsonInput.Observations, errors);
        if (jsonInput.MessageInfo != null)
        {
            ValidateMessageInfo(jsonInput.MessageInfo, errors);
        }

        var result = errors.Count > 0
            ? Domain.Services.Conversion.ValidationResult.Failure(errors)
            : Domain.Services.Conversion.ValidationResult.Success();

        _logger.LogDebug($"Validation completed with {errors.Count} errors");

        return await Task.FromResult(result);
    }

    private static Patient ConvertPatient(JsonPatientData patientData)
    {
        var patient = new Patient
        {
            PatientId = patientData.PatientId,
            FirstName = patientData.FirstName,
            LastName = patientData.LastName,
            MiddleName = patientData.MiddleName,
            Address = patientData.Address
        };

        // Convert date of birth
        if (!string.IsNullOrEmpty(patientData.DateOfBirth))
        {
            if (DateTime.TryParseExact(patientData.DateOfBirth, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
            {
                patient.DateOfBirth = dob;
            }
        }

        // Convert gender
        if (!string.IsNullOrEmpty(patientData.Gender))
        {
            patient.Gender = patientData.Gender.ToUpperInvariant() switch
            {
                "M" => Gender.Male,
                "F" => Gender.Female,
                "O" => Gender.Other,
                _ => Gender.Unknown
            };
        }

        return patient;
    }

    private static List<Observation> ConvertObservations(List<JsonObservationData> observationData)
    {
        return observationData.Select(ConvertObservation).ToList();
    }

    private static Observation ConvertObservation(JsonObservationData observationData)
    {
        var observation = new Observation
        {
            ObservationId = observationData.ObservationId,
            Description = observationData.Description,
            Value = observationData.Value,
            Units = observationData.Units,
            ReferenceRange = observationData.ReferenceRange,
            ValueType = observationData.ValueType ?? "ST" // Default to string type
        };

        // Convert status
        if (!string.IsNullOrEmpty(observationData.Status))
        {
            observation.Status = observationData.Status.ToUpperInvariant() switch
            {
                "N" => ObservationStatus.Normal,
                "A" => ObservationStatus.Abnormal,
                "H" => ObservationStatus.Abnormal, // High
                "L" => ObservationStatus.Abnormal, // Low
                "C" => ObservationStatus.Critical,
                "P" => ObservationStatus.Pending,
                _ => ObservationStatus.Unknown
            };
        }

        return observation;
    }

    private string BuildHL7Message(JsonHL7Input jsonInput, HL7Result hl7Result)
    {
        var messageBuilder = new StringBuilder();
        var timestamp = GetMessageTimestamp(jsonInput.MessageInfo);
        var controlId = GetControlId(jsonInput.MessageInfo);

        // MSH - Message Header
        messageBuilder.AppendLine(BuildMSHSegment(jsonInput.MessageInfo, controlId, timestamp));

        // PID - Patient Identification
        messageBuilder.AppendLine(BuildPIDSegment(hl7Result.Patient));

        // OBR - Observation Request (required for ORU^R01)
        messageBuilder.AppendLine(BuildOBRSegment(controlId, timestamp));

        // OBX - Observation/Result segments
        for (int i = 0; i < hl7Result.Observations.Count; i++)
        {
            var obxSegment = BuildOBXSegment(hl7Result.Observations[i], i + 1);
            if (i < hl7Result.Observations.Count - 1)
            {
                messageBuilder.AppendLine(obxSegment);
            }
            else
            {
                messageBuilder.Append(obxSegment); // Don't add newline to last segment
            }
        }

        return messageBuilder.ToString();
    }

    private static string BuildMSHSegment(JsonMessageInfo messageInfo, string controlId, string timestamp)
    {
        var sendingFacility = messageInfo?.SendingFacility ?? "UNKNOWN";
        var receivingFacility = messageInfo?.ReceivingFacility ?? "UNKNOWN";

        return $"MSH|^~\\&|{sendingFacility}|{receivingFacility}||||||ORU^R01^ORU_R01|{controlId}|P|2.5";
    }

    private static string BuildPIDSegment(Patient patient)
    {
        var patientName = $"{patient.LastName?.ToUpperInvariant()}^{patient.FirstName?.ToUpperInvariant()}";
        if (!string.IsNullOrEmpty(patient.MiddleName))
        {
            patientName += $"^{patient.MiddleName.ToUpperInvariant()}";
        }

        var dateOfBirth = patient.DateOfBirth != default ? patient.DateOfBirth.ToString("yyyyMMdd") : "";
        var gender = patient.Gender switch
        {
            Gender.Male => "M",
            Gender.Female => "F",
            Gender.Other => "O",
            _ => "U"
        };

        var address = FormatAddress(patient.Address);

        return $"PID|1||{patient.PatientId}^^^HOSPITAL||{patientName}||{dateOfBirth}|{gender}|||{address}|";
    }

    private static string FormatAddress(string? address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return "";
        }

        // Simple address formatting - in real implementation, you'd parse the address properly
        return address.Replace(",", "^").Replace("  ", " ");
    }

    private static string BuildOBRSegment(string controlId, string timestamp)
    {
        // Basic OBR segment - in real implementation, you'd have more detailed ordering information
        return $"OBR|1||{controlId}|LAB^LABORATORY^L|||{timestamp}|||||||||||||||F";
    }

    private static string BuildOBXSegment(Observation observation, int sequenceNumber)
    {
        var valueType = observation.ValueType ?? "ST";
        var observationId = $"{observation.ObservationId}^{observation.Description}^L";
        var status = observation.Status switch
        {
            ObservationStatus.Normal => "N",
            ObservationStatus.Abnormal => "A",
            ObservationStatus.Critical => "C",
            ObservationStatus.Pending => "P",
            _ => "U"
        };

        var units = observation.Units ?? "";
        var referenceRange = observation.ReferenceRange ?? "";

        return $"OBX|{sequenceNumber}|{valueType}|{observationId}||{observation.Value}|{units}|{referenceRange}|{status}|||F";
    }

    private static string GetMessageTimestamp(JsonMessageInfo? messageInfo)
    {
        if (messageInfo != null && !string.IsNullOrEmpty(messageInfo.Timestamp))
        {
            if (DateTime.TryParse(messageInfo.Timestamp, out var parsedTime))
            {
                return parsedTime.ToString("yyyyMMddHHmmss");
            }
        }

        return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }

    private static string GetControlId(JsonMessageInfo? messageInfo)
    {
        return messageInfo?.MessageControlId ?? Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
    }

    private static void ValidatePatientData(JsonPatientData patient, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(patient.PatientId))
        {
            errors.Add("Patient ID is required");
        }

        if (string.IsNullOrWhiteSpace(patient.FirstName))
        {
            errors.Add("First name is required");
        }

        if (string.IsNullOrWhiteSpace(patient.LastName))
        {
            errors.Add("Last name is required");
        }

        // Validate date format if provided
        if (!string.IsNullOrEmpty(patient.DateOfBirth))
        {
            if (!DateTime.TryParseExact(patient.DateOfBirth, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add("Date of birth must be in yyyy-MM-dd format");
            }
        }

        // Validate gender if provided
        if (!string.IsNullOrEmpty(patient.Gender))
        {
            var validGenders = new[] { "M", "F", "O", "U" };
            if (!validGenders.Contains(patient.Gender.ToUpperInvariant()))
            {
                errors.Add("Gender must be M, F, O, or U");
            }
        }
    }

    private static void ValidateObservations(List<JsonObservationData> observations, List<string> errors)
    {
        if (observations.Count == 0)
        {
            errors.Add("At least one observation is required");
            return;
        }

        for (int i = 0; i < observations.Count; i++)
        {
            var observation = observations[i];
            var prefix = $"Observation {i + 1}:";

            if (string.IsNullOrWhiteSpace(observation.ObservationId))
            {
                errors.Add($"{prefix} Observation ID is required");
            }

            if (string.IsNullOrWhiteSpace(observation.Description))
            {
                errors.Add($"{prefix} Description is required");
            }

            if (string.IsNullOrWhiteSpace(observation.Value))
            {
                errors.Add($"{prefix} Value is required");
            }

            // Validate status if provided
            if (!string.IsNullOrEmpty(observation.Status))
            {
                var validStatuses = new[] { "N", "A", "H", "L", "C", "P" };
                if (!validStatuses.Contains(observation.Status.ToUpperInvariant()))
                {
                    errors.Add($"{prefix} Status must be N, A, H, L, C, or P");
                }
            }

            // Validate value type if provided
            if (!string.IsNullOrEmpty(observation.ValueType))
            {
                var validTypes = new[] { "NM", "ST", "TX", "DT", "TM", "TS" };
                if (!validTypes.Contains(observation.ValueType.ToUpperInvariant()))
                {
                    errors.Add($"{prefix} Value type must be NM, ST, TX, DT, TM, or TS");
                }
            }
        }
    }

    private static void ValidateMessageInfo(JsonMessageInfo messageInfo, List<string> errors)
    {
        // Validate timestamp format if provided
        if (!string.IsNullOrEmpty(messageInfo.Timestamp))
        {
            if (!DateTime.TryParse(messageInfo.Timestamp, out _))
            {
                errors.Add("Timestamp must be a valid date/time format");
            }
        }
    }
}
