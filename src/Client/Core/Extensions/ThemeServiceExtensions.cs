using Microsoft.Extensions.DependencyInjection;
using HL7ResultsGateway.Client.Core.Theming;

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
        // Add theme management service
        services.AddScoped<IThemeService, ThemeService>();

        // Add user preferences service when implemented
        // services.AddScoped<IUserPreferencesService, UserPreferencesService>();

        return services;
    }
}
