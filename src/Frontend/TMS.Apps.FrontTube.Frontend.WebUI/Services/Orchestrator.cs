using TMS.Apps.FrontTube.Backend.Core.ViewModels;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Services;

/// <summary>
/// Orchestrator service that creates and manages the Super ViewModel instance.
/// This service is scoped to ensure each user session has its own instance.
/// </summary>
public sealed class Orchestrator : IDisposable
{
    private readonly ILogger<Orchestrator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Lazy<Super> _super;
    private bool _disposed;

    /// <summary>
    /// Event raised when a channel should be opened in the dock panel.
    /// </summary>
    public event EventHandler<string>? ChannelOpenRequested;

    /// <summary>
    /// Gets or sets the currently active channel ID.
    /// </summary>
    public string? ActiveChannelId { get; private set; }

    public Orchestrator(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<Orchestrator>();

        // Lazy initialization of Super to defer creation until first access
        _super = new Lazy<Super>(() =>
        {
            _logger.LogDebug("Creating Super instance");
            return new Super(loggerFactory, httpClientFactory);
        });

        _logger.LogDebug("Orchestrator initialized");
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing Super instance");
        await Super.InitAsync(cancellationToken);
        _logger.LogDebug("Super instance initialized");
    }

    /// <summary>
    /// Gets the Super ViewModel instance.
    /// </summary>
    public Super Super => _super.Value;

    internal ILoggerFactory LoggerFactory => _loggerFactory;

    /// <summary>
    /// Gets the video provider base URL.
    /// </summary>
    public string ProviderBaseUrl => Super.Proxy.ProviderBaseUrl.ToString().TrimEnd('/');

    /// <summary>
    /// Requests to open a channel in the dock panel.
    /// </summary>
    /// <param name="channelId">The channel ID to open.</param>
    public void OpenChannel(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            return;
        }

        _logger.LogDebug("[{MethodName}] Opening channel '{ChannelId}'.", nameof(OpenChannel), channelId);
        ActiveChannelId = channelId;
        ChannelOpenRequested?.Invoke(this, channelId);
    }

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
