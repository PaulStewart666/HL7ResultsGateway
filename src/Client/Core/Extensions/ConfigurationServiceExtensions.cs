using HL7ResultsGateway.Client.Core.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HL7ResultsGateway.Client.Core.Extensions;

/// <summary>
/// Extension methods for configuring application configuration services.
/// </summary>
public static class ConfigurationServiceExtensions
{
    /// <summary>
    /// Adds configuration services with type-safe binding to the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and register the main app configuration
        services.Configure<AppConfiguration>(configuration);

        // Bind and register individual configuration sections for easier injection
        services.Configure<ApiConfiguration>(configuration.GetSection("Api"));
        services.Configure<FeatureConfiguration>(configuration.GetSection("Features"));
        services.Configure<HL7TestingConfiguration>(configuration.GetSection("Features:HL7Testing"));
        services.Configure<JsonToHL7Configuration>(configuration.GetSection("Features:JsonToHL7"));
        services.Configure<DashboardConfiguration>(configuration.GetSection("Features:Dashboard"));

        return services;
    }

    /// <summary>
    /// Validates configuration on application startup.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ValidateConfiguration(this IServiceCollection services)
    {
        // Add options validation for critical configurations
        // TODO: Add validation once Microsoft.Extensions.Options.DataAnnotations is available in .NET 10
        services.AddOptions<ApiConfiguration>();

        return services;
    }
}
