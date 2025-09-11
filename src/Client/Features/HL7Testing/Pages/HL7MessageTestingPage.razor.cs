using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Pages;

public partial class HL7MessageTestingPage : ComponentBase, IAsyncDisposable
{
    [Inject] private IHL7MessageService HL7MessageService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<HL7MessageTestingPage>? _dotNetReference;

    private string _currentMessage = string.Empty;
    private string _currentSource = "Manual Entry";
    private HL7ProcessingResult? _currentResult;
    private bool _isProcessing;
    private string? _globalError;
    private readonly List<HL7ProcessingResult> _processingHistory = new();

    protected override async Task OnInitializedAsync()
    {
        // Load any saved history from local storage if needed
        await LoadProcessingHistory();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./Features/HL7Testing/Pages/HL7MessageTestingPage.razor.js");

            _dotNetReference = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initialize", _dotNetReference);
        }
    }

    private async Task OnMessageContentChanged(string content)
    {
        _currentMessage = content;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnMessageSourceChanged(string source)
    {
        _currentSource = source;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnProcessMessage()
    {
        _isProcessing = true;
        _globalError = null;
        _currentResult = null;

        try
        {
            await InvokeAsync(StateHasChanged);

            var result = await HL7MessageService.ProcessMessageAsync(
                _currentMessage,
                _currentSource,
                CancellationToken.None);

            _currentResult = result;
            _processingHistory.Add(result);

            // Save to local storage for persistence
            await SaveProcessingHistory();
        }
        catch (Exception ex)
        {
            _globalError = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnClearMessage()
    {
        _currentMessage = string.Empty;
        _currentResult = null;
        _globalError = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnValidateMessage()
    {
        // Basic validation - could be enhanced with more sophisticated HL7 validation
        _globalError = null;

        if (string.IsNullOrWhiteSpace(_currentMessage))
        {
            _globalError = "Message content cannot be empty";
            return;
        }

        if (!_currentMessage.StartsWith("MSH"))
        {
            _globalError = "HL7 messages must start with MSH segment";
            return;
        }

        // If validation passes, show success message
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("showToast", "Message validation passed", "success");
        }
    }

    private void ClearGlobalError()
    {
        _globalError = null;
    }

    private async Task ViewResult(HL7ProcessingResult result)
    {
        _currentResult = result;
        StateHasChanged();

        // Scroll to results panel
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("scrollToElement", "processing-result-heading");
        }
    }

    private async Task ExportHistory()
    {
        if (!_processingHistory.Any()) return;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(_processingHistory, options);
        var fileName = $"hl7_testing_history_{DateTime.Now:yyyyMMdd_HHmmss}.json";

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("downloadFile", fileName, json, "application/json");
        }
    }

    private async Task ClearHistory()
    {
        _processingHistory.Clear();
        _currentResult = null;
        await SaveProcessingHistory();
        StateHasChanged();
    }

    private double _getSuccessRate()
    {
        if (!_processingHistory.Any()) return 0.0;
        return (_processingHistory.Count(h => h.Success) * 100.0) / _processingHistory.Count;
    }

    private async Task LoadProcessingHistory()
    {
        try
        {
            if (_jsModule != null)
            {
                var historyData = await _jsModule.InvokeAsync<object?>("loadFromLocalStorage", "hl7-testing-history");
                if (historyData != null)
                {
                    var historyJson = JsonSerializer.Serialize(historyData);
                    var history = JsonSerializer.Deserialize<List<HL7ProcessingResult>>(historyJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    });

                    if (history != null)
                    {
                        _processingHistory.AddRange(history);
                    }
                }
            }
            else
            {
                // Fallback to direct localStorage access if module not loaded yet
                var historyJson = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "hl7-testing-history");
                if (!string.IsNullOrEmpty(historyJson))
                {
                    var history = JsonSerializer.Deserialize<List<HL7ProcessingResult>>(historyJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    });

                    if (history != null)
                    {
                        _processingHistory.AddRange(history);
                    }
                }
            }
        }
        catch (Exception)
        {
            // If loading fails, continue with empty history
            // Could log this error in a real application
        }
    }

    private async Task SaveProcessingHistory()
    {
        try
        {
            // Only keep the last 100 results to prevent storage bloat
            var historyToSave = _processingHistory
                .OrderByDescending(h => h.ProcessedAt)
                .Take(100)
                .ToList();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("saveToLocalStorage", "hl7-testing-history", historyToSave);
            }
            else
            {
                // Fallback to direct localStorage access
                var json = JsonSerializer.Serialize(historyToSave, options);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "hl7-testing-history", json);
            }
        }
        catch (Exception)
        {
            // If saving fails, continue without persistence
            // Could log this error in a real application
        }
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
