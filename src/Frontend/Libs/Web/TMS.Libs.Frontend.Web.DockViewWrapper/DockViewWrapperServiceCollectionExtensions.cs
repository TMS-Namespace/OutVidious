using Microsoft.Extensions.DependencyInjection;
using TMS.Libs.Frontend.Web.DockViewWrapper.Services;

namespace TMS.Libs.Frontend.Web.DockViewWrapper;

/// <summary>
/// Extension methods for registering DockViewWrapper services.
/// </summary>
public static class DockViewWrapperServiceCollectionExtensions
{
    /// <summary>
    /// Adds DockViewWrapper services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDockViewWrapper(this IServiceCollection services)
    {
        services.AddScoped<IDockViewInterop, DockViewInterop>();

        return services;
    }
}
