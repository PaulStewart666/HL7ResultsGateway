using Microsoft.Extensions.DependencyInjection;
using HL7ResultsGateway.Client.Core.Theming;

namespace HL7ResultsGateway.Client.Core.DependencyInjection;

public static class ThemeServiceRegistration
{
    public static IServiceCollection AddThemeService(this IServiceCollection services)
    {
        services.AddScoped<IThemeService, ThemeService>();
        return services;
    }
}
