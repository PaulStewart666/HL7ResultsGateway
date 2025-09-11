using HL7ResultsGateway.Domain.Entities;

namespace HL7ResultsGateway.Client.Features.JsonToHL7.Models;

/// <summary>
/// Result of JSON to HL7 conversion processing
/// </summary>
public class JsonToHL7Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? HL7Message { get; set; }
    public HL7Result? ParsedResult { get; set; }
    public List<string>? ValidationErrors { get; set; }
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string Source { get; set; } = "JSON Converter";
}
