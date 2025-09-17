using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Services.Transmission;

/// <summary>
/// Factory for creating appropriate HL7 transmission providers based on protocol
/// </summary>
public sealed class HL7TransmissionProviderFactory : IHL7TransmissionProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HL7TransmissionProviderFactory> _logger;

    public HL7TransmissionProviderFactory(
        IServiceProvider serviceProvider,
        ILogger<HL7TransmissionProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IHL7TransmissionProvider CreateProvider(TransmissionProtocol protocol)
    {
        _logger.LogDebug("Creating transmission provider for protocol {Protocol}", protocol);

        return protocol switch
        {
            TransmissionProtocol.HTTP => _serviceProvider.GetRequiredService<HttpHL7TransmissionProvider>(),
            TransmissionProtocol.HTTPS => _serviceProvider.GetRequiredService<HttpHL7TransmissionProvider>(), // Same provider handles both
            TransmissionProtocol.MLLP => _serviceProvider.GetRequiredService<MLLPTransmissionProvider>(),
            TransmissionProtocol.SFTP => _serviceProvider.GetRequiredService<SftpTransmissionProvider>(),
            _ => throw new NotSupportedException($"Transmission protocol {protocol} is not supported")
        };
    }

    public IEnumerable<TransmissionProtocol> GetSupportedProtocols()
    {
        return new[]
        {
            TransmissionProtocol.HTTP,
            TransmissionProtocol.HTTPS,
            TransmissionProtocol.MLLP,
            TransmissionProtocol.SFTP
        };
    }

    public bool IsProtocolSupported(TransmissionProtocol protocol)
    {
        return GetSupportedProtocols().Contains(protocol);
    }

    public string? GetProviderName(TransmissionProtocol protocol)
    {
        if (!IsProtocolSupported(protocol))
            return null;

        try
        {
            var provider = CreateProvider(protocol);
            return provider.ProviderName;
        }
        catch
        {
            return null;
        }
    }
}
