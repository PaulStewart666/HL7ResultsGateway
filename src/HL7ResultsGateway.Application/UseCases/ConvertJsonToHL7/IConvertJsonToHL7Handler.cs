using System.Threading;
using System.Threading.Tasks;

namespace HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;

/// <summary>
/// Handler interface for converting JSON input to HL7 v2 messages
/// </summary>
public interface IConvertJsonToHL7Handler
{
    /// <summary>
    /// Handles the conversion of JSON input to HL7 v2 message
    /// </summary>
    /// <param name="command">The conversion command containing JSON input</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversion result</returns>
    Task<ConvertJsonToHL7Result> Handle(ConvertJsonToHL7Command command, CancellationToken cancellationToken);
}