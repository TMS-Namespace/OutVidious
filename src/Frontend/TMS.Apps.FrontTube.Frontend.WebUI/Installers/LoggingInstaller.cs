using Serilog;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Installers;

/// <summary>
/// Configures Serilog logging for the application.
/// </summary>
internal static class LoggingInstaller
{
    /// <summary>
    /// Configures Serilog as the logging provider.
    /// </summary>
    internal static void ConfigureSerilog()
    {
        // Find solution root for log file location
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
    }

    /// <summary>
    /// Adds Serilog to the application builder.
    /// </summary>
    internal static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        return builder;
    }
}
