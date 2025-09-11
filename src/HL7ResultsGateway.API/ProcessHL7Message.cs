using HL7ResultsGateway.Application.UseCases.ProcessHL7Message;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using System.Text;

namespace HL7ResultsGateway.API;

public class ProcessHL7Message
{
    private readonly ILogger<ProcessHL7Message> _logger;
    private readonly IProcessHL7MessageHandler _handler;

    public ProcessHL7Message(
        ILogger<ProcessHL7Message> logger,
        IProcessHL7MessageHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [Function("ProcessHL7Message")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hl7/process")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing HL7 message request");

        try
        {
            // Read the request body
            using var reader = new StreamReader(req.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Received empty request body");
                return new BadRequestObjectResult(new { error = "Request body cannot be empty" });
            }

            // Get source from query parameters or headers
            var source = req.Query["source"].FirstOrDefault()
                        ?? req.Headers["X-Source"].FirstOrDefault()
                        ?? "Unknown";

            // Create command and process
            var command = new ProcessHL7MessageCommand(requestBody, source);
            var result = await _handler.Handle(command, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed HL7 message from source: {Source}", source);

                var observations = result.ParsedMessage?.Observations?.Select(obs => new
                {
                    observationId = obs.ObservationId,
                    description = obs.Description,
                    value = obs.Value,
                    units = obs.Units,
                    referenceRange = obs.ReferenceRange,
                    status = obs.Status.ToString(),
                    valueType = obs.ValueType,
                    displayText = !string.IsNullOrEmpty(obs.Description) && !string.IsNullOrEmpty(obs.Value) && !string.IsNullOrEmpty(obs.Units)
                        ? $"{obs.Description}: {obs.Value} {obs.Units}"
                        : $"{obs.Description}: {obs.Value}"
                }).ToList();

                return new OkObjectResult(new
                {
                    success = true,
                    processedAt = result.ProcessedAt,
                    requestId = Guid.NewGuid().ToString(), // Generate unique request ID
                    messageType = result.ParsedMessage?.MessageType.ToString(),
                    patient = result.ParsedMessage?.Patient != null ? new
                    {
                        patientId = result.ParsedMessage.Patient.PatientId,
                        firstName = result.ParsedMessage.Patient.FirstName,
                        lastName = result.ParsedMessage.Patient.LastName,
                        middleName = result.ParsedMessage.Patient.MiddleName,
                        fullName = $"{result.ParsedMessage.Patient.FirstName} {result.ParsedMessage.Patient.MiddleName} {result.ParsedMessage.Patient.LastName}".Trim(),
                        dateOfBirth = result.ParsedMessage.Patient.DateOfBirth,
                        gender = result.ParsedMessage.Patient.Gender.ToString(),
                        address = result.ParsedMessage.Patient.Address
                    } : null,
                    observations = (object?)observations ?? new List<object>(),
                    observationCount = result.ParsedMessage?.Observations.Count ?? 0
                });
            }
            else
            {
                _logger.LogWarning("Failed to process HL7 message: {Error}", result.ErrorMessage);
                return new BadRequestObjectResult(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    processedAt = result.ProcessedAt
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing HL7 message");
            return new StatusCodeResult(500);
        }
    }
}
