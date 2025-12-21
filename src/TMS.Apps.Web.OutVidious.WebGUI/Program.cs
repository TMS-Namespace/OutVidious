using MudBlazor.Services;
using Serilog;
using TMS.Apps.Web.OutVidious.Core.Interfaces;
using TMS.Apps.Web.OutVidious.Core.Services;
using TMS.Apps.Web.OutVidious.WebGUI.Components;

// Configure Serilog - Find solution root for log file location
var solutionRoot = Directory.GetCurrentDirectory();
var searchDir = new DirectoryInfo(solutionRoot);
while (searchDir != null && !searchDir.GetFiles("*.sln").Any())
{
    searchDir = searchDir.Parent;
}
var logPath = searchDir != null 
    ? Path.Combine(searchDir.FullName, "logs", "outvidious-.log")
    : Path.Combine(AppContext.BaseDirectory, "logs", "outvidious-.log");

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
    Log.Information("Starting OutVidious WebGUI application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add MudBlazor
    builder.Services.AddMudServices();

    // Configure Invidious API service
    const string invidiousBaseUrl = "https://youtube.srv1.tms.com";

    builder.Services.AddHttpClient<IInvidiousApiService, InvidiousApiService>(client =>
    {
        client.BaseAddress = new Uri(invidiousBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // For self-signed certificates in development
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

    builder.Services.AddSingleton<IInvidiousApiService>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(IInvidiousApiService));
        var logger = sp.GetRequiredService<ILogger<InvidiousApiService>>();
        return new InvidiousApiService(httpClient, logger, invidiousBaseUrl);
    });

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
