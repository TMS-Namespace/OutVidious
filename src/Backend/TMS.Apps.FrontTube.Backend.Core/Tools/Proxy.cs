using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

/// <summary>
/// Handles proxy functionality for video playback, DASH manifests, and image fetching.
/// Provides URL generation and HTTP proxying to bypass CORS restrictions.
/// </summary>
public sealed partial class Proxy
{
    private readonly ILogger<Proxy> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _providerBaseUrl;
    private readonly Action<HttpClientHandler>? _httpHandlerConfigurator;

    public Proxy(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, Uri providerBaseUrl, Action<HttpClientHandler>? httpHandlerConfigurator = null)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _providerBaseUrl = providerBaseUrl ?? throw new ArgumentNullException(nameof(providerBaseUrl));
        _httpHandlerConfigurator = httpHandlerConfigurator;
        
        _logger = loggerFactory.CreateLogger<Proxy>();
        _logger.LogDebug("Proxy initialized with provider base URL: {BaseUrl}", _providerBaseUrl);
    }

    /// <summary>
    /// Gets the provider base URL.
    /// </summary>
    public Uri ProviderBaseUrl => _providerBaseUrl;

    #region Internal Proxy URL Generation Methods

    /// <summary>
    /// Gets the watch/player URL for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The watch URL.</returns>
    internal Uri ProxyWatchVideoRemoteUrl(string videoId)
    {
        YouTubeValidator.ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"watch?v={Uri.EscapeDataString(videoId)}");
    }

    /// <summary>
    /// Gets the DASH manifest URL for adaptive streaming.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The DASH manifest URL.</returns>
    internal Uri ProxyDashManifestRemoteUrl(string videoId)
    {
        YouTubeValidator.ValidateVideoIdNotEmpty(videoId);
        // Use local=true for proxying through Invidious (avoids CORS issues)
        // Use unique_res=1 to ensure unique resolutions in the manifest
        return CreateUri($"api/manifest/dash/id/{Uri.EscapeDataString(videoId)}?local=true&unique_res=1");
    }

    /// <summary>
    /// Gets the HLS manifest URL for adaptive streaming.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The HLS manifest URL.</returns>
    internal Uri ProxyHlsManifestRemoteUrl(string videoId)
    {
        YouTubeValidator.ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"api/manifest/hls_variant/{Uri.EscapeDataString(videoId)}");
    }

    /// <summary>
    /// Gets a local proxied DASH manifest URL that bypasses CORS restrictions.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The proxied DASH manifest URL as a relative URI.</returns>
    internal Uri ProxyDashManifestLocalUrl(string videoId)
    {
        YouTubeValidator.ValidateVideoIdNotEmpty(videoId);
        // Returns a local proxy endpoint to avoid CORS issues
        return new Uri($"/api/proxy/dash/{Uri.EscapeDataString(videoId)}", UriKind.Relative);
    }

    /// <summary>
    /// Gets the embed URL for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>The embed URL.</returns>
    internal Uri ProxyEmbedUrl(string videoId)
    {
        YouTubeValidator.ValidateVideoIdNotEmpty(videoId);
        return CreateUri($"embed/{Uri.EscapeDataString(videoId)}?autoplay=1&local=true");
    }

    /// <summary>
    /// Gets the URL to a channel page.
    /// </summary>
    /// <param name="channelId">The channel identifier.</param>
    /// <returns>The channel page URL.</returns>
    internal Uri ProxyChannelUrl(string channelId)
    {
        YouTubeValidator.ValidateChannelIdNotEmpty(channelId);
        return CreateUri($"channel/{Uri.EscapeDataString(channelId)}");
    }

    /// <summary>
    /// Constructs the provider-specific fetch URL for an image from its original YouTube URL.
    /// </summary>
    /// <param name="originalUrl">The original YouTube CDN URL.</param>
    /// <returns>The provider URL to fetch the image from.</returns>
    internal Uri ProxyImageRemoteUrl(Uri originalUrl)
    {
        ArgumentNullException.ThrowIfNull(originalUrl);

        var host = originalUrl.Host.ToLowerInvariant();
        var path = originalUrl.AbsolutePath;

        // Video thumbnails from i.ytimg.com: /vi/VIDEO_ID/quality.jpg -> {baseUrl}/vi/VIDEO_ID/quality.jpg
        if (host is "i.ytimg.com" or "i1.ytimg.com" or "i2.ytimg.com" or "i3.ytimg.com" or "i4.ytimg.com")
        {
            // Path already has /vi/..., just append to base URL
            return CreateUri(path.TrimStart('/'));
        }

        // Channel images from yt3.ggpht.com: /... -> {baseUrl}/ggpht/...
        if (host == "yt3.ggpht.com")
        {
            return CreateUri($"ggpht{path}");
        }

        // For other URLs (e.g., googleusercontent), construct ggpht proxy path
        if (host.EndsWith("googleusercontent.com"))
        {
            // Extract the path after the domain and construct ggpht proxy
            return CreateUri($"ggpht{path}");
        }

        // Fallback: try to proxy as-is (may not work for all URLs)
        _logger.LogWarning("Unknown image URL host, attempting direct proxy: {OriginalUrl}", originalUrl);
        return originalUrl;
    }

    #endregion

    #region Public Endpoint Handlers

    /// <summary>
    /// Proxies a DASH manifest request to avoid CORS issues.
    /// Rewrites manifest URLs to route through the video playback proxy.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>IResult for the endpoint response.</returns>
    public async Task<IResult> ProxyDashManifestAsync(string videoId, HttpContext context, CancellationToken cancellationToken)
    {
        _logger.LogDebug("DASH manifest proxy request: {Method} {VideoId}", context.Request.Method, videoId);
        
        try
        {
            var dashUrl = ProxyDashManifestRemoteUrl(videoId);
            _logger.LogDebug("Fetching DASH manifest from: {DashUrl}", dashUrl);
            
            using var httpClient = CreateConfiguredHttpClient();
            
            var response = await httpClient.GetAsync(dashUrl, cancellationToken);
            _logger.LogDebug("DASH manifest response: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("DASH manifest fetch failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Results.StatusCode((int)response.StatusCode);
            }
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("DASH manifest content length: {Length} chars", content.Length);
            
            // Replace all video URLs to route through our proxy
            var baseUrl = _providerBaseUrl.ToString().TrimEnd('/');
            
            // Replace absolute URLs with our proxy
            content = content.Replace($"{baseUrl}/videoplayback", "/api/proxy/videoplayback");
            
            // Replace relative URLs that might include host info in query params
            content = VideoPlaybackUrlRegex().Replace(content, "/api/proxy/videoplayback");
            
            _logger.LogDebug("DASH manifest rewritten successfully");
            
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Range, Content-Type");
            
            return Results.Content(content, "application/dash+xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy DASH manifest for video {VideoId}", videoId);
            return Results.Problem($"Failed to fetch DASH manifest: {ex.Message}");
        }
    }

    /// <summary>
    /// Proxies video playback segments to avoid CORS issues.
    /// Handles range requests for video seeking.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ProxyVideoPlaybackAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var queryString = context.Request.QueryString.Value ?? "";
        
        // Check if there's a host parameter in the query string (used by YouTube CDN)
        var hostMatch = HostParameterRegex().Match(queryString);
        string proxyUrl;
        
        if (hostMatch.Success)
        {
            // Use the host from query parameter for direct YouTube CDN access
            var cdnHost = System.Net.WebUtility.UrlDecode(hostMatch.Groups[1].Value);
            proxyUrl = $"https://{cdnHost}/videoplayback{queryString}";
            _logger.LogDebug("Video proxy using CDN host: {CdnHost}, Method: {Method}", cdnHost, context.Request.Method);
        }
        else
        {
            // Fallback to provider proxy
            var baseUrl = _providerBaseUrl.ToString().TrimEnd('/');
            proxyUrl = $"{baseUrl}/videoplayback{queryString}";
            _logger.LogDebug("Video proxy using provider: {BaseUrl}, Method: {Method}", baseUrl, context.Request.Method);
        }
        
        // Handle CORS preflight
        if (context.Request.Method == "OPTIONS")
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Range, Content-Type");
            context.Response.Headers.Append("Access-Control-Max-Age", "86400");
            context.Response.StatusCode = 204;
            return;
        }
        
        try
        {
            using var httpClient = CreateConfiguredHttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            
            var method = context.Request.Method == "HEAD" ? HttpMethod.Head : HttpMethod.Get;
            var request = new HttpRequestMessage(method, proxyUrl);
            
            // Forward range headers for video seeking
            if (context.Request.Headers.TryGetValue("Range", out var rangeHeader))
            {
                request.Headers.TryAddWithoutValidation("Range", rangeHeader.ToString());
                _logger.LogDebug("Forwarding Range header: {Range}", rangeHeader.ToString());
            }
            
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            _logger.LogDebug("Video proxy response: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Video proxy failed: {StatusCode} for URL: {Url}", (int)response.StatusCode, proxyUrl);
            }
            
            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "video/mp4";
            
            // Forward relevant headers
            if (response.Content.Headers.ContentLength.HasValue)
            {
                context.Response.Headers.ContentLength = response.Content.Headers.ContentLength.Value;
            }
            if (response.Content.Headers.ContentRange != null)
            {
                context.Response.Headers.Append("Content-Range", response.Content.Headers.ContentRange.ToString());
            }
            context.Response.Headers.Append("Accept-Ranges", "bytes");
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Range, Content-Type");
            context.Response.Headers.Append("Access-Control-Expose-Headers", "Content-Length, Content-Range, Accept-Ranges");
            
            // Only copy body for GET requests
            if (method == HttpMethod.Get)
            {
                await response.Content.CopyToAsync(context.Response.Body, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy video playback: {Url}", proxyUrl);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Failed to proxy video: {ex.Message}", cancellationToken);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates a URI by combining the base URL with a relative path.
    /// </summary>
    /// <param name="relativePath">The relative path to append.</param>
    /// <returns>The combined URI.</returns>
    private Uri CreateUri(string relativePath)
    {
        var baseUrlString = _providerBaseUrl.ToString().TrimEnd('/');
        var path = relativePath.TrimStart('/');
        return new Uri($"{baseUrlString}/{path}");
    }

    /// <summary>
    /// Creates an HttpClient with the configured handler settings.
    /// </summary>
    /// <returns>A configured HttpClient instance.</returns>
    private HttpClient CreateConfiguredHttpClient()
    {
        if (_httpHandlerConfigurator == null)
        {
            // No custom configuration, use factory default
            return _httpClientFactory.CreateClient();
        }

        // Create handler and apply custom configuration
        var handler = new HttpClientHandler();
        _httpHandlerConfigurator(handler);
        return new HttpClient(handler);
    }

    [GeneratedRegex(@"(https?:)?//[^/""']+/videoplayback")]
    private static partial Regex VideoPlaybackUrlRegex();

    [GeneratedRegex(@"[&?]host=([^&]+)")]
    private static partial Regex HostParameterRegex();

    #endregion
}
