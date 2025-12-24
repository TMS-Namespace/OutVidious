using MudBlazor.Services;
using Serilog;
using TMS.Apps.FrontTube.Backend.Repository.Cache;
using TMS.Apps.FrontTube.Backend.Repository.Cache.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Providers.Invidious;
using TMS.Apps.FrontTube.Frontend.WebUI.Components;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;
using TMS.Apps.FrontTube.Frontend.WebUI;

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

    builder.Services.AddHttpClient<IProvider, InvidiousVideoProvider>(client =>
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

    builder.Services.AddSingleton<IProvider>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(IProvider));
        var logger = sp.GetRequiredService<ILogger<InvidiousVideoProvider>>();
        return new InvidiousVideoProvider(httpClient, logger, invidiousBaseUrl);
    });

    // Configure DataRepository
    var dataRepositoryConfig = new CacheConfig
    {
        DataBase = new DataBaseConfig
        {
            Host = "localhost",
            Port = 5656,
            DatabaseName = "front-tube",
            Username = "root",
            Password = "password"
        }
    };
    builder.Services.AddSingleton(dataRepositoryConfig);

    // Register DataRepository as singleton (shared cache across all requests)
    builder.Services.AddSingleton<ICacheManager>(sp =>
    {
        var config = sp.GetRequiredService<CacheConfig>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        return new CacheManager(config, loggerFactory);
    });

    // Define the HTTP handler configurator for SSL bypass (for self-signed certificates in development)
    static void ConfigureProxyHandler(HttpClientHandler handler)
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    // Register Orchestrator as scoped service (one per user session)
    builder.Services.AddScoped<Orchestrator>(sp =>
    {
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var videoProvider = sp.GetRequiredService<IProvider>();
        var dataRepository = sp.GetRequiredService<ICacheManager>();
        
        return new Orchestrator(loggerFactory, httpClientFactory, videoProvider, dataRepository, ConfigureProxyHandler);
    });

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
    app.MapMethods("/api/proxy/dash/{videoId}", new[] { "GET", "HEAD" }, async (string videoId, Orchestrator orchestrator, HttpContext context, CancellationToken cancellationToken) =>
    {
        return await orchestrator.Super.Proxy.ProxyDashManifestAsync(videoId, context, cancellationToken);
    });

    // Proxy endpoint for video playback segments to avoid CORS issues
    // This handles both /api/proxy/videoplayback and legacy /videoplayback paths
    app.MapMethods("/api/proxy/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
    {
        await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
    });
    
    // Also support /videoplayback for backwards compatibility and any edge cases
    app.MapMethods("/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
    {
        await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
    });
    
    // Handle /companion/videoplayback URLs that might appear in manifests
    app.MapMethods("/companion/videoplayback", new[] { "GET", "HEAD", "OPTIONS" }, async (HttpContext context, Orchestrator orchestrator, CancellationToken cancellationToken) =>
    {
        await orchestrator.Super.Proxy.ProxyVideoPlaybackAsync(context, cancellationToken);
    });

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
