using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Authentication.WebAssembly.Msal;

namespace HL7ResultsGateway.Client.Core.Extensions;

/// <summary>
/// Extension methods for configuring authentication services.
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds MSAL authentication services with configuration binding.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMsalAuthentication(options =>
        {
            configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
        });

        return services;
    }
}
