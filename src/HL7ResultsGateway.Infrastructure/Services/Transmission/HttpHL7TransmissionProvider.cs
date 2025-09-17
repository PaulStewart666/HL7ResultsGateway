using System.Diagnostics;
using System.Net;
using System.Text;

using HL7ResultsGateway.Domain.Exceptions;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Services.Transmission;

/// <summary>
/// HTTP/HTTPS transmission provider for HL7 messages
/// </summary>
public sealed class HttpHL7TransmissionProvider : BaseHL7TransmissionProvider
{
    public override TransmissionProtocol SupportedProtocol =>
        TransmissionProtocol.HTTP; // Also handles HTTPS based on endpoint URL

    public override string ProviderName => "HTTP/HTTPS Transmission Provider";

    public HttpHL7TransmissionProvider(ILogger<HttpHL7TransmissionProvider> logger, HttpClient httpClient)
        : base(logger, httpClient)
    {
    }

    public override async Task<TransmissionResult> SendMessageAsync(
        HL7TransmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var stopwatch = Stopwatch.StartNew();
        var transmissionId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "Starting HTTP transmission {TransmissionId} to endpoint {Endpoint}",
                transmissionId, request.Endpoint);

            // Configure HTTP client timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (request.TimeoutSeconds > 0)
            {
                cts.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds));
            }

            // Prepare HTTP request
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.Endpoint)
            {
                Content = new StringContent(request.HL7Message, Encoding.UTF8, "application/hl7-v2")
            };

            // Add custom headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    if (!string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value))
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            // Send HTTP request
            using var response = await _httpClient.SendAsync(httpRequest, cts.Token);
            stopwatch.Stop();

            // Read response content
            var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "HTTP transmission {TransmissionId} completed successfully in {Duration}ms with status {StatusCode}",
                    transmissionId, stopwatch.ElapsedMilliseconds, response.StatusCode);

                return CreateResult(
                    success: true,
                    transmissionId: transmissionId,
                    acknowledgmentMessage: responseContent,
                    responseTime: stopwatch.Elapsed);
            }
            else
            {
                var errorMessage = $"HTTP transmission failed with status {response.StatusCode}: {response.ReasonPhrase}";
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    errorMessage += $" - Response: {responseContent}";
                }

                _logger.LogError(
                    "HTTP transmission {TransmissionId} failed with status {StatusCode} in {Duration}ms",
                    transmissionId, response.StatusCode, stopwatch.ElapsedMilliseconds);

                return CreateResult(
                    success: false,
                    transmissionId: transmissionId,
                    errorMessage: errorMessage,
                    responseTime: stopwatch.Elapsed);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string errorMessage = "HTTP transmission was cancelled";

            _logger.LogWarning(
                "HTTP transmission {TransmissionId} was cancelled after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string errorMessage = "HTTP transmission timed out";

            _logger.LogError(ex,
                "HTTP transmission {TransmissionId} timed out after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            var errorMessage = $"HTTP transmission failed: {ex.Message}";

            _logger.LogError(ex,
                "HTTP transmission {TransmissionId} failed with network error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = $"HTTP transmission failed with unexpected error: {ex.Message}";

            _logger.LogError(ex,
                "HTTP transmission {TransmissionId} failed with unexpected error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            throw new TransmissionException(errorMessage, ex);
        }
    }

    public override Task<bool> ValidateEndpointAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return Task.FromResult(false);

            // Basic URL validation
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
                return Task.FromResult(false);

            // Check if scheme is HTTP or HTTPS
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public override async Task<bool> TestConnectionAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ValidateEndpointAsync(endpoint, cancellationToken))
                return false;

            using var request = new HttpRequestMessage(HttpMethod.Head, endpoint);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            // Consider the connection successful if we get any response (even errors)
            return true;
        }
        catch
        {
            return false;
        }
    }
}
