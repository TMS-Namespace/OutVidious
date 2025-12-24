using Microsoft.AspNetCore.Mvc;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures application endpoints and middleware.
/// </summary>
internal static class EndpointsInstaller
{
    /// <summary>
    /// Maps all application endpoints including proxy endpoints, API controllers, and Blazor components.
    /// </summary>
    internal static WebApplication AddEndpoints(this WebApplication app)
    {
        // Map proxy endpoints for video and image playback
        app.MapProxyEndpoints();
        
        // Map Blazor components
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    /// <summary>
    /// Maps proxy endpoints for DASH manifests, video playback, and images.
    /// </summary>
    private static WebApplication MapProxyEndpoints(this WebApplication app)
    {
        // Proxy endpoint for DASH manifest to avoid CORS issues (supports both GET and HEAD)
        app.MapMethods("/api/proxy/dash/{videoId}", new[] { "GET", "HEAD" }, 
            async (string videoId, Orchestrator orchestrator, HttpContext context, CancellationToken cancellationToken) =>
            {
                return await orchestrator.Super.Proxy.ProxyDashManifestAsync(videoId, context, cancellationToken);
            });

        // Proxy endpoint for video playback segments to avoid CORS issues
        // This handles both /api/proxy/videoplayback and legacy /videoplayback paths
        app.MapMethods("/api/proxy/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, 
            async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
            {
                await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
            });
        
        // Also support /videoplayback for backwards compatibility and any edge cases
        app.MapMethods("/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, 
            async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
            {
                await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
            });
        
        // Handle /companion/videoplayback URLs that might appear in manifests
        app.MapMethods("/companion/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, 
            async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
            {
                await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
            });

        // Image proxy endpoint with caching support
        app.MapGet("/api/ImageProxy", async (
            [FromQuery] string originalUrl,
            [FromQuery] string fetchUrl,
            Orchestrator orchestrator,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            return await orchestrator.Super.Proxy.ProxyImageAsync(originalUrl, fetchUrl, context, cancellationToken);
        });

        return app;
    }
}
