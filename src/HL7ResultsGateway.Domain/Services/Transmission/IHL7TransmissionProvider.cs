using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Services.Transmission;

/// <summary>
/// Interface for HL7 message transmission providers
/// Implements Strategy pattern for different transmission protocols
/// </summary>
public interface IHL7TransmissionProvider
{
    /// <summary>
    /// Gets the transmission protocol supported by this provider
    /// </summary>
    TransmissionProtocol SupportedProtocol { get; }

    /// <summary>
    /// Gets the provider name for identification and logging purposes
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends an HL7 message to the specified endpoint
    /// </summary>
    /// <param name="request">Transmission request containing message and endpoint details</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Result of the transmission attempt</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="TransmissionException">Thrown when transmission fails</exception>
    Task<TransmissionResult> SendMessageAsync(
        HL7TransmissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the endpoint is reachable and properly configured
    /// </summary>
    /// <param name="endpoint">Endpoint URL or address to validate</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if endpoint is valid and reachable, false otherwise</returns>
    Task<bool> ValidateEndpointAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to an endpoint without sending actual data
    /// </summary>
    /// <param name="endpoint">Endpoint URL or address to test</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if connection test succeeds, false otherwise</returns>
    Task<bool> TestConnectionAsync(
        string endpoint,
        CancellationToken cancellationToken = default);
}
