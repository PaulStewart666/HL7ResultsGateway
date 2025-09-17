using System.ComponentModel.DataAnnotations;

namespace HL7ResultsGateway.Infrastructure.Configuration;

/// <summary>
/// Configuration options for HL7 transmission functionality
/// </summary>
public sealed class HL7TransmissionOptions
{
    public const string SectionName = "HL7Transmission";

    /// <summary>
    /// Default timeout for transmission operations in seconds
    /// </summary>
    [Range(1, 300)]
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for failed transmissions
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds
    /// </summary>
    [Range(1, 60)]
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Maximum size of HL7 message in bytes
    /// </summary>
    [Range(1024, 10485760)] // 1KB to 10MB
    public int MaxMessageSizeBytes { get; set; } = 1048576; // 1MB default

    /// <summary>
    /// HTTP client timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int HttpClientTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of concurrent transmissions
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentTransmissions { get; set; } = 10;

    /// <summary>
    /// Enable detailed logging for transmission operations
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Enable transmission audit logging to database
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Cosmos DB configuration for audit logging
    /// </summary>
    public CosmosDbOptions CosmosDb { get; set; } = new();

    /// <summary>
    /// Predefined endpoint configurations
    /// </summary>
    public Dictionary<string, EndpointConfiguration> Endpoints { get; set; } = new();
}

/// <summary>
/// Cosmos DB configuration options
/// </summary>
public sealed class CosmosDbOptions
{
    /// <summary>
    /// Cosmos DB connection string
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name for storing transmission logs
    /// </summary>
    [Required]
    public string DatabaseName { get; set; } = "HL7ResultsGateway";

    /// <summary>
    /// Container name for transmission logs
    /// </summary>
    [Required]
    public string ContainerName { get; set; } = "hl7-transmissions";

    /// <summary>
    /// Auto-create database and container if they don't exist
    /// </summary>
    public bool AutoCreateResources { get; set; } = true;

    /// <summary>
    /// Throughput for the container (RU/s)
    /// </summary>
    [Range(400, 100000)]
    public int ContainerThroughput { get; set; } = 400;
}
