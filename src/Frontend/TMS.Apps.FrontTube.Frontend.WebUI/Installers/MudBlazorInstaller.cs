using MudBlazor.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures MudBlazor services.
/// </summary>
internal static class MudBlazorInstaller
{
    /// <summary>
    /// Adds MudBlazor services to the application.
    /// </summary>
    internal static IServiceCollection AddMudBlazor(this IServiceCollection services)
    {
        services.AddMudServices();
        return services;
    }
}
