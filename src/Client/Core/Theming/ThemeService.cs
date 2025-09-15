using Microsoft.JSInterop;

namespace HL7ResultsGateway.Client.Core.Theming;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    // Set default to GPW branded theme
    private string _currentTheme = "gpw";
    public string CurrentTheme => _currentTheme;
    public IReadOnlyList<string> AvailableThemes { get; } = new[] { "gpw", "light", "dark", "medical" };
    public event Action<string>? ThemeChanged;
    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async Task InitializeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("getTheme");
            if (AvailableThemes.Contains(theme))
            {
                _currentTheme = theme;
                await _jsRuntime.InvokeVoidAsync("setTheme", theme);
            }
        }
        catch (Exception ex)
        {
            // Fallback to gpw theme if JS interop fails
            Console.WriteLine($"Theme initialization failed: {ex.Message}");
            _currentTheme = "gpw";
        }
    }
    public async Task SetThemeAsync(string themeName)
    {
        if (!AvailableThemes.Contains(themeName)) return;

        _currentTheme = themeName;
        ThemeChanged?.Invoke(themeName);

        try
        {
            await _jsRuntime.InvokeVoidAsync("setTheme", themeName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Theme setting failed: {ex.Message}");
        }
    }
}
