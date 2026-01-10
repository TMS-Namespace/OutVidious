using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Models;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious;

/// <summary>
/// Video provider implementation for Invidious instances.
/// </summary>
public sealed class InvidiousVideoProvider : ProviderBase
{
    private readonly JsonWebClient _webClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public InvidiousVideoProvider(
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        ProviderConfig config)
        : base(InvidiousHelpers.CreateHttpClient(config), loggerFactory.CreateLogger<InvidiousVideoProvider>(), loggerFactory, config.BaseUri ?? throw new ArgumentNullException(nameof(config), "BaseUri cannot be null."))
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(config);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new FlexibleStringConverter() }
        };

        _webClient = new JsonWebClient(HttpClient, _jsonOptions, loggerFactory);
    }

    /// <inheritdoc />
    public override string ProviderId => "invidious";

    /// <inheritdoc />
    public override string DisplayName => "Invidious";

    /// <inheritdoc />
    public override string Description => "Privacy-focused YouTube frontend providing access to YouTube videos without tracking.";

    /// <inheritdoc />
    public override async Task<JsonWebResponse<VideoCommon?>> GetVideoAsync(
        RemoteIdentityCommon videoIdentity,
        CancellationToken cancellationToken)
    {
        var videoId = InvidiousHelpers.GetRemoteIdOrThrow(videoIdentity, RemoteIdentityTypeCommon.Video);
        var apiUrl = UrlBuilder.BuildVideoUrl(BaseUrl, videoId);

        Logger.LogDebug("[{Method}] Fetching video details from Invidious: '{ApiUrl}'.", nameof(GetVideoAsync), apiUrl);

        var response = await _webClient.GetAsync<VideoDetails>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch video '{VideoId}' from Invidious. Response: {Response}",
                nameof(GetVideoAsync),
                videoId,
                response);

            return response.MapOrNull<VideoCommon>(_ => null);
        }

        var videoInfo = VideoMapper.ToVideoInfo(response.Data!, BaseUrl);
        Logger.LogDebug("[{Method}] Successfully fetched and mapped video details for: '{VideoId}'.", nameof(GetVideoAsync), videoId);

        return response.MapOrNull<VideoCommon>(_ => videoInfo);
    }

    /// <inheritdoc />
    public override Uri GetEmbedVideoPlayerUri(RemoteIdentityCommon videoIdentity)
    {
        var videoId = InvidiousHelpers.GetRemoteIdOrThrow(videoIdentity, RemoteIdentityTypeCommon.Video);
        var embedUrl = UrlBuilder.BuildEmbedUrl(BaseUrl, videoId, autoplay: true, local: true);

        return new Uri(embedUrl);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<ChannelCommon?>> GetChannelAsync(
        RemoteIdentityCommon channelIdentity,
        CancellationToken cancellationToken)
    {
        var channelId = InvidiousHelpers.GetRemoteIdOrThrow(channelIdentity, RemoteIdentityTypeCommon.Channel);
        var apiUrl = UrlBuilder.BuildChannelUrl(BaseUrl, channelId);

        Logger.LogDebug("[{Method}] Fetching channel details from Invidious: '{ApiUrl}'.", nameof(GetChannelAsync), apiUrl);

        var response = await _webClient.GetAsync<Channel>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch channel '{ChannelId}' from Invidious. Response: {Response}",
                nameof(GetChannelAsync),
                channelId,
                response);

            return response.MapOrNull<ChannelCommon>(_ => null);
        }

        var channelDetails = ChannelMapper.ToChannelDetails(response.Data!, BaseUrl);
        Logger.LogDebug("[{Method}] Successfully fetched and mapped channel details for: '{ChannelId}'.", nameof(GetChannelAsync), channelId);

        return response.MapOrNull<ChannelCommon>(_ => channelDetails);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<VideosPageCommon?>> GetChannelVideosTabAsync(
        RemoteIdentityCommon channelIdentity,
        ChannelTabType tab,
        int? page,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        var channelId = InvidiousHelpers.GetRemoteIdOrThrow(channelIdentity, RemoteIdentityTypeCommon.Channel);
        var tabString = tab.ToApiString();
        var apiUrl = UrlBuilder.BuildChannelTabUrl(BaseUrl, channelId, tabString, continuationToken, page);

        Logger.LogDebug("[{Method}] Fetching channel videos from Invidious: '{ApiUrl}'.", nameof(GetChannelVideosTabAsync), apiUrl);

        var response = await _webClient.GetAsync<ChannelVideosResponse>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch channel videos for '{ChannelId}' from Invidious. Response: {Response}",
                nameof(GetChannelVideosTabAsync),
                channelId,
                response);

            return response.MapOrNull<VideosPageCommon>(_ => new VideosPageCommon
            {
                ChannelRemoteIdentity = channelIdentity,
                Tab = tab,
                Videos = [],
                ContinuationToken = null,
                TotalVideoCount = null
            });
        }

        if (response.Data?.Videos == null)
        {
            Logger.LogWarning(
                "[{Method}] Invidious API returned null videos for channel '{ChannelId}'.",
                nameof(GetChannelVideosTabAsync),
                channelId);

            return response.MapOrNull<VideosPageCommon>(_ => new VideosPageCommon
            {
                ChannelRemoteIdentity = channelIdentity,
                Tab = tab,
                Videos = [],
                ContinuationToken = null,
                TotalVideoCount = null
            });
        }

        var videos = new List<VideoMetadataCommon>();
        foreach (var videoDto in response.Data.Videos)
        {
            if (string.IsNullOrWhiteSpace(videoDto.VideoId))
            {
                Logger.LogDebug(
                    "[{Method}] Skipping video with empty ID from channel '{ChannelId}' tab '{Tab}'. Title: '{Title}'.",
                    nameof(GetChannelVideosTabAsync),
                    channelId,
                    tab,
                    videoDto.Title ?? "(no title)");
                continue;
            }

            try
            {
                var video = ChannelMapper.ToVideoSummary(videoDto, BaseUrl);
                videos.Add(video);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    ex,
                    "[{Method}] Failed to map video '{VideoId}' from channel '{ChannelId}' tab '{Tab}': {Message}.",
                    nameof(GetChannelVideosTabAsync),
                    videoDto.VideoId,
                    channelId,
                    tab,
                    ex.Message);
            }
        }

        var videosPage = new VideosPageCommon
        {
            ChannelRemoteIdentity = channelIdentity,
            Tab = tab,
            Videos = videos,
            ContinuationToken = response.Data.Continuation,
            TotalVideoCount = null // Invidious doesn't provide total count in paginated responses
        };

        Logger.LogDebug(
            "[{Method}] Successfully fetched '{VideoCount}' videos for channel '{ChannelId}', HasMore: '{HasMore}'.",
            nameof(GetChannelVideosTabAsync),
            videos.Count,
            channelId,
            videosPage.HasMore);

        return response.MapOrNull<VideosPageCommon>(_ => videosPage);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<CommentsPageCommon?>> GetCommentsAsync(
        RemoteIdentityCommon videoIdentity,
        CommentSortType? sortBy,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        var videoId = InvidiousHelpers.GetRemoteIdOrThrow(videoIdentity, RemoteIdentityTypeCommon.Video);
        var sortByString = sortBy?.ToApiString();
        var apiUrl = UrlBuilder.BuildCommentsUrl(BaseUrl, videoId, sortByString, source: null, continuationToken);

        Logger.LogDebug("[{Method}] Fetching comments from Invidious: '{ApiUrl}'.", nameof(GetCommentsAsync), apiUrl);

        var response = await _webClient.GetAsync<CommentsResponse>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch comments for video '{VideoId}' from Invidious. Response: {Response}",
                nameof(GetCommentsAsync),
                videoId,
                response);

            return response.MapOrNull<CommentsPageCommon>(_ => new CommentsPageCommon
            {
                VideoId = videoId,
                VideoIdentity = new RemoteIdentityCommon(
                    RemoteIdentityTypeCommon.Video,
                    InvidiousHelpers.ResolveVideoUrl(videoId)),
                Comments = [],
                ContinuationToken = null
            });
        }

        var commentsPage = CommentsMapper.ToCommentsPage(response.Data!, videoId);
        Logger.LogDebug(
            "[{Method}] Successfully fetched '{CommentCount}' comments for video '{VideoId}'.",
            nameof(GetCommentsAsync),
            commentsPage.Comments.Count,
            videoId);

        return response.MapOrNull<CommentsPageCommon>(_ => commentsPage);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<SearchResultsCommon?>> SearchAsync(
        string query,
        int page,
        SearchSortType? sortBy,
        SearchType? type,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var sortByString = sortBy?.ToApiString();
        var typeString = type?.ToApiString();
        var apiUrl = UrlBuilder.BuildSearchUrl(BaseUrl, query, page, sortByString, type: typeString);

        Logger.LogDebug("[{Method}] Searching Invidious: '{ApiUrl}'.", nameof(SearchAsync), apiUrl);

        var response = await _webClient.GetAsync<List<object>>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to search for '{Query}' on Invidious. Response: {Response}",
                nameof(SearchAsync),
                query,
                response);

            return response.MapOrNull<SearchResultsCommon>(_ => null);
        }

        var items = InvidiousHelpers.ParseSearchResults(response.Data ?? [], BaseUrl, _jsonOptions, msg => Logger.LogWarning(msg));

        var searchResults = new SearchResultsCommon
        {
            Query = query,
            Items = items
        };

        Logger.LogDebug(
            "[{Method}] Successfully fetched '{ResultCount}' search results for query '{Query}'.",
            nameof(SearchAsync),
            items.Count,
            query);

        return response.MapOrNull<SearchResultsCommon>(_ => searchResults);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<SearchSuggestionsCommon?>> GetSearchSuggestionsAsync(
        string query,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var apiUrl = UrlBuilder.BuildSearchSuggestionsUrl(BaseUrl, query);

        Logger.LogDebug("[{Method}] Fetching search suggestions from Invidious: '{ApiUrl}'.", nameof(GetSearchSuggestionsAsync), apiUrl);

        var response = await _webClient.GetAsync<SearchSuggestions>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch search suggestions for '{Query}' from Invidious. Response: {Response}",
                nameof(GetSearchSuggestionsAsync),
                query,
                response);

            return response.MapOrNull<SearchSuggestionsCommon>(_ => null);
        }

        var suggestions = SearchMapper.ToSearchSuggestions(response.Data!);
        Logger.LogDebug(
            "[{Method}] Successfully fetched '{SuggestionCount}' suggestions for query '{Query}'.",
            nameof(GetSearchSuggestionsAsync),
            suggestions.Suggestions.Count,
            query);

        return response.MapOrNull<SearchSuggestionsCommon>(_ => suggestions);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<TrendingVideosCommon?>> GetTrendingAsync(
        TrendingCategory category,
        RegionCode? region,
        CancellationToken cancellationToken)
    {
        var type = category switch
        {
            TrendingCategory.Music => ApiConstants.TrendingTypeMusic,
            TrendingCategory.Gaming => ApiConstants.TrendingTypeGaming,
            TrendingCategory.News => ApiConstants.TrendingTypeNews,
            TrendingCategory.Movies => ApiConstants.TrendingTypeMovies,
            _ => null
        };

        var regionString = region?.ToApiString();
        var apiUrl = UrlBuilder.BuildTrendingUrl(BaseUrl, type, regionString);

        Logger.LogDebug("[{Method}] Fetching trending videos from Invidious: '{ApiUrl}'.", nameof(GetTrendingAsync), apiUrl);

        var response = await _webClient.GetAsync<List<TrendingVideo>>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch trending videos from Invidious. Response: {Response}",
                nameof(GetTrendingAsync),
                response);

            return response.MapOrNull<TrendingVideosCommon>(_ => null);
        }

        var trendingVideos = TrendingMapper.ToTrendingVideos(response.Data ?? [], category, regionString, BaseUrl);
        Logger.LogDebug(
            "[{Method}] Successfully fetched '{VideoCount}' trending videos.",
            nameof(GetTrendingAsync),
            trendingVideos.Videos.Count);

        return response.MapOrNull<TrendingVideosCommon>(_ => trendingVideos);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<TrendingVideosCommon?>> GetPopularAsync(CancellationToken cancellationToken)
    {
        var apiUrl = UrlBuilder.BuildPopularUrl(BaseUrl);

        Logger.LogDebug("[{Method}] Fetching popular videos from Invidious: '{ApiUrl}'.", nameof(GetPopularAsync), apiUrl);

        var response = await _webClient.GetAsync<List<PopularVideo>>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch popular videos from Invidious. Response: {Response}",
                nameof(GetPopularAsync),
                response);

            return response.MapOrNull<TrendingVideosCommon>(_ => null);
        }

        var popularVideos = TrendingMapper.ToPopularVideos(response.Data ?? [], BaseUrl);
        Logger.LogDebug(
            "[{Method}] Successfully fetched '{VideoCount}' popular videos.",
            nameof(GetPopularAsync),
            popularVideos.Videos.Count);

        return response.MapOrNull<TrendingVideosCommon>(_ => popularVideos);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<PlaylistCommon?>> GetPlaylistAsync(
        RemoteIdentityCommon playlistIdentity,
        int page,
        CancellationToken cancellationToken)
    {
        var playlistId = InvidiousHelpers.GetRemoteIdOrThrow(playlistIdentity, RemoteIdentityTypeCommon.Playlist);

        var apiUrl = UrlBuilder.BuildPlaylistUrl(BaseUrl, playlistId, page);

        Logger.LogDebug("[{Method}] Fetching playlist from Invidious: '{ApiUrl}'.", nameof(GetPlaylistAsync), apiUrl);

        var response = await _webClient.GetAsync<Playlist>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch playlist '{PlaylistId}' from Invidious. Response: {Response}",
                nameof(GetPlaylistAsync),
                playlistId,
                response);

            return response.MapOrNull<PlaylistCommon>(_ => null);
        }

        var playlist = PlaylistMapper.ToPlaylist(response.Data!, BaseUrl);
        Logger.LogDebug(
            "[{Method}] Successfully fetched playlist '{PlaylistId}' with '{VideoCount}' videos.",
            nameof(GetPlaylistAsync),
            playlistId,
            playlist.Videos.Count);

        return response.MapOrNull<PlaylistCommon>(_ => playlist);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<PlaylistCommon?>> GetMixAsync(
        RemoteIdentityCommon mixIdentity,
        CancellationToken cancellationToken)
    {
        var mixId = InvidiousHelpers.GetRemoteIdOrThrow(mixIdentity, RemoteIdentityTypeCommon.Mix);

        var apiUrl = UrlBuilder.BuildMixUrl(BaseUrl, mixId);

        Logger.LogDebug("[{Method}] Fetching mix from Invidious: '{ApiUrl}'.", nameof(GetMixAsync), apiUrl);

        var response = await _webClient.GetAsync<Mix>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch mix '{MixId}' from Invidious. Response: {Response}",
                nameof(GetMixAsync),
                mixId,
                response);

            return response.MapOrNull<PlaylistCommon>(_ => null);
        }

        var playlist = PlaylistMapper.ToPlaylistFromMix(response.Data!, BaseUrl);
        Logger.LogDebug(
            "[{Method}] Successfully fetched mix '{MixId}' with '{VideoCount}' videos.",
            nameof(GetMixAsync),
            mixId,
            playlist.Videos.Count);

        return response.MapOrNull<PlaylistCommon>(_ => playlist);
    }

    /// <inheritdoc />
    public override async Task<JsonWebResponse<InstanceStatsCommon?>> GetInstanceStatsAsync(CancellationToken cancellationToken)
    {
        var apiUrl = UrlBuilder.BuildStatsUrl(BaseUrl);

        Logger.LogDebug("[{Method}] Fetching instance stats from Invidious: '{ApiUrl}'.", nameof(GetInstanceStatsAsync), apiUrl);

        var response = await _webClient.GetAsync<InstanceStats>(apiUrl, cancellationToken);

        if (response.HasError)
        {
            Logger.LogWarning(
                "[{Method}] Failed to fetch instance stats from Invidious. Response: {Response}",
                nameof(GetInstanceStatsAsync),
                response);

            return response.MapOrNull<InstanceStatsCommon>(_ => null);
        }

        var stats = InstanceStatsMapper.ToInstanceStats(response.Data!);
        Logger.LogDebug(
            "[{Method}] Successfully fetched instance stats. Version: '{Version}'.",
            nameof(GetInstanceStatsAsync),
            stats.Version);

        return response.MapOrNull<InstanceStatsCommon>(_ => stats);
    }
}
