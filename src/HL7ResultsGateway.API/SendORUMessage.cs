using System.Net;

using HL7ResultsGateway.API.Factories;
using HL7ResultsGateway.API.Models;
using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using HL7ResultsGateway.Application.Validators;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.API;

/// <summary>
/// Azure Function for sending HL7 ORU messages to external endpoints
/// </summary>
public sealed class SendORUMessage
{
    private readonly ISendORUMessageHandler _handler;
    private readonly IResponseDTOFactory _responseFactory;
    private readonly SendORURequestValidator _validator;
    private readonly ILogger<SendORUMessage> _logger;

    public SendORUMessage(
        ISendORUMessageHandler handler,
        IResponseDTOFactory responseFactory,
        SendORURequestValidator validator,
        ILogger<SendORUMessage> logger)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("SendORUMessage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hl7/send-oru")] HttpRequest req,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GenerateCorrelationId(req);

        try
        {
            _logger.LogInformation(
                "Processing SendORUMessage request. CorrelationId: {CorrelationId}",
                correlationId);

            // Read and validate request body
            var requestDto = await ReadRequestAsync(req, cancellationToken);
            if (requestDto == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize request body. CorrelationId: {CorrelationId}",
                    correlationId);

                var errorResponse = _responseFactory.CreateErrorResponse(
                    "Invalid or missing request body",
                    StatusCodes.Status400BadRequest,
                    "Request body must be valid JSON containing ORU message transmission details",
                    correlationId);

                return new ObjectResult(errorResponse)
                {
                    StatusCode = errorResponse.StatusCode
                };
            }

            // Validate request DTO
            var validationResult = await _validator.ValidateAsync(requestDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Request validation failed with {ErrorCount} errors. CorrelationId: {CorrelationId}",
                    validationResult.Errors.Count, correlationId);

                var validationErrors = validationResult.Errors
                    .Select(error => $"{error.PropertyName}: {error.ErrorMessage}")
                    .ToList();

                var validationResponse = _responseFactory.CreateValidationErrorResponse(
                    validationErrors,
                    correlationId);

                return new BadRequestObjectResult(validationResponse);
            }

            // Create and execute command
            var command = CreateCommandFromRequest(requestDto);

            _logger.LogInformation(
                "Executing SendORUMessage command for endpoint {Endpoint} with protocol {Protocol}. CorrelationId: {CorrelationId}",
                command.DestinationEndpoint, command.Protocol, correlationId);

            var result = await _handler.Handle(command, cancellationToken);

            _logger.LogInformation(
                "SendORUMessage command completed. Success: {Success}, TransmissionId: {TransmissionId}. CorrelationId: {CorrelationId}",
                result.Success, result.TransmissionId, correlationId);

            // Create and return response
            var response = _responseFactory.CreateSuccessResponse(result, correlationId);

            return new OkObjectResult(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "SendORUMessage operation was cancelled. CorrelationId: {CorrelationId}",
                correlationId);

            var cancelResponse = _responseFactory.CreateErrorResponse(
                "Operation was cancelled",
                StatusCodes.Status409Conflict,
                "The transmission operation was cancelled before completion",
                correlationId);

            return new ObjectResult(cancelResponse)
            {
                StatusCode = cancelResponse.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while processing SendORUMessage request. CorrelationId: {CorrelationId}",
                correlationId);

            var errorResponse = _responseFactory.CreateExceptionResponse(ex, correlationId);

            return new ObjectResult(errorResponse)
            {
                StatusCode = errorResponse.StatusCode
            };
        }
    }

    private static string GenerateCorrelationId(HttpRequest req)
    {
        // Try to get correlation ID from headers first
        if (req.Headers.TryGetValue("X-Correlation-ID", out var correlationHeader) &&
            !string.IsNullOrWhiteSpace(correlationHeader.FirstOrDefault()))
        {
            return correlationHeader.First()!;
        }

        if (req.Headers.TryGetValue("X-Request-ID", out var requestHeader) &&
            !string.IsNullOrWhiteSpace(requestHeader.FirstOrDefault()))
        {
            return requestHeader.First()!;
        }

        // Generate new correlation ID if none provided
        return Guid.NewGuid().ToString();
    }

    private static async Task<SendORURequestDTO?> ReadRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StreamReader(req.Body);
            var json = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<SendORURequestDTO>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private static SendORUMessageCommand CreateCommandFromRequest(SendORURequestDTO requestDto)
    {
        return new SendORUMessageCommand(
            DestinationEndpoint: requestDto.DestinationEndpoint,
            MessageData: requestDto.MessageData,
            Source: requestDto.Source ?? "HL7ResultsGateway.API",
            Protocol: requestDto.Protocol,
            Headers: requestDto.Headers,
            TimeoutSeconds: requestDto.TimeoutSeconds);
    }
}
