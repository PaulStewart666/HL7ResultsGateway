using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

using HL7ResultsGateway.Domain.Exceptions;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Infrastructure.Services.Transmission;

/// <summary>
/// MLLP (Minimal Lower Layer Protocol) transmission provider for HL7 messages
/// </summary>
public sealed class MLLPTransmissionProvider : BaseHL7TransmissionProvider
{
    private const byte MLLP_START_BLOCK = 0x0B;  // Vertical Tab (VT)
    private const byte MLLP_END_BLOCK = 0x1C;    // File Separator (FS)
    private const byte MLLP_CARRIAGE_RETURN = 0x0D; // Carriage Return (CR)

    public override TransmissionProtocol SupportedProtocol => TransmissionProtocol.MLLP;

    public override string ProviderName => "MLLP Transmission Provider";

    public MLLPTransmissionProvider(ILogger<MLLPTransmissionProvider> logger, HttpClient httpClient)
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
            // Parse endpoint to extract host and port
            if (!TryParseMLLPEndpoint(request.Endpoint, out var host, out var port))
            {
                throw new TransmissionException($"Invalid MLLP endpoint format: {request.Endpoint}. Expected format: mllp://host:port");
            }

            _logger.LogInformation(
                "Starting MLLP transmission {TransmissionId} to {Host}:{Port}",
                transmissionId, host, port);

            // Wrap HL7 message with MLLP framing
            var mllpMessage = WrapWithMLLPFraming(request.HL7Message);

            // Configure timeout
            var timeout = request.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(request.TimeoutSeconds)
                : TimeSpan.FromSeconds(30); // Default 30 seconds

            using var tcpClient = new TcpClient();

            // Connect with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            await tcpClient.ConnectAsync(host, port, cts.Token);

            if (!tcpClient.Connected)
            {
                throw new TransmissionException($"Failed to connect to MLLP endpoint {host}:{port}");
            }

            using var stream = tcpClient.GetStream();

            // Send MLLP message
            await stream.WriteAsync(mllpMessage, cts.Token);
            await stream.FlushAsync(cts.Token);

            _logger.LogDebug(
                "MLLP message sent to {Host}:{Port}, waiting for acknowledgment",
                host, port);

            // Read acknowledgment
            var acknowledgment = await ReadMLLPResponseAsync(stream, cts.Token);

            stopwatch.Stop();

            _logger.LogInformation(
                "MLLP transmission {TransmissionId} completed successfully in {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: true,
                transmissionId: transmissionId,
                acknowledgmentMessage: acknowledgment,
                responseTime: stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string errorMessage = "MLLP transmission was cancelled";

            _logger.LogWarning(
                "MLLP transmission {TransmissionId} was cancelled after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            var errorMessage = $"MLLP transmission failed with socket error: {ex.Message}";

            _logger.LogError(ex,
                "MLLP transmission {TransmissionId} failed with socket error after {Duration}ms",
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
            var errorMessage = $"MLLP transmission failed with unexpected error: {ex.Message}";

            _logger.LogError(ex,
                "MLLP transmission {TransmissionId} failed with unexpected error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            throw new TransmissionException(errorMessage, ex);
        }
    }

    private static bool TryParseMLLPEndpoint(string endpoint, out string host, out int port)
    {
        host = string.Empty;
        port = 0;

        try
        {
            // Handle endpoints like "mllp://hostname:port" or "hostname:port"
            var cleanEndpoint = endpoint.Replace("mllp://", "", StringComparison.OrdinalIgnoreCase);
            var parts = cleanEndpoint.Split(':');

            if (parts.Length == 2)
            {
                host = parts[0];
                return int.TryParse(parts[1], out port) && port > 0 && port <= 65535;
            }
        }
        catch
        {
            // Parsing failed
        }

        return false;
    }

    private static byte[] WrapWithMLLPFraming(string hl7Message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(hl7Message);
        var mllpMessage = new byte[messageBytes.Length + 3]; // +3 for VT, FS, CR

        mllpMessage[0] = MLLP_START_BLOCK;
        Array.Copy(messageBytes, 0, mllpMessage, 1, messageBytes.Length);
        mllpMessage[mllpMessage.Length - 2] = MLLP_END_BLOCK;
        mllpMessage[mllpMessage.Length - 1] = MLLP_CARRIAGE_RETURN;

        return mllpMessage;
    }

    private static async Task<string> ReadMLLPResponseAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        const int bufferSize = 4096;
        var buffer = new byte[bufferSize];
        var responseData = new List<byte>();
        var foundStartBlock = false;
        var foundEndBlock = false;

        while (!foundEndBlock && !cancellationToken.IsCancellationRequested)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

            if (bytesRead == 0)
                break; // Connection closed

            for (int i = 0; i < bytesRead; i++)
            {
                var currentByte = buffer[i];

                if (!foundStartBlock && currentByte == MLLP_START_BLOCK)
                {
                    foundStartBlock = true;
                    continue;
                }

                if (foundStartBlock && currentByte == MLLP_END_BLOCK)
                {
                    foundEndBlock = true;
                    break;
                }

                if (foundStartBlock)
                {
                    responseData.Add(currentByte);
                }
            }
        }

        return Encoding.UTF8.GetString(responseData.ToArray());
    }

    public override Task<bool> ValidateEndpointAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(TryParseMLLPEndpoint(endpoint, out _, out _));
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
            if (!TryParseMLLPEndpoint(endpoint, out var host, out var port))
                return false;

            using var tcpClient = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout for connection test

            await tcpClient.ConnectAsync(host, port, cts.Token);
            return tcpClient.Connected;
        }
        catch
        {
            return false;
        }
    }
}
