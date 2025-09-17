namespace HL7ResultsGateway.Client.Core.Configuration;

/// <summary>
/// Root configuration class that contains all application configuration sections.
/// </summary>
public class AppConfiguration
{
    public const string SectionName = "";

    public ApiConfiguration Api { get; set; } = new();
    public FeatureConfiguration Features { get; set; } = new();
}
