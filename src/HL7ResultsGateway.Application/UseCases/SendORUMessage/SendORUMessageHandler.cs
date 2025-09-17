using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.Exceptions;
using HL7ResultsGateway.Domain.Models;
using HL7ResultsGateway.Domain.Services;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

namespace HL7ResultsGateway.Application.UseCases.SendORUMessage;

/// <summary>
/// Handler for sending HL7 ORU^R01 messages to external healthcare systems
/// Implements comprehensive transmission logic with audit logging and error handling
/// </summary>
public class SendORUMessageHandler : ISendORUMessageHandler
{
    private readonly IHL7TransmissionProviderFactory _providerFactory;
    private readonly IHL7TransmissionRepository _transmissionRepository;
    private readonly IJsonHL7Converter _hl7Converter;
    private readonly ILogger<SendORUMessageHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SendORUMessageHandler
    /// </summary>
    /// <param name="providerFactory">Factory for creating transmission providers</param>
    /// <param name="transmissionRepository">Repository for audit logging</param>
    /// <param name="hl7Converter">Converter for generating HL7 message strings</param>
    /// <param name="logger">Logger for diagnostics and monitoring</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public SendORUMessageHandler(
        IHL7TransmissionProviderFactory providerFactory,
        IHL7TransmissionRepository transmissionRepository,
        IJsonHL7Converter hl7Converter,
        ILogger<SendORUMessageHandler> logger)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _transmissionRepository = transmissionRepository ?? throw new ArgumentNullException(nameof(transmissionRepository));
        _hl7Converter = hl7Converter ?? throw new ArgumentNullException(nameof(hl7Converter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SendORUMessageResult> Handle(
        SendORUMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var transmissionId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Starting ORU message transmission. TransmissionId: {TransmissionId}, Endpoint: {Endpoint}, Protocol: {Protocol}, Source: {Source}",
            transmissionId, command.DestinationEndpoint, command.Protocol, command.Source);

        try
        {
            // Validate protocol support
            if (!_providerFactory.IsProtocolSupported(command.Protocol))
            {
                var errorMessage = $"Transmission protocol {command.Protocol} is not supported";
                _logger.LogWarning("Unsupported protocol: {Protocol}", command.Protocol);

                await LogTransmissionFailureAsync(
                    transmissionId, command, errorMessage, TimeSpan.Zero, cancellationToken);

                return SendORUMessageResult.CreateFailure(
                    transmissionId, errorMessage, command.DestinationEndpoint,
                    command.Protocol, command.Source);
            }

            // Generate HL7 message string from domain entity
            var hl7MessageString = await GenerateHL7MessageAsync(command.MessageData, cancellationToken);

            // Create transmission request
            var transmissionRequest = new HL7TransmissionRequest(
                Endpoint: command.DestinationEndpoint,
                HL7Message: hl7MessageString,
                Headers: command.RequestHeaders,
                TimeoutSeconds: command.TimeoutSeconds,
                Protocol: command.Protocol);

            // Get appropriate provider and send message
            var provider = _providerFactory.CreateProvider(command.Protocol);
            var transmissionResult = await provider.SendMessageAsync(transmissionRequest, cancellationToken);

            // Log the transmission attempt
            var auditLogId = await LogTransmissionAttemptAsync(
                transmissionId, command, transmissionResult, cancellationToken);

            if (transmissionResult.Success)
            {
                _logger.LogInformation(
                    "ORU message transmission successful. TransmissionId: {TransmissionId}, ResponseTime: {ResponseTime}ms",
                    transmissionId, transmissionResult.ResponseTime.TotalMilliseconds);

                return SendORUMessageResult.CreateSuccess(
                    transmissionId, transmissionResult, auditLogId,
                    command.DestinationEndpoint, command.Protocol, command.Source);
            }
            else
            {
                _logger.LogWarning(
                    "ORU message transmission failed. TransmissionId: {TransmissionId}, Error: {Error}",
                    transmissionId, transmissionResult.ErrorMessage);

                return SendORUMessageResult.CreateFailure(
                    transmissionId, transmissionResult.ErrorMessage ?? "Transmission failed",
                    command.DestinationEndpoint, command.Protocol, command.Source,
                    auditLogId, transmissionResult);
            }
        }
        catch (TransmissionException ex)
        {
            _logger.LogError(ex,
                "Transmission exception occurred. TransmissionId: {TransmissionId}, Protocol: {Protocol}, Endpoint: {Endpoint}",
                transmissionId, ex.Protocol, ex.Endpoint);

            await LogTransmissionFailureAsync(
                transmissionId, command, ex.Message, TimeSpan.Zero, cancellationToken);

            return SendORUMessageResult.CreateFailure(
                transmissionId, ex.Message, command.DestinationEndpoint,
                command.Protocol, command.Source);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Invalid operation during ORU transmission. TransmissionId: {TransmissionId}",
                transmissionId);

            await LogTransmissionFailureAsync(
                transmissionId, command, ex.Message, TimeSpan.Zero, cancellationToken);

            return SendORUMessageResult.CreateFailure(
                transmissionId, ex.Message, command.DestinationEndpoint,
                command.Protocol, command.Source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during ORU transmission. TransmissionId: {TransmissionId}",
                transmissionId);

            var errorMessage = "An unexpected error occurred during message transmission";
            await LogTransmissionFailureAsync(
                transmissionId, command, errorMessage, TimeSpan.Zero, cancellationToken);

            return SendORUMessageResult.CreateFailure(
                transmissionId, errorMessage, command.DestinationEndpoint,
                command.Protocol, command.Source);
        }
    }

    /// <summary>
    /// Generates an HL7 message string from the domain entity
    /// </summary>
    /// <param name="messageData">HL7 result containing patient and observation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete HL7 message string</returns>
    private async Task<string> GenerateHL7MessageAsync(
        HL7Result messageData,
        CancellationToken cancellationToken)
    {
        // Create a JsonHL7Input from the HL7Result for conversion
        var jsonInput = ConvertHL7ResultToJsonInput(messageData);

        return await _hl7Converter.ConvertToHL7StringAsync(jsonInput);
    }

    /// <summary>
    /// Converts HL7Result domain entity to JsonHL7Input for the converter
    /// </summary>
    /// <param name="hl7Result">Domain entity containing HL7 data</param>
    /// <returns>JsonHL7Input for the converter</returns>
    private static Domain.Models.JsonHL7Input ConvertHL7ResultToJsonInput(HL7Result hl7Result)
    {
        var jsonInput = new Domain.Models.JsonHL7Input
        {
            Patient = new Domain.Models.JsonPatientData
            {
                PatientId = hl7Result.Patient.PatientId,
                FirstName = hl7Result.Patient.FirstName,
                LastName = hl7Result.Patient.LastName,
                MiddleName = hl7Result.Patient.MiddleName,
                DateOfBirth = hl7Result.Patient.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = hl7Result.Patient.Gender switch
                {
                    Gender.Male => "M",
                    Gender.Female => "F",
                    Gender.Other => "O",
                    _ => "U"
                },
                Address = hl7Result.Patient.Address
            },
            Observations = hl7Result.Observations.Select(obs => new Domain.Models.JsonObservationData
            {
                ObservationId = obs.ObservationId,
                Description = obs.Description,
                Value = obs.Value,
                Units = obs.Units,
                ReferenceRange = obs.ReferenceRange,
                Status = obs.Status switch
                {
                    ObservationStatus.Normal => "N",
                    ObservationStatus.Abnormal => "A",
                    ObservationStatus.Critical => "C",
                    ObservationStatus.Pending => "P",
                    _ => "U"
                },
                ValueType = obs.ValueType
            }).ToList(),
            MessageInfo = new Domain.Models.JsonMessageInfo
            {
                SendingFacility = "HL7Gateway",
                ReceivingFacility = "External",
                MessageControlId = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }
        };

        return jsonInput;
    }

    /// <summary>
    /// Logs a successful or failed transmission attempt
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="command">Original command</param>
    /// <param name="result">Transmission result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log identifier</returns>
    private async Task<string> LogTransmissionAttemptAsync(
        string transmissionId,
        SendORUMessageCommand command,
        TransmissionResult result,
        CancellationToken cancellationToken)
    {
        var log = new HL7TransmissionLog
        {
            TransmissionId = transmissionId,
            Endpoint = command.DestinationEndpoint,
            HL7MessageType = command.MessageData.MessageType.ToString(),
            PatientId = command.MessageData.Patient.PatientId,
            SentAt = result.SentAt,
            Success = result.Success,
            ErrorMessage = result.ErrorMessage,
            ResponseTime = result.ResponseTime,
            Source = command.Source,
            AcknowledgmentMessage = result.AcknowledgmentMessage,
            Protocol = command.Protocol,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                CommandId = command.CommandId,
                Headers = command.RequestHeaders,
                TimeoutSeconds = command.TimeoutSeconds,
                ObservationCount = command.MessageData.Observations.Count
            })
        };

        var savedLog = await _transmissionRepository.SaveTransmissionLogAsync(log, cancellationToken);
        return savedLog.TransmissionId;
    }

    /// <summary>
    /// Logs a transmission failure with minimal information
    /// </summary>
    /// <param name="transmissionId">Unique transmission identifier</param>
    /// <param name="command">Original command</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="responseTime">Response time before failure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log identifier</returns>
    private async Task<string> LogTransmissionFailureAsync(
        string transmissionId,
        SendORUMessageCommand command,
        string errorMessage,
        TimeSpan responseTime,
        CancellationToken cancellationToken)
    {
        var log = new HL7TransmissionLog
        {
            TransmissionId = transmissionId,
            Endpoint = command.DestinationEndpoint,
            HL7MessageType = command.MessageData.MessageType.ToString(),
            PatientId = command.MessageData.Patient.PatientId,
            SentAt = DateTime.UtcNow,
            Success = false,
            ErrorMessage = errorMessage,
            ResponseTime = responseTime,
            Source = command.Source,
            Protocol = command.Protocol,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                CommandId = command.CommandId,
                Headers = command.RequestHeaders,
                TimeoutSeconds = command.TimeoutSeconds,
                ObservationCount = command.MessageData.Observations.Count
            })
        };

        var savedLog = await _transmissionRepository.SaveTransmissionLogAsync(log, cancellationToken);
        return savedLog.TransmissionId;
    }
}
