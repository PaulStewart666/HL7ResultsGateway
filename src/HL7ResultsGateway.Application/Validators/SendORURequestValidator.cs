using FluentValidation;

using HL7ResultsGateway.Application.DTOs;
using HL7ResultsGateway.Domain.ValueObjects;

namespace HL7ResultsGateway.Application.Validators;

/// <summary>
/// FluentValidation validator for SendORURequestDTO
/// Provides comprehensive validation rules for ORU message transmission requests
/// </summary>
public class SendORURequestValidator : AbstractValidator<SendORURequestDTO>
{
    /// <summary>
    /// Initializes validation rules for SendORURequestDTO
    /// </summary>
    public SendORURequestValidator()
    {
        // Validate destination endpoint
        RuleFor(x => x.DestinationEndpoint)
            .NotEmpty()
            .WithMessage("Destination endpoint is required")
            .Must(BeValidUrl)
            .WithMessage("Destination endpoint must be a valid URL")
            .Must(BeSecureEndpoint)
            .WithMessage("Destination endpoint should use HTTPS for secure transmission");

        // Validate message data
        RuleFor(x => x.MessageData)
            .NotNull()
            .WithMessage("Message data is required")
            .DependentRules(() =>
            {
                RuleFor(x => x.MessageData.Patient)
                    .NotNull()
                    .WithMessage("Patient data is required")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.MessageData.Patient.PatientId)
                            .NotEmpty()
                            .WithMessage("Patient ID is required")
                            .Length(1, 50)
                            .WithMessage("Patient ID must be between 1 and 50 characters");

                        RuleFor(x => x.MessageData.Patient.FirstName)
                            .NotEmpty()
                            .WithMessage("Patient first name is required")
                            .Length(1, 100)
                            .WithMessage("Patient first name must be between 1 and 100 characters");

                        RuleFor(x => x.MessageData.Patient.LastName)
                            .NotEmpty()
                            .WithMessage("Patient last name is required")
                            .Length(1, 100)
                            .WithMessage("Patient last name must be between 1 and 100 characters");

                        RuleFor(x => x.MessageData.Patient.MiddleName)
                            .MaximumLength(100)
                            .WithMessage("Patient middle name cannot exceed 100 characters")
                            .When(x => !string.IsNullOrEmpty(x.MessageData.Patient.MiddleName));
                    });

                RuleFor(x => x.MessageData.Observations)
                    .NotNull()
                    .WithMessage("Observations are required")
                    .Must(observations => observations != null && observations.Count > 0)
                    .WithMessage("At least one observation is required")
                    .Must(observations => observations != null && observations.Count <= 100)
                    .WithMessage("Cannot exceed 100 observations per message")
                    .DependentRules(() =>
                    {
                        RuleForEach(x => x.MessageData.Observations)
                            .ChildRules(observation =>
                            {
                                observation.RuleFor(obs => obs.ObservationId)
                                    .NotEmpty()
                                    .WithMessage("Observation ID is required")
                                    .Length(1, 50)
                                    .WithMessage("Observation ID must be between 1 and 50 characters");

                                observation.RuleFor(obs => obs.Description)
                                    .NotEmpty()
                                    .WithMessage("Observation description is required")
                                    .Length(1, 200)
                                    .WithMessage("Observation description must be between 1 and 200 characters");

                                observation.RuleFor(obs => obs.Value)
                                    .NotEmpty()
                                    .WithMessage("Observation value is required")
                                    .Length(1, 100)
                                    .WithMessage("Observation value must be between 1 and 100 characters");

                                observation.RuleFor(obs => obs.ValueType)
                                    .Must(BeValidValueType)
                                    .WithMessage("Observation value type must be one of: NM, ST, TX, DT, TM, TS")
                                    .When(obs => !string.IsNullOrEmpty(obs.ValueType));
                            });
                    });

                RuleFor(x => x.MessageData.MessageType)
                    .Must(BeValidMessageType)
                    .WithMessage("Message type must be ORU_R01 for ORU message transmission");
            });

        // Validate protocol
        RuleFor(x => x.Protocol)
            .IsInEnum()
            .WithMessage("Protocol must be a valid transmission protocol");

        // Validate timeout
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThanOrEqualTo(5)
            .WithMessage("Timeout must be at least 5 seconds")
            .LessThanOrEqualTo(300)
            .WithMessage("Timeout cannot exceed 300 seconds (5 minutes)");

        // Validate source if provided
        RuleFor(x => x.Source)
            .Length(1, 100)
            .WithMessage("Source must be between 1 and 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Source));

        // Validate headers if provided
        RuleFor(x => x.Headers)
            .Must(headers => headers!.Count <= 20)
            .WithMessage("Cannot exceed 20 custom headers")
            .When(x => x.Headers != null && x.Headers.Count > 0)
            .DependentRules(() =>
            {
                RuleFor(x => x.Headers)
                    .Must(BeValidHeaders)
                    .WithMessage("Header keys and values must not be empty and must be valid strings");
            });

        // Validate priority
        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be a valid transmission priority");

        // Cross-validation rules
        RuleFor(x => x)
            .Must(HaveConsistentProtocolAndEndpoint)
            .WithMessage("Protocol and endpoint must be compatible (HTTPS endpoints require HTTPS protocol)");
    }

    /// <summary>
    /// Validates if the URL is properly formatted
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates if the endpoint uses secure protocols (warns but doesn't fail for HTTP)
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if secure or valid, false if invalid</returns>
    private static bool BeSecureEndpoint(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Allow HTTP for development, but prefer HTTPS
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    /// <summary>
    /// Validates if the HL7 value type is supported
    /// </summary>
    /// <param name="valueType">Value type to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool BeValidValueType(string valueType)
    {
        var validTypes = new[] { "NM", "ST", "TX", "DT", "TM", "TS" };
        return validTypes.Contains(valueType.ToUpperInvariant());
    }

    /// <summary>
    /// Validates if the message type is appropriate for ORU transmission
    /// </summary>
    /// <param name="messageType">Message type to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool BeValidMessageType(HL7MessageType messageType)
    {
        return messageType == HL7MessageType.ORU_R01;
    }

    /// <summary>
    /// Validates if the headers are properly formatted
    /// </summary>
    /// <param name="headers">Headers to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool BeValidHeaders(Dictionary<string, string>? headers)
    {
        if (headers == null) return true;

        return headers.All(kvp =>
            !string.IsNullOrWhiteSpace(kvp.Key) &&
            !string.IsNullOrWhiteSpace(kvp.Value) &&
            kvp.Key.Length <= 100 &&
            kvp.Value.Length <= 500);
    }

    /// <summary>
    /// Validates consistency between protocol and endpoint
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <returns>True if consistent, false otherwise</returns>
    private static bool HaveConsistentProtocolAndEndpoint(SendORURequestDTO request)
    {
        if (!Uri.TryCreate(request.DestinationEndpoint, UriKind.Absolute, out var uri))
            return true; // Let URL validation handle this

        // For HTTP-based protocols, ensure consistency
        if (request.Protocol == TransmissionProtocol.HTTP || request.Protocol == TransmissionProtocol.HTTPS)
        {
            if (request.Protocol == TransmissionProtocol.HTTPS && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            if (request.Protocol == TransmissionProtocol.HTTP && uri.Scheme != Uri.UriSchemeHttp)
                return false;
        }

        return true;
    }
}
