using System.Text;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

/// <summary>
/// Builds URLs for Invidious API endpoints.
/// </summary>
internal static class UrlBuilder
{
    /// <summary>
    /// Builds the URL for fetching video details.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="videoId">The video ID.</param>
    /// <param name="region">Optional region code (ISO 3166).</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildVideoUrl(Uri baseUrl, string videoId, string? region = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.VideosPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(videoId));

        if (!string.IsNullOrWhiteSpace(region))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamRegion);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(region));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching channel details.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildChannelUrl(Uri baseUrl, string channelId)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.ChannelsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(channelId));

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching channel videos/shorts/streams/playlists tab.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="tab">The tab name (videos, shorts, streams, playlists, community, channels).</param>
    /// <param name="continuationToken">Optional continuation token for pagination.</param>
    /// <param name="page">Optional page number (1-based) for pagination.</param>
    /// <param name="sortBy">Optional sort order.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildChannelTabUrl(
        Uri baseUrl,
        string channelId,
        string tab,
        string? continuationToken = null,
        int? page = null,
        string? sortBy = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.ChannelsPath);
        builder.Append(ApiConstants.PathSeparator);
        builder.Append(Uri.EscapeDataString(channelId));
        builder.Append(ApiConstants.PathSeparator);
        builder.Append(Uri.EscapeDataString(tab));

        var hasParams = false;

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamContinuation);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(continuationToken));
            hasParams = true;
        }

        if (page.HasValue)
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamPage);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(page.Value);
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamSortBy);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(sortBy));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching channel search results.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="query">The search query.</param>
    /// <param name="page">Optional page number.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildChannelSearchUrl(Uri baseUrl, string channelId, string query, int? page = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.ChannelsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(channelId));
        builder.Append(ApiConstants.SearchPath);
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.ParamQuery);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(Uri.EscapeDataString(query));

        if (page.HasValue)
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamPage);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(page.Value);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching video comments.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="videoId">The video ID.</param>
    /// <param name="sortBy">Optional sort order (top, new).</param>
    /// <param name="source">Optional source (youtube, reddit).</param>
    /// <param name="continuationToken">Optional continuation token for pagination.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildCommentsUrl(
        Uri baseUrl,
        string videoId,
        string? sortBy = null,
        string? source = null,
        string? continuationToken = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.CommentsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(videoId));

        var hasParams = false;

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamSortBy);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(sortBy));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamSource);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(source));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamContinuation);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(continuationToken));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching video captions.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="videoId">The video ID.</param>
    /// <param name="label">Optional caption label to get specific caption in WebVTT format.</param>
    /// <param name="lang">Optional language code.</param>
    /// <param name="tlang">Optional translation language.</param>
    /// <param name="region">Optional region code.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildCaptionsUrl(
        Uri baseUrl,
        string videoId,
        string? label = null,
        string? lang = null,
        string? tlang = null,
        string? region = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.CaptionsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(videoId));

        var hasParams = false;

        if (!string.IsNullOrWhiteSpace(label))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamLabel);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(label));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(lang))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamLang);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(lang));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(tlang))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamTLang);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(tlang));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamRegion);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(region));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching trending videos.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="type">Optional type (music, gaming, movies, default).</param>
    /// <param name="region">Optional region code (ISO 3166).</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildTrendingUrl(Uri baseUrl, string? type = null, string? region = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.TrendingPath);

        var hasParams = false;

        if (!string.IsNullOrWhiteSpace(type))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamType);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(type));
            hasParams = true;
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            builder.Append(hasParams ? ApiConstants.ParamSeparator : ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamRegion);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(region));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching popular videos.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildPopularUrl(Uri baseUrl)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.PopularPath);

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for search.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="query">The search query.</param>
    /// <param name="page">Optional page number.</param>
    /// <param name="sortBy">Optional sort order (relevance, rating, date, views).</param>
    /// <param name="date">Optional date filter (hour, today, week, month, year).</param>
    /// <param name="duration">Optional duration filter (short, long, medium).</param>
    /// <param name="type">Optional type filter (video, playlist, channel, movie, show, all).</param>
    /// <param name="features">Optional features filter (comma-separated: hd, subtitles, etc.).</param>
    /// <param name="region">Optional region code (ISO 3166).</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildSearchUrl(
        Uri baseUrl,
        string query,
        int? page = null,
        string? sortBy = null,
        string? date = null,
        string? duration = null,
        string? type = null,
        string? features = null,
        string? region = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.SearchPath);
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.ParamQuery);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(Uri.EscapeDataString(query));

        if (page.HasValue)
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamPage);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(page.Value);
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamSortBy);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(sortBy));
        }

        if (!string.IsNullOrWhiteSpace(date))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamDate);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(date));
        }

        if (!string.IsNullOrWhiteSpace(duration))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamDuration);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(duration));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamType);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(type));
        }

        if (!string.IsNullOrWhiteSpace(features))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamFeatures);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(features));
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            builder.Append(ApiConstants.ParamSeparator);
            builder.Append(ApiConstants.ParamRegion);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(region));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for search suggestions.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="query">The search query.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildSearchSuggestionsUrl(Uri baseUrl, string query)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.SearchSuggestionsPath);
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.ParamQuery);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(Uri.EscapeDataString(query));

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching a playlist.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="playlistId">The playlist ID.</param>
    /// <param name="page">Optional page number.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildPlaylistUrl(Uri baseUrl, string playlistId, int? page = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.PlaylistsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(playlistId));

        if (page.HasValue)
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamPage);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(page.Value);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching a mix.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="mixId">The mix ID.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildMixUrl(Uri baseUrl, string mixId)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.MixesPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(mixId));

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching hashtag videos.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="tag">The hashtag (without #).</param>
    /// <param name="page">Optional page number.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildHashtagUrl(Uri baseUrl, string tag, int? page = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.HashtagPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(tag));

        if (page.HasValue)
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamPage);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(page.Value);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for resolving a YouTube URL.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="url">The URL to resolve.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildResolveUrlUrl(Uri baseUrl, string url)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.ResolveUrlPath);
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.ParamUrl);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(Uri.EscapeDataString(url));

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching clip details.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="clipId">The clip ID.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildClipsUrl(Uri baseUrl, string clipId)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.ClipsPath);
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.ParamId);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(Uri.EscapeDataString(clipId));

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching instance stats.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildStatsUrl(Uri baseUrl)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.StatsPath);

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching a community post.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="postId">The post ID.</param>
    /// <param name="channelId">Optional channel ID.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildPostUrl(Uri baseUrl, string postId, string? channelId = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.PostPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(postId));

        if (!string.IsNullOrWhiteSpace(channelId))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamUcid);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(channelId));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching post comments.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="postId">The post ID.</param>
    /// <param name="channelId">Optional channel ID.</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildPostCommentsUrl(Uri baseUrl, string postId, string? channelId = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.PostPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(postId));
        builder.Append(ApiConstants.CommentsPath);

        if (!string.IsNullOrWhiteSpace(channelId))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamUcid);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(channelId));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the embed player URL for a video.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="videoId">The video ID.</param>
    /// <param name="autoplay">Whether to autoplay the video.</param>
    /// <param name="local">Whether to use local proxy.</param>
    /// <returns>The embed player URL.</returns>
    internal static string BuildEmbedUrl(Uri baseUrl, string videoId, bool autoplay = true, bool local = true)
    {
        var builder = new StringBuilder();
        builder.Append(baseUrl.ToString().TrimEnd('/'));
        builder.Append(ApiConstants.EmbedPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(videoId));
        builder.Append(ApiConstants.ParamQueryDelimiter);
        builder.Append(ApiConstants.EmbedParamAutoplay);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(autoplay ? "1" : "0");
        builder.Append(ApiConstants.ParamSeparator);
        builder.Append(ApiConstants.EmbedParamLocal);
        builder.Append(ApiConstants.ParamAssignment);
        builder.Append(local ? "true" : "false");

        return builder.ToString();
    }

    /// <summary>
    /// Builds the URL for fetching video annotations.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Invidious instance.</param>
    /// <param name="videoId">The video ID.</param>
    /// <param name="source">Optional source (archive, youtube).</param>
    /// <returns>The complete API URL.</returns>
    internal static string BuildAnnotationsUrl(Uri baseUrl, string videoId, string? source = null)
    {
        var builder = new StringBuilder();
        builder.Append(GetBaseApiUrl(baseUrl));
        builder.Append(ApiConstants.AnnotationsPath);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(videoId));

        if (!string.IsNullOrWhiteSpace(source))
        {
            builder.Append(ApiConstants.ParamQueryDelimiter);
            builder.Append(ApiConstants.ParamSource);
            builder.Append(ApiConstants.ParamAssignment);
            builder.Append(Uri.EscapeDataString(source));
        }

        return builder.ToString();
    }

    private static string GetBaseApiUrl(Uri baseUrl)
    {
        return $"{baseUrl.ToString().TrimEnd('/')}{ApiConstants.ApiBase}";
    }
}
