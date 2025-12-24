namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures API controllers.
/// </summary>
internal static class ControllersInstaller
{
    /// <summary>
    /// Adds API controllers support to the application.
    /// </summary>
    internal static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
