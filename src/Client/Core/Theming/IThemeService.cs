namespace HL7ResultsGateway.Client.Core.Theming;

public interface IThemeService
{
    string CurrentTheme { get; }
    IReadOnlyList<string> AvailableThemes { get; }
    event Action<string>? ThemeChanged;
    void SetTheme(string themeName);
    void Initialize();
}
