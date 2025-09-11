namespace HL7ResultsGateway.Client.Core.Configuration;

public class FeatureConfiguration
{
    public HL7TestingConfiguration HL7Testing { get; set; } = new();
    public JsonToHL7Configuration JsonToHL7 { get; set; } = new();
    public DashboardConfiguration Dashboard { get; set; } = new();
}

public class HL7TestingConfiguration
{
    public bool Enabled { get; set; } = true;
    public int MaxMessageSize { get; set; } = 1048576; // 1MB
}

public class JsonToHL7Configuration
{
    public bool Enabled { get; set; } = true;
    public bool ValidationEnabled { get; set; } = true;
}

public class DashboardConfiguration
{
    public bool Enabled { get; set; } = true;
    public int RefreshIntervalSeconds { get; set; } = 30;

    public TimeSpan RefreshInterval => TimeSpan.FromSeconds(RefreshIntervalSeconds);
}
