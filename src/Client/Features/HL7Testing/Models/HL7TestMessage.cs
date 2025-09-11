namespace HL7ResultsGateway.Client.Features.HL7Testing.Models;

/// <summary>
/// Represents a predefined HL7 test message with metadata
/// </summary>
public class HL7TestMessage
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ExpectedSource { get; set; } = "Test";
    public bool IsValid { get; set; } = true;
    public string Category { get; set; } = string.Empty;
}
