using HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;
using HL7ResultsGateway.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace HL7ResultsGateway.API;

/// <summary>
/// Azure Function for converting JSON input to HL7 v2 messages
/// Follows RESTful API design principles and Clean Architecture
/// </summary>
public class ConvertJsonToHL7
{
    private readonly ILogger<ConvertJsonToHL7> _logger;
    private readonly IConvertJsonToHL7Handler _handler;

    public ConvertJsonToHL7(
        ILogger<ConvertJsonToHL7> logger,
        IConvertJsonToHL7Handler handler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// Converts JSON input to HL7 v2 ORU^R01 message format
    /// </summary>
    /// <param name="req">HTTP request containing JSON payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted HL7 message and parsed data</returns>
    [Function("ConvertJsonToHL7")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hl7/convert")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing JSON to HL7 conversion request");

        try
        {
            // Read and parse JSON request body
            using var reader = new StreamReader(req.Body, System.Text.Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Received empty request body");
                return new BadRequestObjectResult(new
                {
                    success = false,
                    error = "Request body cannot be empty. Please provide JSON input data.",
                    processedAt = DateTime.UtcNow
                });
            }

            JsonHL7Input jsonInput;
            try
            {
                // Deserialize JSON with case-insensitive property matching
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                jsonInput = JsonSerializer.Deserialize<JsonHL7Input>(requestBody, options)
                           ?? throw new JsonException("Deserialized JSON resulted in null object");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Failed to parse JSON input: {Error}", ex.Message);
                return new BadRequestObjectResult(new
                {
                    success = false,
                    error = $"Invalid JSON format: {ex.Message}",
                    processedAt = DateTime.UtcNow
                });
            }

            // Get source from query parameters or headers
            var source = req.Query["source"].FirstOrDefault()
                        ?? req.Headers["X-Source"].FirstOrDefault()
                        ?? "API";

            // Create command and process conversion
            var command = new ConvertJsonToHL7Command(jsonInput, source);
            var result = await _handler.Handle(command, cancellationToken);

            if (result.Success && result.ConvertedMessage != null)
            {
                _logger.LogInformation("Successfully converted JSON to HL7 from source: {Source}", source);

                // Format observations for response
                var observationsForResponse = result.ConvertedMessage.Observations?.Select(obs => new
                {
                    observationId = obs.ObservationId,
                    description = obs.Description,
                    value = obs.Value,
                    units = obs.Units,
                    referenceRange = obs.ReferenceRange,
                    status = obs.Status.ToString(),
                    valueType = obs.ValueType,
                    displayText = BuildObservationDisplayText(obs.Description, obs.Value, obs.Units)
                }).ToArray() ?? Array.Empty<object>();

                // Format patient data for response
                var patientResponse = result.ConvertedMessage.Patient != null ? new
                {
                    patientId = result.ConvertedMessage.Patient.PatientId,
                    firstName = result.ConvertedMessage.Patient.FirstName,
                    lastName = result.ConvertedMessage.Patient.LastName,
                    middleName = result.ConvertedMessage.Patient.MiddleName,
                    fullName = BuildPatientFullName(result.ConvertedMessage.Patient),
                    dateOfBirth = result.ConvertedMessage.Patient.DateOfBirth,
                    gender = result.ConvertedMessage.Patient.Gender.ToString(),
                    address = result.ConvertedMessage.Patient.Address
                } : null;

                return new OkObjectResult(new
                {
                    success = true,
                    processedAt = result.ProcessedAt,
                    requestId = Guid.NewGuid().ToString(),
                    source = source,
                    messageType = result.ConvertedMessage.MessageType.ToString(),
                    hl7Message = result.HL7MessageString,
                    patient = patientResponse,
                    observations = observationsForResponse,
                    observationCount = result.ConvertedMessage.Observations?.Count ?? 0,
                    validationResult = new
                    {
                        isValid = result.ValidationResult.IsValid,
                        errors = result.ValidationResult.Errors
                    }
                });
            }
            else
            {
                _logger.LogWarning("Failed to convert JSON to HL7: {Error}", result.ErrorMessage);
                return new BadRequestObjectResult(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    processedAt = result.ProcessedAt,
                    validationResult = new
                    {
                        isValid = result.ValidationResult.IsValid,
                        errors = result.ValidationResult.Errors
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JSON to HL7 conversion");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Builds display text for observation data
    /// </summary>
    private static string BuildObservationDisplayText(string description, string value, string units)
    {
        if (string.IsNullOrWhiteSpace(description))
            return value;

        if (string.IsNullOrWhiteSpace(value))
            return description;

        if (string.IsNullOrWhiteSpace(units))
            return $"{description}: {value}";

        return $"{description}: {value} {units}";
    }

    /// <summary>
    /// Builds full name from patient name components
    /// </summary>
    private static string BuildPatientFullName(Domain.Entities.Patient patient)
    {
        var nameParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(patient.FirstName))
            nameParts.Add(patient.FirstName);

        if (!string.IsNullOrWhiteSpace(patient.MiddleName))
            nameParts.Add(patient.MiddleName);

        if (!string.IsNullOrWhiteSpace(patient.LastName))
            nameParts.Add(patient.LastName);

        return string.Join(" ", nameParts);
    }
}
