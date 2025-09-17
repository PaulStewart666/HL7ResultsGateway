using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Services.Transmission;

/// <summary>
/// Base abstract class for HL7 transmission providers providing common functionality
/// </summary>
public abstract class BaseHL7TransmissionProvider : IHL7TransmissionProvider
{
    protected readonly ILogger _logger;
    protected readonly HttpClient _httpClient;

    protected BaseHL7TransmissionProvider(ILogger logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public abstract TransmissionProtocol SupportedProtocol { get; }

    public abstract string ProviderName { get; }

    public abstract Task<TransmissionResult> SendMessageAsync(
        HL7TransmissionRequest request,
        CancellationToken cancellationToken = default);

    public abstract Task<bool> ValidateEndpointAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    public abstract Task<bool> TestConnectionAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the transmission request for common requirements
    /// </summary>
    protected virtual void ValidateRequest(HL7TransmissionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(request));

        if (string.IsNullOrWhiteSpace(request.HL7Message))
            throw new ArgumentException("HL7 message cannot be null or empty", nameof(request));

        if (request.Protocol != SupportedProtocol)
            throw new ArgumentException($"Protocol {request.Protocol} is not supported by this provider", nameof(request));
    }

    /// <summary>
    /// Creates a transmission result with timing information
    /// </summary>
    protected static TransmissionResult CreateResult(
        bool success,
        string? transmissionId = null,
        string? errorMessage = null,
        string? acknowledgmentMessage = null,
        TimeSpan? responseTime = null)
    {
        return new TransmissionResult
        (
            Success: success,
            TransmissionId: transmissionId ?? Guid.NewGuid().ToString(),
            ErrorMessage: errorMessage,
            AcknowledgmentMessage: acknowledgmentMessage,
            ResponseTime: responseTime ?? TimeSpan.Zero,
            SentAt: DateTime.UtcNow
        );
    }

    protected virtual void Dispose(bool disposing)
    {
        // Base implementation - derived classes can override if needed
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
