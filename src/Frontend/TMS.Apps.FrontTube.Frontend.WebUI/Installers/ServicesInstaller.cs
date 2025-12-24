using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures the Orchestrator service.
/// </summary>
internal static class ServicesInstaller
{
    /// <summary>
    /// Adds Orchestrator as a scoped service with SSL bypass configuration for proxy.
    /// </summary>
    internal static IServiceCollection AddOrchestrator(this IServiceCollection services)
    {
        // Define the HTTP handler configurator for SSL bypass (for self-signed certificates in development)
        static void ConfigureProxyHandler(HttpClientHandler handler)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        services.AddScoped<Orchestrator>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var videoProvider = sp.GetRequiredService<IProvider>();
            var dataRepository = sp.GetRequiredService<ICacheManager>();
            
            return new Orchestrator(loggerFactory, httpClientFactory, videoProvider, dataRepository, ConfigureProxyHandler);
        });

        return services;
    }
}
