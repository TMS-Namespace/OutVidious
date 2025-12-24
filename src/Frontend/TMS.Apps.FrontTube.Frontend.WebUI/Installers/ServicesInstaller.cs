using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures the Orchestrator
/// </summary>
internal static class ServicesInstaller
{
    /// <summary>
    /// Adds Orchestrator as a scoped service
    /// </summary>
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<Orchestrator>();    

        return services;
    }
}
