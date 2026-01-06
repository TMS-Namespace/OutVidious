using TMS.Apps.FrontTube.Frontend.WebUI.Services;
using TMS.Libs.Frontend.Web.DockViewWrapper;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures the application services.
/// </summary>
internal static class ServicesInstaller
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<Orchestrator>();
        services.AddDockViewWrapper();
        services.AddScoped<BrowserConsoleCapture>();

        return services;
    }
}
