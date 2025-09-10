using Microsoft.JSInterop;

namespace HL7ResultsGateway.Client.Core.Theming;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";
    public string CurrentTheme => _currentTheme;
    public IReadOnlyList<string> AvailableThemes { get; } = new[] { "light", "dark", "medical" };
    public event Action<string>? ThemeChanged;
    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async void Initialize()
    {
        var theme = await _jsRuntime.InvokeAsync<string>("getTheme");
        if (AvailableThemes.Contains(theme))
        {
            _currentTheme = theme;
            await _jsRuntime.InvokeVoidAsync("setTheme", theme);
        }
    }
    public async void SetTheme(string themeName)
    {
        if (!AvailableThemes.Contains(themeName)) return;
        _currentTheme = themeName;
        ThemeChanged?.Invoke(themeName);
        await _jsRuntime.InvokeVoidAsync("setTheme", themeName);
    }
}
