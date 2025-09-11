using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Models;

namespace HL7ResultsGateway.Domain.Services.Conversion;

/// <summary>
/// Service interface for converting JSON input to HL7 v2 messages
/// Follows Strategy pattern for different conversion types
/// </summary>
public interface IJsonHL7Converter
{
    /// <summary>
    /// Converts JSON input to HL7 v2 ORU^R01 message
    /// </summary>
    /// <param name="jsonInput">JSON data containing patient and observation information</param>
    /// <returns>HL7Result containing parsed HL7 message data</returns>
    /// <exception cref="ArgumentNullException">Thrown when jsonInput is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when conversion fails due to invalid data</exception>
    Task<HL7Result> ConvertToHL7Async(JsonHL7Input jsonInput);

    /// <summary>
    /// Converts JSON input to raw HL7 v2 string message
    /// </summary>
    /// <param name="jsonInput">JSON data containing patient and observation information</param>
    /// <returns>Raw HL7 v2 message string</returns>
    /// <exception cref="ArgumentNullException">Thrown when jsonInput is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when conversion fails due to invalid data</exception>
    Task<string> ConvertToHL7StringAsync(JsonHL7Input jsonInput);

    /// <summary>
    /// Validates JSON input before conversion
    /// </summary>
    /// <param name="jsonInput">JSON data to validate</param>
    /// <returns>Validation result with any errors found</returns>
    Task<ValidationResult> ValidateInputAsync(JsonHL7Input jsonInput);
}

/// <summary>
/// Validation result for JSON input
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates if validation passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Collection of validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Creates a failed validation result with error collection
    /// </summary>
    public static ValidationResult Failure(IEnumerable<string> errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}