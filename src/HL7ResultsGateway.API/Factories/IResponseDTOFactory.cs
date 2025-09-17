using HL7ResultsGateway.API.Models;
using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using Microsoft.AspNetCore.Http;

namespace HL7ResultsGateway.API.Factories;

/// <summary>
/// Factory interface for creating consistent API response DTOs
/// </summary>
public interface IResponseDTOFactory
{
    /// <summary>
    /// Creates a successful response from SendORUMessage result
    /// </summary>
    /// <param name="result">SendORUMessage result</param>
    /// <param name="correlationId">Optional correlation ID for tracing</param>
    /// <returns>Successful API response</returns>
    ApiResponse<SendORUResponseDTO> CreateSuccessResponse(
        SendORUMessageResult result,
        string? correlationId = null);

    /// <summary>
    /// Creates an error response with appropriate status code and message
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorDetails">Optional detailed error information</param>
    /// <param name="correlationId">Optional correlation ID for tracing</param>
    /// <returns>Error API response</returns>
    ApiResponse<SendORUResponseDTO> CreateErrorResponse(
        string errorMessage,
        int statusCode = StatusCodes.Status400BadRequest,
        string? errorDetails = null,
        string? correlationId = null);

    /// <summary>
    /// Creates a validation error response from validation failures
    /// </summary>
    /// <param name="validationErrors">Collection of validation error messages</param>
    /// <param name="correlationId">Optional correlation ID for tracing</param>
    /// <returns>Validation error API response</returns>
    ApiResponse<SendORUResponseDTO> CreateValidationErrorResponse(
        IEnumerable<string> validationErrors,
        string? correlationId = null);

    /// <summary>
    /// Creates an internal server error response
    /// </summary>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <param name="errorDetails">Optional detailed error information</param>
    /// <param name="correlationId">Optional correlation ID for tracing</param>
    /// <returns>Internal server error API response</returns>
    ApiResponse<SendORUResponseDTO> CreateInternalServerErrorResponse(
        string? errorMessage = null,
        string? errorDetails = null,
        string? correlationId = null);

    /// <summary>
    /// Creates a response from an exception
    /// </summary>
    /// <param name="exception">Exception to convert to response</param>
    /// <param name="correlationId">Optional correlation ID for tracing</param>
    /// <returns>Appropriate error API response based on exception type</returns>
    ApiResponse<SendORUResponseDTO> CreateExceptionResponse(
        Exception exception,
        string? correlationId = null);
}