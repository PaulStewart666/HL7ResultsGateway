using HL7ResultsGateway.Client.Features.JsonToHL7.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Features.JsonToHL7.Extensions;

/// <summary>
/// Extension methods for configuring JsonToHL7 feature services.
/// </summary>
public static class JsonToHL7ServiceExtensions
{
    /// <summary>
    /// Adds JsonToHL7 feature services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJsonToHL7Services(this IServiceCollection services)
    {
        // Register JsonToHL7 service
        services.AddScoped<JsonToHL7Service>();

        return services;
    }
}