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
        .AddHttpClient()
        .AddBlazorComponents()
        .AddMudBlazor()
        .AddServices();

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
    app.AddEndpoints();

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
