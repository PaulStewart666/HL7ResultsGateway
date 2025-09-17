namespace HL7ResultsGateway.Domain.Models;

/// <summary>
/// Result model containing the outcome of an HL7 message transmission
/// Provides comprehensive information about transmission success, timing, and any errors
/// </summary>
/// <param name="Success">Indicates whether the transmission was successful</param>
/// <param name="TransmissionId">Unique identifier for this transmission attempt</param>
/// <param name="ErrorMessage">Error message if transmission failed, null if successful</param>
/// <param name="AcknowledgmentMessage">HL7 acknowledgment message received from the endpoint</param>
/// <param name="ResponseTime">Time taken to complete the transmission</param>
/// <param name="SentAt">Timestamp when the transmission was initiated</param>
public record TransmissionResult(
    bool Success,
    string TransmissionId,
    string? ErrorMessage,
    string? AcknowledgmentMessage,
    TimeSpan ResponseTime,
    DateTime SentAt)
{
    /// <summary>
    /// Creates a successful transmission result
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="acknowledgmentMessage">Optional acknowledgment message</param>
    /// <param name="responseTime">Time taken for transmission</param>
    /// <returns>Successful transmission result</returns>
    public static TransmissionResult CreateSuccess(
        string transmissionId,
        string? acknowledgmentMessage,
        TimeSpan responseTime) =>
        new(true, transmissionId, null, acknowledgmentMessage, responseTime, DateTime.UtcNow);

    /// <summary>
    /// Creates a failed transmission result
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <param name="responseTime">Time taken before failure occurred</param>
    /// <returns>Failed transmission result</returns>
    public static TransmissionResult CreateFailure(
        string transmissionId,
        string errorMessage,
        TimeSpan responseTime) =>
        new(false, transmissionId, errorMessage, null, responseTime, DateTime.UtcNow);
}
