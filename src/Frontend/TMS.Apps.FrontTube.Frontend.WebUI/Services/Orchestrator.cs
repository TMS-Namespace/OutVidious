using TMS.Apps.FrontTube.Backend.Repository.CacheManager.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Services;

/// <summary>
/// Orchestrator service that creates and manages the Super ViewModel instance.
/// This service is scoped to ensure each user session has its own instance.
/// </summary>
public sealed class Orchestrator : IDisposable
{
    private readonly ILogger<Orchestrator> _logger;
    private readonly Lazy<Super> _super;
    private bool _disposed;

    public Orchestrator(
        ILoggerFactory loggerFactory,
        IVideoProvider videoProvider,
        ICacheManager dataRepository)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(videoProvider);
        ArgumentNullException.ThrowIfNull(dataRepository);

        _logger = loggerFactory.CreateLogger<Orchestrator>();

        // Lazy initialization of Super to defer creation until first access
        _super = new Lazy<Super>(() =>
        {
            _logger.LogDebug("Creating Super instance");
            return new Super(loggerFactory, videoProvider, dataRepository);
        });

        _logger.LogDebug("Orchestrator initialized");
    }

    /// <summary>
    /// Gets the Super ViewModel instance.
    /// </summary>
    public Super Super => _super.Value;

    /// <summary>
    /// Gets the video provider base URL.
    /// </summary>
    public string ProviderBaseUrl => Super.VideoProvider.BaseUrl.ToString().TrimEnd('/');

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_super.IsValueCreated)
        {
            _super.Value.Dispose();
        }

        _disposed = true;
        _logger.LogDebug("Orchestrator disposed");
    }
}
