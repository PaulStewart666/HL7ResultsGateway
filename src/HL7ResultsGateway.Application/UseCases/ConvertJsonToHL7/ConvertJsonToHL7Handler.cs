using System.Threading;
using System.Threading.Tasks;
using HL7ResultsGateway.Domain.Services.Conversion;

namespace HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;

/// <summary>
/// Handler for converting JSON input to HL7 v2 messages
/// Orchestrates the conversion process using domain services
/// </summary>
public class ConvertJsonToHL7Handler : IConvertJsonToHL7Handler
{
    private readonly IJsonHL7Converter _converter;

    public ConvertJsonToHL7Handler(IJsonHL7Converter converter)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    /// <summary>
    /// Handles the conversion of JSON input to HL7 v2 message
    /// </summary>
    public async Task<ConvertJsonToHL7Result> Handle(ConvertJsonToHL7Command command, CancellationToken cancellationToken)
    {
        if (command?.JsonInput == null)
        {
            return new ConvertJsonToHL7Result(
                Success: false,
                ConvertedMessage: null,
                HL7MessageString: null,
                ValidationResult: ValidationResult.Failure("Command or JsonInput is null"),
                ErrorMessage: "Invalid input: Command or JsonInput cannot be null",
                ProcessedAt: DateTime.UtcNow);
        }

        try
        {
            // Validate input first
            var validationResult = await _converter.ValidateInputAsync(command.JsonInput);
            if (!validationResult.IsValid)
            {
                return new ConvertJsonToHL7Result(
                    Success: false,
                    ConvertedMessage: null,
                    HL7MessageString: null,
                    ValidationResult: validationResult,
                    ErrorMessage: $"Validation failed: {string.Join(", ", validationResult.Errors)}",
                    ProcessedAt: DateTime.UtcNow);
            }

            // Perform conversion
            var convertedMessage = await _converter.ConvertToHL7Async(command.JsonInput);
            var hl7String = await _converter.ConvertToHL7StringAsync(command.JsonInput);

            return new ConvertJsonToHL7Result(
                Success: true,
                ConvertedMessage: convertedMessage,
                HL7MessageString: hl7String,
                ValidationResult: validationResult,
                ErrorMessage: null,
                ProcessedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return new ConvertJsonToHL7Result(
                Success: false,
                ConvertedMessage: null,
                HL7MessageString: null,
                ValidationResult: ValidationResult.Failure($"Conversion error: {ex.Message}"),
                ErrorMessage: ex.Message,
                ProcessedAt: DateTime.UtcNow);
        }
    }
}