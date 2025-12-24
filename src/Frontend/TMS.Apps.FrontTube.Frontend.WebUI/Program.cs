using Serilog;
using TMS.Apps.FrontTube.Frontend.WebUI.Installers;

// Configure Serilog
LoggingInstaller.ConfigureSerilog();

try
{
    Log.Information("Starting FrontTube WebUI application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure logging
    builder.AddSerilog();

    // Add services to the container
    builder.Services
        .AddBlazorComponents()
        .AddMudBlazor()
        .AddInvidiousProvider(new Uri("https://youtube.srv1.tms.com"))
        .AddCacheManager(
            host: "localhost",
            port: 5656,
            databaseName: "front-tube",
            username: "root",
            password: "password")
        .AddOrchestrator()
        .AddApiControllers();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // Map all application endpoints
    app.MapApplicationEndpoints();

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
