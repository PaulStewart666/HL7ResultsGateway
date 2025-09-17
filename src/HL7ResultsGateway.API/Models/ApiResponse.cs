namespace HL7ResultsGateway.API.Models;

/// <summary>
/// Generic API response wrapper providing consistent structure for all API endpoints
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional error details for troubleshooting
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for request tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// <param name="data">Response data</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>Successful API response</returns>
    public static ApiResponse<T> CreateSuccess(T data, int statusCode = 200, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = statusCode,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="errorDetails">Optional error details</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>Error API response</returns>
    public static ApiResponse<T> CreateError(string errorMessage, int statusCode = 400, string? errorDetails = null, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            StatusCode = statusCode,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    /// <param name="validationErrors">Validation error messages</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>Validation error API response</returns>
    public static ApiResponse<T> CreateValidationError(IEnumerable<string> validationErrors, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = "Validation failed",
            ErrorDetails = string.Join("; ", validationErrors),
            StatusCode = 400,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Creates an internal server error response
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="errorDetails">Optional error details</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>Internal server error API response</returns>
    public static ApiResponse<T> CreateInternalServerError(string errorMessage = "An internal server error occurred", string? errorDetails = null, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            StatusCode = 500,
            CorrelationId = correlationId
        };
    }
}