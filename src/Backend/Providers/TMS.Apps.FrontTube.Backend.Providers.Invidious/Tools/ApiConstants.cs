namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

/// <summary>
/// API path constants for Invidious endpoints.
/// </summary>
internal static class ApiConstants
{
    /// <summary>
    /// Base API path prefix.
    /// </summary>
    internal const string ApiBase = "/api/v1";

    /// <summary>
    /// Query string delimiter symbol.
    /// </summary>
    internal const string ParamQueryDelimiter = "?";

    /// <summary>
    /// Query parameter assignment symbol.
    /// </summary>
    internal const string ParamAssignment = "=";

    /// <summary>
    /// Query parameter separator symbol.
    /// </summary>
    internal const string ParamSeparator = "&";

    /// <summary>
    /// URL path separator symbol.
    /// </summary>
    internal const string PathSeparator = "/";

    /// <summary>
    /// Videos endpoint path segment.
    /// </summary>
    internal const string VideosPath = "/videos";

    /// <summary>
    /// Channels endpoint path segment.
    /// </summary>
    internal const string ChannelsPath = "/channels";

    /// <summary>
    /// Comments endpoint path segment.
    /// </summary>
    internal const string CommentsPath = "/comments";

    /// <summary>
    /// Captions endpoint path segment.
    /// </summary>
    internal const string CaptionsPath = "/captions";

    /// <summary>
    /// Annotations endpoint path segment.
    /// </summary>
    internal const string AnnotationsPath = "/annotations";

    /// <summary>
    /// Trending endpoint path segment.
    /// </summary>
    internal const string TrendingPath = "/trending";

    /// <summary>
    /// Popular endpoint path segment.
    /// </summary>
    internal const string PopularPath = "/popular";

    /// <summary>
    /// Search endpoint path segment.
    /// </summary>
    internal const string SearchPath = "/search";

    /// <summary>
    /// Search suggestions endpoint path segment.
    /// </summary>
    internal const string SearchSuggestionsPath = "/search/suggestions";

    /// <summary>
    /// Playlists endpoint path segment.
    /// </summary>
    internal const string PlaylistsPath = "/playlists";

    /// <summary>
    /// Mixes endpoint path segment.
    /// </summary>
    internal const string MixesPath = "/mixes";

    /// <summary>
    /// Hashtag endpoint path segment.
    /// </summary>
    internal const string HashtagPath = "/hashtag";

    /// <summary>
    /// Resolve URL endpoint path segment.
    /// </summary>
    internal const string ResolveUrlPath = "/resolveurl";

    /// <summary>
    /// Clips endpoint path segment.
    /// </summary>
    internal const string ClipsPath = "/clips";

    /// <summary>
    /// Stats endpoint path segment.
    /// </summary>
    internal const string StatsPath = "/stats";

    /// <summary>
    /// Post endpoint path segment.
    /// </summary>
    internal const string PostPath = "/post";

    /// <summary>
    /// Channel tab: Videos.
    /// </summary>
    internal const string ChannelTabVideos = "videos";

    /// <summary>
    /// Channel tab: Shorts.
    /// </summary>
    internal const string ChannelTabShorts = "shorts";

    /// <summary>
    /// Channel tab: Live streams.
    /// </summary>
    internal const string ChannelTabStreams = "streams";

    /// <summary>
    /// Channel tab: Playlists.
    /// </summary>
    internal const string ChannelTabPlaylists = "playlists";

    /// <summary>
    /// Channel tab: Community posts.
    /// </summary>
    internal const string ChannelTabCommunity = "community";

    /// <summary>
    /// Channel tab: Related channels.
    /// </summary>
    internal const string ChannelTabChannels = "channels";

    /// <summary>
    /// Channel tab: Latest videos.
    /// </summary>
    internal const string ChannelTabLatest = "latest";

    /// <summary>
    /// Channel tab: Podcasts.
    /// </summary>
    internal const string ChannelTabPodcasts = "podcasts";

    /// <summary>
    /// Channel tab: Releases.
    /// </summary>
    internal const string ChannelTabReleases = "releases";

    /// <summary>
    /// Default channel tab when not specified.
    /// </summary>
    internal const string DefaultChannelTab = ChannelTabVideos;

    /// <summary>
    /// Query parameter: Continuation token for pagination.
    /// </summary>
    internal const string ParamContinuation = "continuation";

    /// <summary>
    /// Query parameter: Sort by.
    /// </summary>
    internal const string ParamSortBy = "sort_by";

    /// <summary>
    /// Query parameter: Region (ISO 3166 country code).
    /// </summary>
    internal const string ParamRegion = "region";

    /// <summary>
    /// Query parameter: Language.
    /// </summary>
    internal const string ParamLanguage = "hl";

    /// <summary>
    /// Query parameter: Search query.
    /// </summary>
    internal const string ParamQuery = "q";

    /// <summary>
    /// Query parameter: Page number.
    /// </summary>
    internal const string ParamPage = "page";

    /// <summary>
    /// Query parameter: Source (for comments/annotations).
    /// </summary>
    internal const string ParamSource = "source";

    /// <summary>
    /// Query parameter: Type filter for trending/search.
    /// </summary>
    internal const string ParamType = "type";

    /// <summary>
    /// Query parameter: Date filter for search.
    /// </summary>
    internal const string ParamDate = "date";

    /// <summary>
    /// Query parameter: Duration filter for search.
    /// </summary>
    internal const string ParamDuration = "duration";

    /// <summary>
    /// Query parameter: Features filter for search.
    /// </summary>
    internal const string ParamFeatures = "features";

    /// <summary>
    /// Query parameter: Caption label.
    /// </summary>
    internal const string ParamLabel = "label";

    /// <summary>
    /// Query parameter: Caption language.
    /// </summary>
    internal const string ParamLang = "lang";

    /// <summary>
    /// Query parameter: Translation language.
    /// </summary>
    internal const string ParamTLang = "tlang";

    /// <summary>
    /// Query parameter: URL to resolve.
    /// </summary>
    internal const string ParamUrl = "url";

    /// <summary>
    /// Query parameter: Clip ID.
    /// </summary>
    internal const string ParamId = "id";

    /// <summary>
    /// Query parameter: Channel UCID (for posts).
    /// </summary>
    internal const string ParamUcid = "ucid";

    /// <summary>
    /// Embed player path segment.
    /// </summary>
    internal const string EmbedPath = "/embed";

    /// <summary>
    /// Embed parameter: Autoplay.
    /// </summary>
    internal const string EmbedParamAutoplay = "autoplay";

    /// <summary>
    /// Embed parameter: Local proxy.
    /// </summary>
    internal const string EmbedParamLocal = "local";

    /// <summary>
    /// Default region (US).
    /// </summary>
    internal const string DefaultRegion = "US";

    /// <summary>
    /// Comment source: YouTube.
    /// </summary>
    internal const string CommentSourceYouTube = "youtube";

    /// <summary>
    /// Comment source: Reddit.
    /// </summary>
    internal const string CommentSourceReddit = "reddit";

    /// <summary>
    /// Comment sort: Top.
    /// </summary>
    internal const string CommentSortTop = "top";

    /// <summary>
    /// Comment sort: New.
    /// </summary>
    internal const string CommentSortNew = "new";

    /// <summary>
    /// Trending type: Default.
    /// </summary>
    internal const string TrendingTypeDefault = "default";

    /// <summary>
    /// Trending type: Music.
    /// </summary>
    internal const string TrendingTypeMusic = "music";

    /// <summary>
    /// Trending type: Gaming.
    /// </summary>
    internal const string TrendingTypeGaming = "gaming";

    /// <summary>
    /// Trending type: News.
    /// </summary>
    internal const string TrendingTypeNews = "news";

    /// <summary>
    /// Trending type: Movies.
    /// </summary>
    internal const string TrendingTypeMovies = "movies";

    /// <summary>
    /// Search sort: Relevance.
    /// </summary>
    internal const string SearchSortRelevance = "relevance";

    /// <summary>
    /// Search sort: Rating.
    /// </summary>
    internal const string SearchSortRating = "rating";

    /// <summary>
    /// Search sort: Date.
    /// </summary>
    internal const string SearchSortDate = "date";

    /// <summary>
    /// Search sort: Views.
    /// </summary>
    internal const string SearchSortViews = "views";

    /// <summary>
    /// Video sort: Newest.
    /// </summary>
    internal const string VideoSortNewest = "newest";

    /// <summary>
    /// Video sort: Popular.
    /// </summary>
    internal const string VideoSortPopular = "popular";

    /// <summary>
    /// Video sort: Oldest.
    /// </summary>
    internal const string VideoSortOldest = "oldest";

    /// <summary>
    /// Playlist sort: Last updated.
    /// </summary>
    internal const string PlaylistSortLast = "last";
}
