using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Mappers;
using TMS.Apps.FrontTube.Backend.Core.Tools;
using TMS.Apps.FrontTube.Backend.Repository.Data;
using TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;
using TMS.Apps.FrontTube.Backend.Repository.Data.Enums;

namespace TMS.Apps.FrontTube.Backend.Core.ViewModels;

/// <summary>
/// Top-level ViewModel that manages data repository and other ViewModels.
/// Acts as the central hub for accessing video and channel data.
/// </summary>
public sealed class Super : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Super> _logger;
    private readonly CancellationTokenSource _repositoryManagerCts = new();
    private bool _disposed;

    /// <summary>
    /// Gets the logger factory for creating loggers in child ViewModels.
    /// </summary>
    internal ILoggerFactory LoggerFactory => _loggerFactory;

    /// <summary>
    /// Gets the proxy for video playback, DASH manifests, and image fetching.
    /// </summary>
    public ProxyToProvider Proxy { get; }

    /// <summary>
    /// Gets the application configurations.
    /// UI can modify these to change application behavior.
    /// </summary>
    public Configurations Configurations { get; }

    internal Orchestrator RepositoryManager { get; }

    /// <summary>
    /// Creates a Super instance with default configurations.
    /// All dependencies are created internally.
    /// </summary>
    public Super(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<Super>();

        Configurations = new Configurations();

        RepositoryManager = new Orchestrator(
            Configurations.DataBase,
            Configurations.Cache,
            Configurations.Provider,
            loggerFactory,
            httpClientFactory);

        var providerBaseUri = Configurations.Provider.BaseUri!;

        Action<HttpClientHandler>? proxyHandlerConfigurator = Configurations.Provider.BypassSslValidation
            ? handler => handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            : null;

        Proxy = new ProxyToProvider(
            this,
            httpClientFactory,
            providerBaseUri,
            proxyHandlerConfigurator);

        _logger.LogDebug(
            "[{MethodName}] Initialized with provider base URL '{BaseUrl}'.",
            nameof(Super),
            providerBaseUri);
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        await RepositoryManager.InitAsync(cancellationToken, _repositoryManagerCts.Token);
    }

    /// <summary>
    /// Gets video information by remote identity input (URL, ID, or supported identifier).
    /// </summary>
    /// <param name="remoteIdentity">The video's remote identifier (URL, ID, or supported identifier).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Video ViewModel wrapping the video data, or null if not found.</returns>
    public async Task<Video?> GetVideoAsync(string remoteIdentity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(remoteIdentity))
        {
            _logger.LogWarning(
                "[{MethodName}] Empty remote identity.",
                nameof(GetVideoAsync));
            return null;
        }

        if (!RepositoryManager.TryCreateRemoteIdentity(remoteIdentity, null, out var identity, out var errorMessage))
        {
            _logger.LogWarning(
                "[{MethodName}] Invalid identity '{Identity}': '{ErrorMessage}'.",
                nameof(GetVideoAsync),
                remoteIdentity,
                errorMessage);
            return null;
        }

        if (identity.IdentityType != RemoteIdentityTypeDomain.Video)
        {
            _logger.LogWarning(
                "[{MethodName}] Identity type mismatch for '{Identity}': '{Type}'.",
                nameof(GetVideoAsync),
                remoteIdentity,
                identity.IdentityType);
            return null;
        }

        var videoDomain = await RepositoryManager.GetVideoAsync(identity, cancellationToken);
        if (videoDomain is null)
        {
            return null;
        }

        if (videoDomain.Channel is null)
        {
            _logger.LogWarning(
                "[{MethodName}] Video domain missing channel for URL '{Url}'.",
                nameof(GetVideoAsync),
                videoDomain.RemoteIdentity.AbsoluteRemoteUrl);
            return null;
        }

        return DomainViewModelMapper.ToViewModel(this, videoDomain);
    }

    /// <summary>
    /// Gets channel details by remote identity input (URL, ID, or supported identifier).
    /// </summary>
    /// <param name="remoteIdentity">The channel's remote identifier (URL, ID, or supported identifier).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Channel ViewModel wrapping the channel data, or null if not found.</returns>
    public async Task<Channel?> GetChannelByIdAsync(string remoteIdentity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(remoteIdentity))
        {
            _logger.LogWarning(
                "[{MethodName}] Empty remote identity.",
                nameof(GetChannelByIdAsync));
            return null;
        }

        if (!RepositoryManager.TryCreateRemoteIdentity(remoteIdentity, null, out var identity, out var errorMessage))
        {
            _logger.LogWarning(
                "[{MethodName}] Invalid identity '{Identity}': '{ErrorMessage}'.",
                nameof(GetChannelByIdAsync),
                remoteIdentity,
                errorMessage);
            return null;
        }

        if (identity.IdentityType != RemoteIdentityTypeDomain.Channel)
        {
            _logger.LogWarning(
                "[{MethodName}] Identity type mismatch for '{Identity}': '{Type}'.",
                nameof(GetChannelByIdAsync),
                remoteIdentity,
                identity.IdentityType);
            return null;
        }

        var channelDomain = await RepositoryManager.GetChannelAsync(identity, cancellationToken);
        return channelDomain is null ? null : DomainViewModelMapper.ToViewModel(this, channelDomain);
    }

    /// <summary>
    /// Gets a page of videos from a channel.
    /// </summary>
    /// <param name="remoteIdentity">The channel's remote identifier (URL, ID, or supported identifier).</param>
    /// <param name="tab">The tab to fetch from.</param>
    /// <param name="continuationToken">Pagination token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of video summaries.</returns>
    public async Task<VideosPage?> GetChannelVideosAsync(
        string remoteIdentity,
        Enums.ChannelTab tab,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "[{MethodName}] Getting channel videos for '{Identity}' tab '{Tab}' hasContinuation '{HasContinuation}'.",
            nameof(GetChannelVideosAsync),
            remoteIdentity,
            tab,
            continuationToken is not null);

        if (!RepositoryManager.TryCreateRemoteIdentity(remoteIdentity, null, out var identity, out var errorMessage))
        {
            _logger.LogWarning(
                "[{MethodName}] Invalid identity '{Identity}': '{ErrorMessage}'.",
                nameof(GetChannelVideosAsync),
                remoteIdentity,
                errorMessage);
            return null;
        }

        if (identity.IdentityType != RemoteIdentityTypeDomain.Channel)
        {
            _logger.LogWarning(
                "[{MethodName}] Identity type mismatch for '{Identity}': '{Type}'.",
                nameof(GetChannelVideosAsync),
                remoteIdentity,
                identity.IdentityType);
            return null;
        }

        var pageDomain = await RepositoryManager.GetChannelsPageAsync(
            identity,
            tab.ToDomainChannelTab(),
            continuationToken,
            cancellationToken);

        if (pageDomain is null)
        {
            return null;
        }

        var channelDomain = await RepositoryManager.GetChannelAsync(identity, cancellationToken);
        if (channelDomain is null)
        {
            return null;
        }

        var channelVm = DomainViewModelMapper.ToViewModel(this, channelDomain);
        return DomainViewModelMapper.ToViewModel(this, pageDomain, channelVm);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _repositoryManagerCts.Cancel();
        _repositoryManagerCts.Dispose();

        _disposed = true;
        _logger.LogDebug(
            "[{MethodName}] Disposed.",
            nameof(Dispose));
    }
}
