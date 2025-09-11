using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Core.Extensions;

/// <summary>
/// Extension methods for configuring theme-related services.
/// </summary>
public static class ThemeServiceExtensions
{
    /// <summary>
    /// Adds theme management services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddThemeServices(this IServiceCollection services)
    {
        // Add theme-related services when they are implemented
        // services.AddScoped<IThemeService, ThemeService>();
        // services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        
        return services;
    }
}