using Microsoft.Extensions.DependencyInjection;
using TMS.Libs.Frontend.Web.DockPanels.Services;

namespace TMS.Libs.Frontend.Web.DockPanels;

/// <summary>
/// Extension methods for registering dock panels services.
/// </summary>
public static class DockPanelsServiceCollectionExtensions
{
    /// <summary>
    /// Adds dock panels services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDockPanels(this IServiceCollection services)
    {
        services.AddScoped<IDockPanelInterop, DockPanelInterop>();

        return services;
    }
}
