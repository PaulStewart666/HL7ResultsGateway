using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Features.Authentication.Extensions;

/// <summary>
/// Extension methods for configuring Authentication feature services.
/// </summary>
public static class AuthenticationFeatureServiceExtensions
{
    /// <summary>
    /// Adds Authentication feature services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthenticationFeatureServices(this IServiceCollection services)
    {
        // Register Authentication feature services
        // Note: Authentication services (MSAL) are registered at the Core level
        // This extension is for feature-specific authentication-related services
        
        return services;
    }
}