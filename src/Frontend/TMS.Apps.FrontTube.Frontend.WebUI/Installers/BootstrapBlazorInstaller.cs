using BootstrapBlazor.Components;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures BootstrapBlazor services.
/// </summary>
internal static class BootstrapBlazorInstaller
{
    /// <summary>
    /// Adds BootstrapBlazor services to the application.
    /// </summary>
    internal static IServiceCollection AddBootstrapBlazorServices(this IServiceCollection services)
    {
        services.AddBootstrapBlazor();
        return services;
    }
}
