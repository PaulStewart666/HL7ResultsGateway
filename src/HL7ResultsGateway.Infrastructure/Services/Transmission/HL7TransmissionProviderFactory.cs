using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Domain.ValueObjects;

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
        try
        {
            // Try to return an already-registered provider instance if available
            switch (protocol)
            {
                case TransmissionProtocol.HTTP:
                case TransmissionProtocol.HTTPS:
                    {
                        // If a provider was registered directly, prefer that (test setups often use GetRequiredService)
                        // Prefer a directly-registered provider instance when available.
                        // Use IServiceProvider.GetService(typeof(...)) so test mocks can intercept the call.
                        try
                        {
                            var registered = _serviceProvider.GetService(typeof(HttpHL7TransmissionProvider)) as HttpHL7TransmissionProvider;
                            if (registered != null) return registered;
                        }
                        catch
                        {
                            // ignore and try to construct
                        }

                        var httpLogger = _serviceProvider.GetService(typeof(ILogger<HttpHL7TransmissionProvider>)) as ILogger<HttpHL7TransmissionProvider>;
                        var httpClient = _serviceProvider.GetService(typeof(HttpClient)) as HttpClient;

                        if (httpLogger == null || httpClient == null)
                            throw new InvalidOperationException("Failed to create HTTP transmission provider due to missing dependencies");

                        return new HttpHL7TransmissionProvider(httpLogger, httpClient);
                    }
                case TransmissionProtocol.MLLP:
                    {
                        try
                        {
                            var registered = _serviceProvider.GetService(typeof(MLLPTransmissionProvider)) as MLLPTransmissionProvider;
                            if (registered != null) return registered;
                        }
                        catch
                        {
                            // ignore and try to construct
                        }

                        var mllpLogger = _serviceProvider.GetService(typeof(ILogger<MLLPTransmissionProvider>)) as ILogger<MLLPTransmissionProvider>;
                        var httpClient = _serviceProvider.GetService(typeof(HttpClient)) as HttpClient;

                        if (mllpLogger == null || httpClient == null)
                            throw new InvalidOperationException("Failed to create MLLP transmission provider due to missing dependencies");

                        return new MLLPTransmissionProvider(mllpLogger, httpClient);
                    }
                case TransmissionProtocol.SFTP:
                    {
                        try
                        {
                            var registered = _serviceProvider.GetService(typeof(SftpTransmissionProvider)) as SftpTransmissionProvider;
                            if (registered != null) return registered;
                        }
                        catch
                        {
                            // ignore and try to construct
                        }

                        var sftpLogger = _serviceProvider.GetService(typeof(ILogger<SftpTransmissionProvider>)) as ILogger<SftpTransmissionProvider>;
                        var httpClient = _serviceProvider.GetService(typeof(HttpClient)) as HttpClient;

                        if (sftpLogger == null || httpClient == null)
                            throw new InvalidOperationException("Failed to create SFTP transmission provider due to missing dependencies");

                        return new SftpTransmissionProvider(sftpLogger, httpClient);
                    }
                default:
                    throw new ArgumentException("Unsupported transmission protocol", nameof(protocol));
            }
        }
        catch (ArgumentException)
        {
            throw; // let caller handle unsupported protocol as ArgumentException
        }
        catch (Exception ex)
        {
            // Wrap any creation error into InvalidOperationException with contextual message
            throw new InvalidOperationException($"Failed to create {protocol} transmission provider", ex);
        }
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
