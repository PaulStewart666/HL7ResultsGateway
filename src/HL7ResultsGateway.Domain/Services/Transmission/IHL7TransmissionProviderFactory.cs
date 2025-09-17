using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Domain.Services.Transmission;

/// <summary>
/// Factory interface for creating and managing HL7 transmission providers
/// Implements Factory pattern for provider selection based on protocol
/// </summary>
public interface IHL7TransmissionProviderFactory
{
    /// <summary>
    /// Creates a transmission provider for the specified protocol
    /// </summary>
    /// <param name="protocol">Transmission protocol to create provider for</param>
    /// <returns>Transmission provider instance</returns>
    /// <exception cref="ArgumentException">Thrown when protocol is not supported</exception>
    /// <exception cref="InvalidOperationException">Thrown when provider cannot be created</exception>
    IHL7TransmissionProvider CreateProvider(TransmissionProtocol protocol);

    /// <summary>
    /// Gets all supported transmission protocols
    /// </summary>
    /// <returns>Collection of supported protocols</returns>
    IEnumerable<TransmissionProtocol> GetSupportedProtocols();

    /// <summary>
    /// Checks if a specific protocol is supported by any registered provider
    /// </summary>
    /// <param name="protocol">Protocol to check support for</param>
    /// <returns>True if protocol is supported, false otherwise</returns>
    bool IsProtocolSupported(TransmissionProtocol protocol);

    /// <summary>
    /// Gets the provider name for a specific protocol
    /// </summary>
    /// <param name="protocol">Protocol to get provider name for</param>
    /// <returns>Provider name, or null if protocol is not supported</returns>
    string? GetProviderName(TransmissionProtocol protocol);
}
