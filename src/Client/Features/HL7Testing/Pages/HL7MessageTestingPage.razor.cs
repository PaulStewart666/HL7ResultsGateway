using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;
using System.Text.Json;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Pages;

public partial class HL7MessageTestingPage
{
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
        await JSRuntime.InvokeVoidAsync("showToast", "success", "Message validation passed", "The HL7 message format appears to be valid.");
    }

    private void ClearGlobalError()
    {
        _globalError = null;
    }

    private void ViewResult(HL7ProcessingResult result)
    {
        _currentResult = result;
        StateHasChanged();

        // Scroll to results panel
        JSRuntime.InvokeVoidAsync("scrollToElement", "processing-result-heading");
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

        await JSRuntime.InvokeVoidAsync("downloadFile", fileName, "application/json", json);
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

            var json = JsonSerializer.Serialize(historyToSave, options);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "hl7-testing-history", json);
        }
        catch (Exception)
        {
            // If saving fails, continue without persistence
            // Could log this error in a real application
        }
    }
}
