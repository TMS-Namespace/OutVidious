using MudBlazor.Services;
using Serilog;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager;
using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;
using TMS.Apps.FrontTube.Frontend.WebUI.Components;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

// Configure Serilog - Find solution root for log file location
var solutionRoot = Directory.GetCurrentDirectory();
var searchDir = new DirectoryInfo(solutionRoot);
while (searchDir != null && !searchDir.GetFiles("*.sln").Any())
{
    searchDir = searchDir.Parent;
}
var logPath = searchDir != null 
    ? Path.Combine(searchDir.FullName, "logs", "front-tube-.log")
    : Path.Combine(AppContext.BaseDirectory, "logs", "front-tube-.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore.Components", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", Serilog.Events.LogEventLevel.Information)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting FrontTube WebUI application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add MudBlazor
    builder.Services.AddMudServices();

    // Configure Invidious video provider
    var invidiousBaseUrl = new Uri("https://youtube.srv1.tms.com");

    builder.Services.AddHttpClient<IVideoProvider, InvidiousVideoProvider>(client =>
    {
        client.BaseAddress = invidiousBaseUrl;
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // For self-signed certificates in development
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

    builder.Services.AddSingleton<IVideoProvider>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(IVideoProvider));
        var logger = sp.GetRequiredService<ILogger<InvidiousVideoProvider>>();
        return new InvidiousVideoProvider(httpClient, logger, invidiousBaseUrl);
    });

    // Configure DataRepository
    var dataRepositoryConfig = new DataRepositoryConfig
    {
        DataBase = new DataBaseConfig
        {
            Host = "localhost",
            Port = 5656,
            Database = "ftube",
            Username = "root",
            Password = "password"
        }
    };
    builder.Services.AddSingleton(dataRepositoryConfig);

    // Register DataRepository as singleton (shared cache across all requests)
    builder.Services.AddSingleton<IDataRepository>(sp =>
    {
        var config = sp.GetRequiredService<DataRepositoryConfig>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        return new DataRepository(config, loggerFactory);
    });

    // Register Orchestrator as scoped service (one per user session)
    builder.Services.AddScoped<Orchestrator>();

    // Add API controllers support
    builder.Services.AddControllers();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    // Proxy endpoint for DASH manifest to avoid CORS issues (supports both GET and HEAD)
    app.MapMethods("/api/proxy/dash/{videoId}", new[] { "GET", "HEAD" }, async (string videoId, IVideoProvider videoProvider, HttpContext context) =>
    {
        Log.Debug("DASH manifest proxy request: {Method} {VideoId}", context.Request.Method, videoId);
        
        try
        {
            var dashUrl = videoProvider.GetDashManifestUrl(videoId);
            if (dashUrl == null)
            {
                Log.Warning("Provider does not support DASH manifest for video: {VideoId}", videoId);
                return Results.NotFound("DASH manifest not supported");
            }
            Log.Debug("Fetching DASH manifest from: {DashUrl}", dashUrl);
            
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            
            var response = await httpClient.GetAsync(dashUrl);
            Log.Debug("DASH manifest response: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Warning("DASH manifest fetch failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Results.StatusCode((int)response.StatusCode);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            Log.Debug("DASH manifest content length: {Length} chars", content.Length);
            
            // Replace all video URLs to route through our proxy
            // The manifest contains URLs like https://host/videoplayback?... or /videoplayback?...
            // We need to route them through our /api/proxy/videoplayback endpoint
            var baseUrl = videoProvider.BaseUrl.ToString().TrimEnd('/');
            
            // Replace absolute URLs with our proxy
            content = content.Replace($"{baseUrl}/videoplayback", "/api/proxy/videoplayback");
            
            // Replace relative URLs that might include host info in query params
            // The manifest may have URLs like //host/videoplayback or just /videoplayback
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(https?:)?//[^/""']+/videoplayback",
                "/api/proxy/videoplayback");
            
            Log.Debug("DASH manifest rewritten successfully");
            
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Range, Content-Type");
            
            return Results.Content(content, "application/dash+xml");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to proxy DASH manifest for video {VideoId}", videoId);
            return Results.Problem($"Failed to fetch DASH manifest: {ex.Message}");
        }
    });

    // Proxy endpoint for video playback segments to avoid CORS issues
    // This handles both /api/proxy/videoplayback and legacy /videoplayback paths
    app.MapMethods("/api/proxy/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, IVideoProvider videoProvider) =>
    {
        await ProxyVideoPlaybackAsync(context, videoProvider);
    });
    
    // Also support /videoplayback for backwards compatibility and any edge cases
    app.MapMethods("/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, IVideoProvider videoProvider) =>
    {
        await ProxyVideoPlaybackAsync(context, videoProvider);
    });
    
    // Handle /companion/videoplayback URLs that might appear in manifests
    app.MapMethods("/companion/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, IVideoProvider videoProvider) =>
    {
        await ProxyVideoPlaybackAsync(context, videoProvider);
    });

    async Task ProxyVideoPlaybackAsync(HttpContext context, IVideoProvider videoProvider)
    {
        var queryString = context.Request.QueryString.Value ?? "";
        
        // Check if there's a host parameter in the query string (used by YouTube CDN)
        var hostMatch = System.Text.RegularExpressions.Regex.Match(queryString, @"[&?]host=([^&]+)");
        string proxyUrl;
        
        if (hostMatch.Success)
        {
            // Use the host from query parameter for direct YouTube CDN access
            var cdnHost = System.Net.WebUtility.UrlDecode(hostMatch.Groups[1].Value);
            proxyUrl = $"https://{cdnHost}/videoplayback{queryString}";
            Log.Debug("Video proxy using CDN host: {CdnHost}, Method: {Method}", cdnHost, context.Request.Method);
        }
        else
        {
            // Fallback to provider proxy
            var baseUrl = videoProvider.BaseUrl.ToString().TrimEnd('/');
            proxyUrl = $"{baseUrl}/videoplayback{queryString}";
            Log.Debug("Video proxy using provider: {BaseUrl}, Method: {Method}", baseUrl, context.Request.Method);
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
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            
            var method = context.Request.Method == "HEAD" ? HttpMethod.Head : HttpMethod.Get;
            var request = new HttpRequestMessage(method, proxyUrl);
            
            // Forward range headers for video seeking
            if (context.Request.Headers.TryGetValue("Range", out var rangeHeader))
            {
                request.Headers.TryAddWithoutValidation("Range", rangeHeader.ToString());
                Log.Debug("Forwarding Range header: {Range}", rangeHeader.ToString());
            }
            
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Log.Debug("Video proxy response: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Video proxy failed: {StatusCode} for URL: {Url}", (int)response.StatusCode, proxyUrl);
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
                await response.Content.CopyToAsync(context.Response.Body);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to proxy video playback: {Url}", proxyUrl);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Failed to proxy video: {ex.Message}");
        }
    }

    // Map API controllers (like ImageProxyController)
    app.MapControllers();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
