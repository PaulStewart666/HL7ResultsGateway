using Microsoft.AspNetCore.Components;
using HL7ResultsGateway.Client.Features.HL7Testing.Models;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Components;

public partial class HL7MessageInputComponent : ComponentBase
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

    protected List<HL7TestMessage> _testMessages = new();
    protected List<string> _testMessageCategories = new();
    protected HL7TestMessage? _selectedTestMessage;
    protected bool _isProcessing;
    protected List<string> _validationErrors = new();

    protected override async Task OnInitializedAsync()
    {
        _testMessages = await TestMessageRepository.GetAllTestMessagesAsync();
        _testMessageCategories = _testMessages.Select(m => m.Category).Distinct().ToList();
        _isProcessing = IsProcessing;
    }

    protected async Task OnTestMessageSelected(ChangeEventArgs e)
    {
        if (e.Value?.ToString() is string messageId && !string.IsNullOrEmpty(messageId))
        {
            _selectedTestMessage = _testMessages.FirstOrDefault(m => m.Id == messageId);
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
}
