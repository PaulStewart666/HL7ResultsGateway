using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Exceptions;

/// <summary>
/// Exception thrown when HL7 message transmission operations fail
/// Provides specific context about transmission failures
/// </summary>
public class TransmissionException : Exception
{
    /// <summary>
    /// Gets the transmission protocol that was being used when the error occurred
    /// </summary>
    public TransmissionProtocol? Protocol { get; }

    /// <summary>
    /// Gets the endpoint that was being contacted when the error occurred
    /// </summary>
    public string? Endpoint { get; }

    /// <summary>
    /// Gets the transmission identifier associated with this error
    /// </summary>
    public string? TransmissionId { get; }

    /// <summary>
    /// Gets the HTTP status code if applicable to the transmission error
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// Initializes a new instance of TransmissionException
    /// </summary>
    /// <param name="message">Error message</param>
    public TransmissionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of TransmissionException with inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public TransmissionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of TransmissionException with transmission context
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="protocol">Transmission protocol</param>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="transmissionId">Transmission identifier</param>
    public TransmissionException(
        string message,
        TransmissionProtocol protocol,
        string endpoint,
        string transmissionId) : base(message)
    {
        Protocol = protocol;
        Endpoint = endpoint;
        TransmissionId = transmissionId;
    }

    /// <summary>
    /// Initializes a new instance of TransmissionException with full context
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="protocol">Transmission protocol</param>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="transmissionId">Transmission identifier</param>
    /// <param name="httpStatusCode">HTTP status code if applicable</param>
    public TransmissionException(
        string message,
        Exception innerException,
        TransmissionProtocol protocol,
        string endpoint,
        string transmissionId,
        int? httpStatusCode = null) : base(message, innerException)
    {
        Protocol = protocol;
        Endpoint = endpoint;
        TransmissionId = transmissionId;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// Creates a TransmissionException for endpoint validation failures
    /// </summary>
    /// <param name="endpoint">Invalid endpoint</param>
    /// <param name="protocol">Protocol used</param>
    /// <returns>TransmissionException instance</returns>
    public static TransmissionException InvalidEndpoint(string endpoint, TransmissionProtocol protocol) =>
        new($"Invalid or unreachable endpoint: {endpoint}", protocol, endpoint, Guid.NewGuid().ToString());

    /// <summary>
    /// Creates a TransmissionException for timeout scenarios
    /// </summary>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="protocol">Protocol used</param>
    /// <param name="timeoutSeconds">Timeout value</param>
    /// <returns>TransmissionException instance</returns>
    public static TransmissionException Timeout(string endpoint, TransmissionProtocol protocol, int timeoutSeconds) =>
        new($"Transmission timeout after {timeoutSeconds} seconds to endpoint: {endpoint}", protocol, endpoint, Guid.NewGuid().ToString());

    /// <summary>
    /// Creates a TransmissionException for authentication failures
    /// </summary>
    /// <param name="endpoint">Target endpoint</param>
    /// <param name="protocol">Protocol used</param>
    /// <returns>TransmissionException instance</returns>
    public static TransmissionException AuthenticationFailed(string endpoint, TransmissionProtocol protocol) =>
        new($"Authentication failed for endpoint: {endpoint}", protocol, endpoint, Guid.NewGuid().ToString());
}
