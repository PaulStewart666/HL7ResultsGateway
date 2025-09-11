using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Core.Extensions;

/// <summary>
/// Extension methods for registering core infrastructure services.
/// </summary>
public static class CoreServiceExtensions
{
    /// <summary>
    /// Adds all core infrastructure services to the DI container.
    /// This includes HttpClient configuration, theme services, and error handling.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration services
        services.AddAppConfiguration(configuration);
        services.ValidateConfiguration();
        
        // Add HTTP client services
        services.AddHttpClientServices(configuration);
        
        // Add authentication services
        services.AddAuthenticationServices(configuration);
        
        // Add theme services
        services.AddThemeServices();
        
        return services;
    }
}