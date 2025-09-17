using System.ComponentModel.DataAnnotations;

using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Infrastructure.Configuration;

/// <summary>
/// Configuration for a specific HL7 transmission endpoint
/// </summary>
public sealed class EndpointConfiguration
{
    /// <summary>
    /// Unique identifier for this endpoint configuration
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this endpoint
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of this endpoint
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The endpoint URL or connection string
    /// </summary>
    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Transmission protocol to use
    /// </summary>
    [Required]
    public TransmissionProtocol Protocol { get; set; }

    /// <summary>
    /// Timeout for this endpoint in seconds (overrides global default)
    /// </summary>
    [Range(1, 300)]
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Custom headers to include with transmissions
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public AuthenticationConfiguration? Authentication { get; set; }

    /// <summary>
    /// SSL/TLS configuration
    /// </summary>
    public SslConfiguration Ssl { get; set; } = new();

    /// <summary>
    /// Retry configuration for this endpoint
    /// </summary>
    public RetryConfiguration Retry { get; set; } = new();

    /// <summary>
    /// Whether this endpoint is currently enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Environment tags (e.g., "dev", "staging", "prod")
    /// </summary>
    public HashSet<string> EnvironmentTags { get; set; } = new();
}

/// <summary>
/// Authentication configuration for an endpoint
/// </summary>
public sealed class AuthenticationConfiguration
{
    /// <summary>
    /// Authentication type
    /// </summary>
    [Required]
    public AuthenticationType Type { get; set; }

    /// <summary>
    /// Username for basic/digest authentication
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for basic/digest authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// API key for API key authentication
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// API key header name (default: "X-API-Key")
    /// </summary>
    public string ApiKeyHeader { get; set; } = "X-API-Key";

    /// <summary>
    /// Bearer token for bearer authentication
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// Path to private key file for SFTP key-based authentication
    /// </summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>
    /// Private key passphrase
    /// </summary>
    public string? PrivateKeyPassphrase { get; set; }
}

/// <summary>
/// SSL/TLS configuration
/// </summary>
public sealed class SslConfiguration
{
    /// <summary>
    /// Require SSL/TLS for this endpoint
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Validate SSL certificates
    /// </summary>
    public bool ValidateCertificate { get; set; } = true;

    /// <summary>
    /// Accept self-signed certificates (only for development)
    /// </summary>
    public bool AcceptSelfSignedCertificates { get; set; } = false;

    /// <summary>
    /// Custom CA certificate path for validation
    /// </summary>
    public string? CustomCaCertificatePath { get; set; }

    /// <summary>
    /// Client certificate path for mutual TLS
    /// </summary>
    public string? ClientCertificatePath { get; set; }

    /// <summary>
    /// Client certificate password
    /// </summary>
    public string? ClientCertificatePassword { get; set; }
}

/// <summary>
/// Retry configuration for an endpoint
/// </summary>
public sealed class RetryConfiguration
{
    /// <summary>
    /// Enable retry for this endpoint
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    [Range(0, 10)]
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay between retries in seconds
    /// </summary>
    [Range(1, 60)]
    public int InitialDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Use exponential backoff for retry delays
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Maximum delay between retries in seconds
    /// </summary>
    [Range(1, 300)]
    public int MaxDelaySeconds { get; set; } = 60;

    /// <summary>
    /// HTTP status codes that should trigger a retry
    /// </summary>
    public HashSet<int> RetryOnStatusCodes { get; set; } = new() { 408, 429, 500, 502, 503, 504 };
}

/// <summary>
/// Authentication types supported by endpoints
/// </summary>
public enum AuthenticationType
{
    None,
    Basic,
    Digest,
    ApiKey,
    Bearer,
    Certificate
}
