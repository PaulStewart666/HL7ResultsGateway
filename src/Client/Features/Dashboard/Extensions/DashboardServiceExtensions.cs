using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Features.Dashboard.Extensions;

/// <summary>
/// Extension methods for configuring Dashboard feature services.
/// </summary>
public static class DashboardServiceExtensions
{
    /// <summary>
    /// Adds Dashboard feature services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDashboardServices(this IServiceCollection services)
    {
        // Register Dashboard feature services
        // Add dashboard-specific services when implemented

        return services;
    }
}
