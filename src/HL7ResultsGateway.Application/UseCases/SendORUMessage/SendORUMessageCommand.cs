using System.ComponentModel.DataAnnotations;

using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.UseCases.SendORUMessage;

/// <summary>
/// Command for sending HL7 ORU^R01 messages to external healthcare systems
/// Contains all necessary information for message transmission and audit logging
/// </summary>
/// <param name="DestinationEndpoint">Target endpoint URL or address for transmission</param>
/// <param name="MessageData">Complete HL7 message data including patient and observations</param>
/// <param name="Source">Source identifier for audit trail purposes</param>
/// <param name="Protocol">Transmission protocol to use (HTTP, HTTPS, MLLP, SFTP)</param>
/// <param name="Headers">Additional headers required for transmission (optional)</param>
/// <param name="TimeoutSeconds">Maximum time to wait for transmission completion (optional, defaults to 30)</param>
public record SendORUMessageCommand(
    [Required(ErrorMessage = "Destination endpoint is required")]
    [Url(ErrorMessage = "Destination endpoint must be a valid URL")]
    string DestinationEndpoint,

    [Required(ErrorMessage = "Message data is required")]
    HL7Result MessageData,

    [Required(ErrorMessage = "Source is required")]
    [MinLength(1, ErrorMessage = "Source cannot be empty")]
    string Source,

    [Required(ErrorMessage = "Protocol is required")]
    TransmissionProtocol Protocol,

    Dictionary<string, string>? Headers = null,

    [Range(5, 300, ErrorMessage = "Timeout must be between 5 and 300 seconds")]
    int TimeoutSeconds = 30)
{
    /// <summary>
    /// Gets the unique identifier for this command execution
    /// </summary>
    public string CommandId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the timestamp when this command was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the headers dictionary, ensuring it's never null
    /// </summary>
    public Dictionary<string, string> RequestHeaders => Headers ?? new Dictionary<string, string>();
}
