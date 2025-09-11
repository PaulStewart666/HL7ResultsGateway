using HL7ResultsGateway.Client.Features.JsonToHL7.Models;
using HL7ResultsGateway.Domain.Entities;
using HL7ResultsGateway.Domain.ValueObjects;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.JsonToHL7.Services;

public interface IJsonToHL7Service
{
    Task<JsonToHL7Result> ConvertJsonToHL7Async(JsonToHL7Request request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for communicating with the JSON to HL7 conversion API
/// </summary>
public class JsonToHL7Service : IJsonToHL7Service
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonToHL7Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<JsonToHL7Result> ConvertJsonToHL7Async(JsonToHL7Request request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        try
        {
            // Convert the request to the format expected by the API
            var apiRequest = ConvertToApiRequest(request);

            // Make the API call
            var response = await _httpClient.PostAsJsonAsync("api/convert-json-to-hl7", apiRequest, _jsonOptions, cancellationToken);
            stopwatch.Stop();

            // Log response details for debugging
            Console.WriteLine($"Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Error Response: {errorContent}");

                return new JsonToHL7Result
                {
                    Success = false,
                    ErrorMessage = $"API request failed with status {response.StatusCode}: {errorContent}",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingTime = stopwatch.Elapsed,
                    RequestId = requestId
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Response JSON: {jsonResponse}");

            var apiResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse, _jsonOptions);

            if (!apiResponse.TryGetProperty("success", out var successProperty))
            {
                return new JsonToHL7Result
                {
                    Success = false,
                    ErrorMessage = "Invalid API response format",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingTime = stopwatch.Elapsed,
                    RequestId = requestId
                };
            }

            var success = successProperty.GetBoolean();

            if (!success)
            {
                var errorMessage = apiResponse.TryGetProperty("errorMessage", out var errorProp)
                    ? errorProp.GetString()
                    : "Unknown error occurred";

                var validationErrors = new List<string>();
                if (apiResponse.TryGetProperty("validationResult", out var validationResultProp))
                {
                    if (validationResultProp.TryGetProperty("errors", out var errorsProp))
                    {
                        if (errorsProp.ValueKind == JsonValueKind.Array)
                        {
                            validationErrors = errorsProp.EnumerateArray()
                                .Where(x => x.ValueKind == JsonValueKind.String)
                                .Select(x => x.GetString()!)
                                .ToList();
                        }
                    }
                }

                return new JsonToHL7Result
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    ValidationErrors = validationErrors,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingTime = stopwatch.Elapsed,
                    RequestId = requestId
                };
            }

            // Extract the HL7 message and parsed result
            var hl7Message = apiResponse.TryGetProperty("hl7Message", out var hl7Prop)
                ? hl7Prop.GetString()
                : null;

            HL7Result? parsedMessage = null;
            if (apiResponse.TryGetProperty("parsedMessage", out var parsedProp))
            {
                parsedMessage = ParseHL7Result(parsedProp);
            }

            return new JsonToHL7Result
            {
                Success = true,
                HL7Message = hl7Message,
                ParsedResult = parsedMessage,
                ProcessedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed,
                RequestId = requestId
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Exception in ConvertJsonToHL7Async: {ex}");

            return new JsonToHL7Result
            {
                Success = false,
                ErrorMessage = $"HTTP request failed: {ex.Message}",
                ProcessedAt = DateTime.UtcNow,
                ProcessingTime = stopwatch.Elapsed,
                RequestId = requestId
            };
        }
    }

    private static object ConvertToApiRequest(JsonToHL7Request request)
    {
        return new
        {
            patient = new
            {
                patientId = request.Patient.PatientId,
                firstName = request.Patient.FirstName,
                lastName = request.Patient.LastName,
                middleName = request.Patient.MiddleName,
                dateOfBirth = request.Patient.DateOfBirth.ToString("yyyy-MM-dd"),
                gender = request.Patient.Gender,
                address = request.Patient.Address,
                phoneNumber = request.Patient.PhoneNumber
            },
            observations = request.Observations.Select(obs => new
            {
                observationId = obs.ObservationId,
                description = obs.Description,
                value = obs.Value,
                units = obs.Units,
                referenceRange = obs.ReferenceRange,
                status = obs.Status,
                valueType = obs.ValueType
            }).ToArray(),
            messageInfo = new
            {
                sendingApplication = request.MessageInfo.SendingApplication,
                sendingFacility = request.MessageInfo.SendingFacility,
                receivingApplication = request.MessageInfo.ReceivingApplication,
                receivingFacility = request.MessageInfo.ReceivingFacility,
                messageControlId = request.MessageInfo.MessageControlId,
                timestamp = request.MessageInfo.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")
            }
        };
    }

    private static HL7Result? ParseHL7Result(JsonElement parsedProp)
    {
        try
        {
            var messageType = ParseMessageType(
                parsedProp.TryGetProperty("messageType", out var msgTypeProp)
                    ? msgTypeProp.GetString() ?? "Unknown"
                    : "Unknown"
            );

            Patient? patient = null;
            if (parsedProp.TryGetProperty("patient", out var patientProp))
            {
                patient = new Patient
                {
                    PatientId = patientProp.TryGetProperty("patientId", out var pidProp) ? pidProp.GetString() ?? "Unknown" : "Unknown",
                    FirstName = patientProp.TryGetProperty("firstName", out var fnProp) ? fnProp.GetString() ?? string.Empty : string.Empty,
                    LastName = patientProp.TryGetProperty("lastName", out var lnProp) ? lnProp.GetString() ?? string.Empty : string.Empty,
                    MiddleName = patientProp.TryGetProperty("middleName", out var mnProp) ? mnProp.GetString() ?? string.Empty : string.Empty,
                    DateOfBirth = patientProp.TryGetProperty("dateOfBirth", out var dobProp) && dobProp.TryGetDateTime(out var dob) ? dob : DateTime.MinValue,
                    Gender = ParseGender(patientProp.TryGetProperty("gender", out var genderProp) ? genderProp.GetString() ?? "U" : "U"),
                    Address = patientProp.TryGetProperty("address", out var addrProp) ? addrProp.GetString() ?? string.Empty : string.Empty
                };
            }

            var observations = new List<Observation>();
            if (parsedProp.TryGetProperty("observations", out var obsProp) && obsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var obsElement in obsProp.EnumerateArray())
                {
                    var observation = new Observation
                    {
                        ObservationId = obsElement.TryGetProperty("observationId", out var obsIdProp) ? obsIdProp.GetString() ?? "" : "",
                        Description = obsElement.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "",
                        Value = obsElement.TryGetProperty("value", out var valueProp) ? valueProp.GetString() ?? "" : "",
                        Units = obsElement.TryGetProperty("units", out var unitsProp) ? unitsProp.GetString() ?? string.Empty : string.Empty,
                        ReferenceRange = obsElement.TryGetProperty("referenceRange", out var refProp) ? refProp.GetString() ?? string.Empty : string.Empty,
                        Status = ParseObservationStatus(obsElement.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "N" : "N"),
                        ValueType = obsElement.TryGetProperty("valueType", out var vtProp) ? vtProp.GetString() ?? "ST" : "ST"
                    };
                    observations.Add(observation);
                }
            }

            return new HL7Result
            {
                MessageType = messageType,
                Patient = patient ?? new Patient { PatientId = "Unknown" },
                Observations = observations
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing HL7Result: {ex.Message}");
            return null;
        }
    }

    private static HL7MessageType ParseMessageType(string messageType)
    {
        return messageType?.ToUpperInvariant() switch
        {
            "ORU_R01" => HL7MessageType.ORU_R01,
            "ADT_A01" => HL7MessageType.ADT_A01,
            "ADT_A03" => HL7MessageType.ADT_A03,
            "ORM_O01" => HL7MessageType.ORM_O01,
            _ => HL7MessageType.Unknown
        };
    }

    private static Gender ParseGender(string gender)
    {
        return gender?.ToUpperInvariant() switch
        {
            "M" => Gender.Male,
            "F" => Gender.Female,
            "O" => Gender.Other,
            "U" => Gender.Unknown,
            _ => Gender.Unknown
        };
    }

    private static ObservationStatus ParseObservationStatus(string status)
    {
        return status?.ToUpperInvariant() switch
        {
            "N" => ObservationStatus.Normal,
            "A" => ObservationStatus.Abnormal,
            "C" => ObservationStatus.Critical,
            "P" => ObservationStatus.Pending,
            _ => ObservationStatus.Unknown
        };
    }
}
