using System.Diagnostics;
using System.Text;

using HL7ResultsGateway.Domain.Exceptions;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

using Renci.SshNet;
using Renci.SshNet.Common;

namespace HL7ResultsGateway.Infrastructure.Services.Transmission;

/// <summary>
/// SFTP transmission provider for HL7 messages
/// </summary>
public sealed class SftpTransmissionProvider : BaseHL7TransmissionProvider
{
    public override TransmissionProtocol SupportedProtocol => TransmissionProtocol.SFTP;

    public override string ProviderName => "SFTP Transmission Provider";

    public SftpTransmissionProvider(ILogger<SftpTransmissionProvider> logger, HttpClient httpClient)
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
            // Parse SFTP endpoint and extract connection details
            if (!TryParseSftpEndpoint(request.Endpoint, request.Headers, out var connectionInfo, out var remotePath))
            {
                throw new TransmissionException($"Invalid SFTP endpoint or missing credentials: {request.Endpoint}");
            }

            _logger.LogInformation(
                "Starting SFTP transmission {TransmissionId} to {Host}:{Port}{Path}",
                transmissionId, connectionInfo.Host, connectionInfo.Port, remotePath);

            // Configure timeout
            var timeout = request.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(request.TimeoutSeconds)
                : TimeSpan.FromSeconds(60); // Default 60 seconds for SFTP

            connectionInfo.Timeout = timeout;

            // Create unique filename
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var filename = $"HL7_ORU_{transmissionId}_{timestamp}.hl7";
            var fullRemotePath = Path.Combine(remotePath, filename).Replace('\\', '/');

            using var sftpClient = new SftpClient(connectionInfo);

            // Connect with cancellation support
            await ConnectWithCancellationAsync(sftpClient, cancellationToken);

            if (!sftpClient.IsConnected)
            {
                throw new TransmissionException($"Failed to connect to SFTP server {connectionInfo.Host}:{connectionInfo.Port}");
            }

            // Upload the HL7 message as a file
            var messageBytes = Encoding.UTF8.GetBytes(request.HL7Message);
            using var messageStream = new MemoryStream(messageBytes);

            await Task.Run(() => sftpClient.UploadFile(messageStream, fullRemotePath), cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "SFTP transmission {TransmissionId} completed successfully in {Duration}ms. File uploaded: {Filename}",
                transmissionId, stopwatch.ElapsedMilliseconds, filename);

            return CreateResult(
                success: true,
                transmissionId: transmissionId,
                acknowledgmentMessage: $"File uploaded successfully: {filename}",
                responseTime: stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string errorMessage = "SFTP transmission was cancelled";

            _logger.LogWarning(
                "SFTP transmission {TransmissionId} was cancelled after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (SshConnectionException ex)
        {
            stopwatch.Stop();
            var errorMessage = $"SFTP transmission failed with connection error: {ex.Message}";

            _logger.LogError(ex,
                "SFTP transmission {TransmissionId} failed with connection error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (SshAuthenticationException ex)
        {
            stopwatch.Stop();
            var errorMessage = $"SFTP transmission failed with authentication error: {ex.Message}";

            _logger.LogError(ex,
                "SFTP transmission {TransmissionId} failed with authentication error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            return CreateResult(
                success: false,
                transmissionId: transmissionId,
                errorMessage: errorMessage,
                responseTime: stopwatch.Elapsed);
        }
        catch (SftpPathNotFoundException ex)
        {
            stopwatch.Stop();
            var errorMessage = $"SFTP transmission failed - remote path not found: {ex.Message}";

            _logger.LogError(ex,
                "SFTP transmission {TransmissionId} failed - remote path not found after {Duration}ms",
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
            var errorMessage = $"SFTP transmission failed with unexpected error: {ex.Message}";

            _logger.LogError(ex,
                "SFTP transmission {TransmissionId} failed with unexpected error after {Duration}ms",
                transmissionId, stopwatch.ElapsedMilliseconds);

            throw new TransmissionException(errorMessage, ex);
        }
    }

    private static bool TryParseSftpEndpoint(
        string endpoint,
        Dictionary<string, string>? headers,
        out ConnectionInfo connectionInfo,
        out string remotePath)
    {
        connectionInfo = null!;
        remotePath = string.Empty;

        try
        {
            // Parse endpoint like "sftp://username@hostname:port/path" or "hostname:port/path"
            var cleanEndpoint = endpoint.Replace("sftp://", "", StringComparison.OrdinalIgnoreCase);

            // Extract path if present
            var pathIndex = cleanEndpoint.IndexOf('/');
            if (pathIndex > 0)
            {
                remotePath = cleanEndpoint.Substring(pathIndex);
                cleanEndpoint = cleanEndpoint.Substring(0, pathIndex);
            }
            else
            {
                remotePath = "/"; // Default to root
            }

            // Extract username if present in URL
            string? username = null;
            var atIndex = cleanEndpoint.IndexOf('@');
            if (atIndex > 0)
            {
                username = cleanEndpoint.Substring(0, atIndex);
                cleanEndpoint = cleanEndpoint.Substring(atIndex + 1);
            }

            // Parse host and port
            var hostPortParts = cleanEndpoint.Split(':');
            if (hostPortParts.Length < 1 || hostPortParts.Length > 2)
                return false;

            var host = hostPortParts[0];
            var port = hostPortParts.Length == 2 && int.TryParse(hostPortParts[1], out var p) ? p : 22;

            // Get credentials from headers
            var headerUsername = headers?.GetValueOrDefault("Username") ?? headers?.GetValueOrDefault("SFTP-Username");
            var password = headers?.GetValueOrDefault("Password") ?? headers?.GetValueOrDefault("SFTP-Password");
            var privateKeyPath = headers?.GetValueOrDefault("PrivateKeyPath") ?? headers?.GetValueOrDefault("SFTP-PrivateKeyPath");

            // Use URL username if header username not provided
            username = headerUsername ?? username;

            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Create connection info based on available authentication methods
            if (!string.IsNullOrWhiteSpace(privateKeyPath) && File.Exists(privateKeyPath))
            {
                // Use private key authentication
                var keyFile = new PrivateKeyFile(privateKeyPath, password);
                connectionInfo = new ConnectionInfo(host, port, username, new PrivateKeyAuthenticationMethod(username, keyFile));
            }
            else if (!string.IsNullOrWhiteSpace(password))
            {
                // Use password authentication
                connectionInfo = new ConnectionInfo(host, port, username, new PasswordAuthenticationMethod(username, password));
            }
            else
            {
                return false; // No valid authentication method
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task ConnectWithCancellationAsync(SftpClient sftpClient, CancellationToken cancellationToken)
    {
        var connectTask = Task.Run(() => sftpClient.Connect(), cancellationToken);
        await connectTask;
    }

    public override Task<bool> ValidateEndpointAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For validation, we just check if we can parse the endpoint
            // We don't check credentials since they might be in headers
            var cleanEndpoint = endpoint.Replace("sftp://", "", StringComparison.OrdinalIgnoreCase);
            var pathIndex = cleanEndpoint.IndexOf('/');
            if (pathIndex > 0)
            {
                cleanEndpoint = cleanEndpoint.Substring(0, pathIndex);
            }

            var atIndex = cleanEndpoint.IndexOf('@');
            if (atIndex > 0)
            {
                cleanEndpoint = cleanEndpoint.Substring(atIndex + 1);
            }

            var hostPortParts = cleanEndpoint.Split(':');
            if (hostPortParts.Length < 1 || hostPortParts.Length > 2)
                return Task.FromResult(false);

            var host = hostPortParts[0];
            if (string.IsNullOrWhiteSpace(host))
                return Task.FromResult(false);

            if (hostPortParts.Length == 2)
            {
                return Task.FromResult(int.TryParse(hostPortParts[1], out var port) && port > 0 && port <= 65535);
            }

            return Task.FromResult(true); // Valid host, default port will be used
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
            // For SFTP, we need credentials to test the connection
            // This is a basic test that just validates the endpoint format
            // Full connection test would require actual credentials
            return await ValidateEndpointAsync(endpoint, cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
