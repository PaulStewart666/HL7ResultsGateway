using HL7ResultsGateway.Domain.Entities;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Models;

/// <summary>
/// Represents the complete result of HL7 message processing including API response and parsed data
/// </summary>
public class HL7ProcessingResult
{
    public bool Success { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public HL7Result? ParsedMessage { get; set; }
    public string OriginalMessage { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public string? RequestId { get; set; }
}
