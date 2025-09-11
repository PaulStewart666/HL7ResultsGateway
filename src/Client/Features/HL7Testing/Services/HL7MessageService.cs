using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Domain.Entities;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Services;

public interface IHL7MessageService
{
    Task<HL7ProcessingResult> ProcessMessageAsync(string message, string source = "Unknown", CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for communicating with the HL7 processing API
/// </summary>
public class HL7MessageService : IHL7MessageService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public HL7MessageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<HL7ProcessingResult> ProcessMessageAsync(string message, string source = "Unknown", CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        try
        {
            // Prepare the request content
            var content = new StringContent(message, Encoding.UTF8, "text/plain");

            // Add source as header
            content.Headers.Add("X-Source", source);

            // Make the API call
            var response = await _httpClient.PostAsync("/api/hl7/process", content, cancellationToken);
            stopwatch.Stop();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<HL7ApiResponse>(jsonResponse, _jsonOptions);

            if (apiResponse == null)
            {
                return new HL7ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Failed to deserialize API response",
                    OriginalMessage = message,
                    Source = source,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingTime = stopwatch.Elapsed,
                    RequestId = requestId
                };
            }

            return new HL7ProcessingResult
            {
                Success = apiResponse.Success,
                ErrorMessage = apiResponse.Error,
                OriginalMessage = message,
                Source = source,
                ProcessedAt = apiResponse.ProcessedAt,
                ProcessingTime = stopwatch.Elapsed,
                RequestId = requestId,
                // Note: We would need to make an additional call or extend the API to get full parsed data
                // For now, we'll create a minimal HL7Result from the response
                ParsedMessage = apiResponse.Success ? CreateMinimalHL7Result(apiResponse) : null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HL7ProcessingResult
            {
                Success = false,
                ErrorMessage = $"HTTP request failed: {ex.Message}",
                OriginalMessage = message,
                Source = source,
                ProcessedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed,
                RequestId = requestId
            };
        }
    }

    private static HL7Result? CreateMinimalHL7Result(HL7ApiResponse response)
    {
        if (!response.Success || string.IsNullOrEmpty(response.MessageType))
            return null;

        return new HL7Result
        {
            MessageType = ParseMessageType(response.MessageType),
            Patient = new Patient
            {
                PatientId = response.PatientId ?? "Unknown"
            },
            Observations = new List<Observation>()
        };
    }

    private static Domain.ValueObjects.HL7MessageType ParseMessageType(string messageType)
    {
        return messageType?.ToUpperInvariant() switch
        {
            "ORU_R01" => Domain.ValueObjects.HL7MessageType.ORU_R01,
            "ADT_A01" => Domain.ValueObjects.HL7MessageType.ADT_A01,
            "ADT_A03" => Domain.ValueObjects.HL7MessageType.ADT_A03,
            "ORM_O01" => Domain.ValueObjects.HL7MessageType.ORM_O01,
            _ => Domain.ValueObjects.HL7MessageType.Unknown
        };
    }
}
