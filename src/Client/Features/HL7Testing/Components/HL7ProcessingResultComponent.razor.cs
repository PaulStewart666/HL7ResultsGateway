using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Domain.ValueObjects;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Components;

public partial class HL7ProcessingResultComponent
{
    [Parameter] public HL7ProcessingResult? Result { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private async Task ExportAsJson()
    {
        if (Result == null) return;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(Result, options);
        var fileName = $"hl7_result_{Result.RequestId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

        await JSRuntime.InvokeVoidAsync("hl7Testing.downloadFile", fileName, json, "application/json");
    }

    private async Task CopyRequestId()
    {
        if (Result?.RequestId == null) return;

        await JSRuntime.InvokeVoidAsync("hl7Testing.copyToClipboard", Result.RequestId);
        await JSRuntime.InvokeVoidAsync("hl7Testing.showToast", "Request ID copied to clipboard", "success");
    }

    private string _getStatusIcon()
    {
        return Result?.Success == true ? "fa-check-circle text-success" : "fa-exclamation-circle text-danger";
    }

    private string _getStatusBadgeClass()
    {
        return Result?.Success == true ? "bg-success" : "bg-danger";
    }

    private string _getObservationStatusBadgeClass(ObservationStatus status)
    {
        return status switch
        {
            ObservationStatus.Normal => "bg-success",
            ObservationStatus.Abnormal => "bg-warning text-dark",
            ObservationStatus.Critical => "bg-danger",
            ObservationStatus.Pending => "bg-info text-dark",
            _ => "bg-light text-dark"
        };
    }
}
