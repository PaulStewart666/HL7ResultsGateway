using System.ComponentModel.DataAnnotations;

namespace HL7ResultsGateway.Client.Core.Configuration;

public class ApiConfiguration
{
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(0, 10)]
    public int RetryAttempts { get; set; } = 3;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutSeconds);
}
