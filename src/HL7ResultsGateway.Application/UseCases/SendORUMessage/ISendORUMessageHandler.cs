namespace HL7ResultsGateway.Application.UseCases.SendORUMessage;

/// <summary>
/// Handler interface for sending HL7 ORU^R01 messages to external healthcare systems
/// Follows the Command Handler pattern for CQRS implementation
/// </summary>
public interface ISendORUMessageHandler
{
    /// <summary>
    /// Handles the transmission of an HL7 ORU message to an external endpoint
    /// </summary>
    /// <param name="command">Command containing transmission details and message data</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Result of the transmission operation including success status and audit information</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    Task<SendORUMessageResult> Handle(
        SendORUMessageCommand command,
        CancellationToken cancellationToken = default);
}
