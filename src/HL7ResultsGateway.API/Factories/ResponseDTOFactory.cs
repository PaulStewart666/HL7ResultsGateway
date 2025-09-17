using HL7ResultsGateway.API.Models;
using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using HL7ResultsGateway.Domain.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.API.Factories;

/// <summary>
/// Factory for creating consistent API response DTOs
/// </summary>
public sealed class ResponseDTOFactory : IResponseDTOFactory
{
    private readonly ILogger<ResponseDTOFactory> _logger;

    public ResponseDTOFactory(ILogger<ResponseDTOFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ApiResponse<SendORUResponseDTO> CreateSuccessResponse(
        SendORUMessageResult result,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        _logger.LogDebug(
            "Creating success response for transmission {TransmissionId}",
            result.TransmissionId);

        var responseDto = SendORUResponseDTO.FromSuccessResult(result);

        return ApiResponse<SendORUResponseDTO>.CreateSuccess(
            responseDto,
            StatusCodes.Status200OK,
            correlationId);
    }

    public ApiResponse<SendORUResponseDTO> CreateErrorResponse(
        string errorMessage,
        int statusCode = StatusCodes.Status400BadRequest,
        string? errorDetails = null,
        string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        _logger.LogDebug(
            "Creating error response with status {StatusCode}: {ErrorMessage}",
            statusCode, errorMessage);

        return ApiResponse<SendORUResponseDTO>.CreateError(
            errorMessage,
            statusCode,
            errorDetails,
            correlationId);
    }

    public ApiResponse<SendORUResponseDTO> CreateValidationErrorResponse(
        IEnumerable<string> validationErrors,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        var errors = validationErrors.ToList();
        if (!errors.Any())
            throw new ArgumentException("Validation errors collection cannot be empty", nameof(validationErrors));

        _logger.LogDebug(
            "Creating validation error response with {ErrorCount} errors",
            errors.Count);

        return ApiResponse<SendORUResponseDTO>.CreateValidationError(
            errors,
            correlationId);
    }

    public ApiResponse<SendORUResponseDTO> CreateInternalServerErrorResponse(
        string? errorMessage = null,
        string? errorDetails = null,
        string? correlationId = null)
    {
        var message = errorMessage ?? "An internal server error occurred";

        _logger.LogWarning(
            "Creating internal server error response: {ErrorMessage}",
            message);

        return ApiResponse<SendORUResponseDTO>.CreateInternalServerError(
            message,
            errorDetails,
            correlationId);
    }

    public ApiResponse<SendORUResponseDTO> CreateExceptionResponse(
        Exception exception,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogError(exception,
            "Creating exception response for {ExceptionType}",
            exception.GetType().Name);

        return exception switch
        {
            ArgumentNullException nullEx => CreateErrorResponse(
                $"Required parameter is missing: {nullEx.ParamName}",
                StatusCodes.Status400BadRequest,
                nullEx.ToString(),
                correlationId),

            ArgumentException argEx => CreateErrorResponse(
                $"Invalid argument: {argEx.Message}",
                StatusCodes.Status400BadRequest,
                argEx.ToString(),
                correlationId),

            TransmissionException transmissionEx => CreateErrorResponse(
                $"Transmission failed: {transmissionEx.Message}",
                StatusCodes.Status502BadGateway,
                transmissionEx.ToString(),
                correlationId),

            TimeoutException timeoutEx => CreateErrorResponse(
                $"Operation timed out: {timeoutEx.Message}",
                StatusCodes.Status408RequestTimeout,
                timeoutEx.ToString(),
                correlationId),

            OperationCanceledException cancelEx => CreateErrorResponse(
                "Operation was cancelled",
                StatusCodes.Status409Conflict,
                cancelEx.ToString(),
                correlationId),

            NotSupportedException notSupportedEx => CreateErrorResponse(
                $"Operation not supported: {notSupportedEx.Message}",
                StatusCodes.Status501NotImplemented,
                notSupportedEx.ToString(),
                correlationId),

            UnauthorizedAccessException unauthorizedEx => CreateErrorResponse(
                "Access denied",
                StatusCodes.Status401Unauthorized,
                unauthorizedEx.ToString(),
                correlationId),

            InvalidOperationException invalidOpEx => CreateErrorResponse(
                $"Invalid operation: {invalidOpEx.Message}",
                StatusCodes.Status422UnprocessableEntity,
                invalidOpEx.ToString(),
                correlationId),

            _ => CreateInternalServerErrorResponse(
                "An unexpected error occurred",
                exception.ToString(),
                correlationId)
        };
    }
}
