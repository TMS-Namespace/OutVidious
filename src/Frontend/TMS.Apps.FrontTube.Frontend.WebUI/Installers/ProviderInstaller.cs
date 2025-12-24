using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures video provider services (Invidious).
/// </summary>
internal static class ProviderInstaller
{
    /// <summary>
    /// Adds Invidious video provider with SSL bypass for self-signed certificates.
    /// </summary>
    internal static IServiceCollection AddInvidiousProvider(this IServiceCollection services, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);

        services.AddHttpClient<IProvider, InvidiousVideoProvider>(client =>
        {
            client.BaseAddress = baseUrl;
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // For self-signed certificates in development
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddSingleton<IProvider>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(IProvider));
            var logger = sp.GetRequiredService<ILogger<InvidiousVideoProvider>>();
            return new InvidiousVideoProvider(httpClient, logger, baseUrl);
        });

        return services;
    }
}
