using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Models;

/// <summary>
/// Request model for HL7 message transmission operations
/// Contains all necessary information for sending an HL7 message to an external endpoint
/// </summary>
/// <param name="Endpoint">Target endpoint URL or address for transmission</param>
/// <param name="HL7Message">Complete HL7 message string to be transmitted</param>
/// <param name="Headers">Additional headers required for transmission (e.g., authentication, content-type)</param>
/// <param name="TimeoutSeconds">Maximum time to wait for transmission completion</param>
/// <param name="Protocol">Transmission protocol to use for the request</param>
public record HL7TransmissionRequest(
    string Endpoint,
    string HL7Message,
    Dictionary<string, string> Headers,
    int TimeoutSeconds,
    TransmissionProtocol Protocol)
{
    /// <summary>
    /// Gets the unique identifier for this transmission request
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the timestamp when this request was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
