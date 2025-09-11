using HL7ResultsGateway.Domain.Models;

namespace HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;

/// <summary>
/// Command for converting JSON input to HL7 v2 message
/// </summary>
public record ConvertJsonToHL7Command(JsonHL7Input JsonInput, string Source);
