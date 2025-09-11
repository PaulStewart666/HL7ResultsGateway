using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Domain.ValueObjects;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Components;

public partial class HL7ProcessingResultComponent : ComponentBase, IAsyncDisposable
{
    [Parameter] public HL7ProcessingResult? Result { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<HL7ProcessingResultComponent>? _dotNetReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./Features/HL7Testing/Components/HL7ProcessingResultComponent.razor.js");

            _dotNetReference = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initialize", _dotNetReference);
        }
    }

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

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("downloadFile", fileName, json, "application/json");
        }
    }

    private async Task CopyRequestId()
    {
        if (Result?.RequestId == null) return;

        if (_jsModule != null)
        {
            var success = await _jsModule.InvokeAsync<bool>("copyToClipboard", Result.RequestId);
            if (success)
            {
                await _jsModule.InvokeVoidAsync("showToast", "Request ID copied to clipboard", "success");
            }
            else
            {
                await _jsModule.InvokeVoidAsync("showToast", "Failed to copy to clipboard", "error");
            }
        }
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

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Expected when the circuit is disconnected
            }
        }

        _dotNetReference?.Dispose();
    }
}
