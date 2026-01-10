using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore;

/// <summary>
/// Base class for video providers with common functionality.
/// </summary>
public abstract class ProviderBase : IProvider
{
    private bool _disposed;

    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;

    protected ProviderBase(HttpClient httpClient, ILogger logger, ILoggerFactory loggerFactory, Uri baseUrl)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    /// <inheritdoc />
    public abstract string ProviderId { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public Uri BaseUrl { get; }

    /// <inheritdoc />
    public virtual bool IsConfigured => BaseUrl.IsAbsoluteUri;

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<VideoCommon?>> GetVideoAsync(RemoteIdentityCommon videoIdentity, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Uri GetEmbedVideoPlayerUri(RemoteIdentityCommon videoIdentity);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<ChannelCommon?>> GetChannelAsync(RemoteIdentityCommon channelIdentity, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<VideosPageCommon?>> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        ChannelTabType tab,
        int? page,
        string? continuationToken,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<CommentsPageCommon?>> GetCommentsAsync(
        RemoteIdentityCommon videoIdentity,
        CommentSortType? sortBy,
        string? continuationToken,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<SearchResultsCommon?>> SearchAsync(
        string query,
        int page,
        SearchSortType? sortBy,
        SearchType? type,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<SearchSuggestionsCommon?>> GetSearchSuggestionsAsync(
        string query,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<TrendingVideosCommon?>> GetTrendingAsync(
        TrendingCategory category,
        RegionCode? region,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<TrendingVideosCommon?>> GetPopularAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<PlaylistCommon?>> GetPlaylistAsync(
        RemoteIdentityCommon playlistIdentity,
        int page,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<PlaylistCommon?>> GetMixAsync(
        RemoteIdentityCommon mixIdentity,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<JsonWebResponse<InstanceStatsCommon?>> GetInstanceStatsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a URI by combining the base URL with a relative path.
    /// </summary>
    /// <param name="relativePath">The relative path to append.</param>
    /// <returns>The combined URI.</returns>
    protected Uri CreateUri(string relativePath)
    {
        var baseUrlString = BaseUrl.ToString().TrimEnd('/');
        var path = relativePath.TrimStart('/');
        return new Uri($"{baseUrlString}/{path}");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Note: We don't dispose HttpClient here as it's typically injected
            // and managed by the DI container
        }

        _disposed = true;
    }
}
