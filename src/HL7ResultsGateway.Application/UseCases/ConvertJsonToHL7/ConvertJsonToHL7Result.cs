using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Services.Conversion;

namespace HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;

/// <summary>
/// Result of converting JSON input to HL7 v2 message
/// </summary>
public record ConvertJsonToHL7Result(
    bool Success,
    HL7Result? ConvertedMessage,
    string? HL7MessageString,
    ValidationResult ValidationResult,
    string? ErrorMessage,
    DateTime ProcessedAt);