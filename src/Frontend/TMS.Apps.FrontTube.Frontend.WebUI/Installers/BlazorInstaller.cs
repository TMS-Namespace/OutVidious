namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures Blazor components and services.
/// </summary>
internal static class BlazorInstaller
{
    /// <summary>
    /// Adds Blazor Razor Components with interactive server-side rendering.
    /// </summary>
    internal static IServiceCollection AddBlazorComponents(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        return services;
    }
}
