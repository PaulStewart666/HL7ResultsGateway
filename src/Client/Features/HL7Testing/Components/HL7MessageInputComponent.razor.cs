using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Components;

public partial class HL7MessageInputComponent : ComponentBase, IAsyncDisposable
{
    [Parameter] public string MessageContent { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> MessageContentChanged { get; set; }

    [Parameter] public string MessageSource { get; set; } = "manual";
    [Parameter] public EventCallback<string> MessageSourceChanged { get; set; }

    [Parameter] public EventCallback OnProcessMessage { get; set; }
    [Parameter] public EventCallback OnClearMessage { get; set; }
    [Parameter] public EventCallback OnValidateMessage { get; set; }
    [Parameter] public bool IsProcessing { get; set; }

    [Inject] private ITestMessageRepository TestMessageRepository { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<HL7MessageInputComponent>? _dotNetReference;

    protected List<HL7TestMessage> _testMessages = new();
    protected List<string> _testMessageCategories = new();
    protected HL7TestMessage? _selectedTestMessage;
    protected bool _isProcessing;
    protected List<string> _validationErrors = new();

    protected override void OnInitialized()
    {
        _testMessages = TestMessageRepository.GetAllTestMessages().ToList();
        _testMessageCategories = _testMessages.Select(m => m.Category).Distinct().ToList();
        _isProcessing = IsProcessing;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./Features/HL7Testing/Components/HL7MessageInputComponent.razor.js");

            _dotNetReference = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initialize", _dotNetReference);
        }
    }

    protected async Task OnTestMessageSelected(ChangeEventArgs e)
    {
        if (e.Value?.ToString() is string messageName && !string.IsNullOrEmpty(messageName))
        {
            _selectedTestMessage = _testMessages.FirstOrDefault(m => m.Name == messageName);
            if (_selectedTestMessage != null)
            {
                MessageContent = _selectedTestMessage.Content;
                await MessageContentChanged.InvokeAsync(MessageContent);
                MessageSource = "test";
                await MessageSourceChanged.InvokeAsync(MessageSource);
                _validationErrors.Clear();
            }
        }
    }

    protected async Task OnMessageContentChanged(ChangeEventArgs e)
    {
        MessageContent = e.Value?.ToString() ?? string.Empty;
        await MessageContentChanged.InvokeAsync(MessageContent);

        // Clear test message selection when manual content changes
        if (_selectedTestMessage != null && MessageContent != _selectedTestMessage.Content)
        {
            _selectedTestMessage = null;
            MessageSource = "manual";
            await MessageSourceChanged.InvokeAsync(MessageSource);
        }

        // Clear validation errors on content change
        _validationErrors.Clear();
    }

    protected override void OnParametersSet()
    {
        _isProcessing = IsProcessing;
    }

    protected async Task ValidateCurrentMessage()
    {
        if (_jsModule != null && !string.IsNullOrEmpty(MessageContent))
        {
            var result = await _jsModule.InvokeAsync<object>("validateHL7Format", MessageContent);
            // Could process validation result here if needed
        }
    }

    [JSInvokable]
    public async Task OnValidationComplete(bool isValid, string[] errors)
    {
        _validationErrors = errors?.ToList() ?? new List<string>();
        await InvokeAsync(StateHasChanged);
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
