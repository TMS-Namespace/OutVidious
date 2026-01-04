using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Contracts;

/// <summary>
/// Represents the parsed components of a YouTube URL or identity string.
/// </summary>
public sealed record YouTubeIdentityParts
{
    /// <summary>
    /// The original input string that was parsed.
    /// </summary>
    public required string OriginalInput { get; init; }

    /// <summary>
    /// The classified type of the YouTube identity.
    /// </summary>
    public required YouTubeIdentityType IdentityType { get; init; }

    /// <summary>
    /// Whether the parsing was successful (no critical errors).
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// The protocol used (http, https, or null if not present).
    /// </summary>
    public string? Protocol { get; init; }

    /// <summary>
    /// The subdomain if present (www, m, music, etc.).
    /// </summary>
    public string? Subdomain { get; init; }

    /// <summary>
    /// The recognized YouTube domain type.
    /// </summary>
    public YouTubeDomain Domain { get; init; }

    /// <summary>
    /// The full host string (e.g., "www.youtube.com").
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// The extracted video ID (11 characters) if applicable.
    /// </summary>
    public string? VideoId { get; init; }

    /// <summary>
    /// The extracted channel ID (UC... format) if applicable.
    /// </summary>
    public string? ChannelId { get; init; }

    /// <summary>
    /// The extracted channel handle (@...) if applicable.
    /// </summary>
    public string? ChannelHandle { get; init; }

    /// <summary>
    /// The extracted channel custom name (from /c/...) if applicable.
    /// </summary>
    public string? ChannelCustomName { get; init; }

    /// <summary>
    /// The extracted username (from /user/...) if applicable.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// The extracted playlist ID if applicable.
    /// </summary>
    public string? PlaylistId { get; init; }

    /// <summary>
    /// The channel tab if the URL points to a specific tab.
    /// </summary>
    public YouTubeChannelTab ChannelTab { get; init; }

    /// <summary>
    /// The playlist index if present in URL.
    /// </summary>
    public int? PlaylistIndex { get; init; }

    /// <summary>
    /// The timestamp/start time in seconds if present.
    /// </summary>
    public int? StartTimeSeconds { get; init; }

    /// <summary>
    /// The canonical absolute remote URL constructed from the parsed parts.
    /// Returns null if unable to construct a valid URL.
    /// </summary>
    public Uri? AbsoluteRemoteUrl { get; init; }

    /// <summary>
    /// The canonical absolute remote URL for the associated channel (if video or channel URL).
    /// </summary>
    public Uri? ChannelAbsoluteRemoteUrl { get; init; }

    /// <summary>
    /// List of validation warnings (non-critical issues found during parsing).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>
    /// List of validation errors (critical issues that caused parsing to fail or be incomplete).
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// Additional query parameters extracted from the URL.
    /// </summary>
    public IReadOnlyDictionary<string, string> QueryParameters { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// The URL fragment (portion after #) if present.
    /// </summary>
    public string? Fragment { get; init; }

    /// <summary>
    /// Gets a value indicating whether this identity represents a video.
    /// </summary>
    public bool IsVideo => IdentityType is
        YouTubeIdentityType.VideoId or
        YouTubeIdentityType.VideoWatch or
        YouTubeIdentityType.VideoShortUrl or
        YouTubeIdentityType.VideoEmbed or
        YouTubeIdentityType.VideoLegacyEmbed or
        YouTubeIdentityType.VideoShorts or
        YouTubeIdentityType.VideoLive or
        YouTubeIdentityType.VideoInPlaylist or
        YouTubeIdentityType.VideoOEmbed or
        YouTubeIdentityType.VideoAttributionLink or
        YouTubeIdentityType.VideoNoCookieEmbed;

    /// <summary>
    /// Gets a value indicating whether this identity represents a channel.
    /// </summary>
    public bool IsChannel => IdentityType is
        YouTubeIdentityType.ChannelId or
        YouTubeIdentityType.ChannelById or
        YouTubeIdentityType.ChannelByHandle or
        YouTubeIdentityType.ChannelByCustomName or
        YouTubeIdentityType.ChannelByUsername or
        YouTubeIdentityType.ChannelTabByHandle or
        YouTubeIdentityType.ChannelTabById;

    /// <summary>
    /// Gets a value indicating whether this identity represents a playlist.
    /// </summary>
    public bool IsPlaylist => IdentityType is
        YouTubeIdentityType.PlaylistId or
        YouTubeIdentityType.Playlist or
        YouTubeIdentityType.VideoInPlaylist;

    /// <summary>
    /// Gets the primary remote ID (video ID, channel ID, or playlist ID depending on type).
    /// </summary>
    public string? PrimaryRemoteId => IdentityType switch
    {
        YouTubeIdentityType.VideoId or
        YouTubeIdentityType.VideoWatch or
        YouTubeIdentityType.VideoShortUrl or
        YouTubeIdentityType.VideoEmbed or
        YouTubeIdentityType.VideoLegacyEmbed or
        YouTubeIdentityType.VideoShorts or
        YouTubeIdentityType.VideoLive or
        YouTubeIdentityType.VideoInPlaylist or
        YouTubeIdentityType.VideoOEmbed or
        YouTubeIdentityType.VideoAttributionLink or
        YouTubeIdentityType.VideoNoCookieEmbed or
        YouTubeIdentityType.ThumbnailImage => VideoId,

        YouTubeIdentityType.ChannelId or
        YouTubeIdentityType.ChannelById or
        YouTubeIdentityType.ChannelTabById => ChannelId,

        YouTubeIdentityType.ChannelByHandle or
        YouTubeIdentityType.ChannelTabByHandle => ChannelHandle,

        YouTubeIdentityType.ChannelByCustomName => ChannelCustomName,

        YouTubeIdentityType.ChannelByUsername => Username,

        YouTubeIdentityType.PlaylistId or
        YouTubeIdentityType.Playlist => PlaylistId,

        _ => null
    };

    /// <summary>
    /// Converts the parsed parts into a canonical absolute URL.
    /// Returns null if the parts are invalid or cannot be converted to a URL.
    /// </summary>
    /// <param name="includeTimestamp">Whether to include the start time in the URL if present.</param>
    /// <param name="includePlaylistInfo">Whether to include playlist ID and index if present.</param>
    /// <returns>The canonical URL or null if conversion is not possible.</returns>
    public Uri? ToUrl(bool includeTimestamp = false, bool includePlaylistInfo = false)
    {
        if (!IsValid || Errors.Count > 0)
        {
            return null;
        }

        // For video types, build video URL with optional parameters
        if (IsVideo && !string.IsNullOrEmpty(VideoId))
        {
            return BuildVideoUrl(includeTimestamp, includePlaylistInfo);
        }

        // For channel types, return the channel URL
        if (IsChannel)
        {
            return BuildChannelUrl();
        }

        // For playlist types, return the playlist URL
        if (IsPlaylist && !string.IsNullOrEmpty(PlaylistId))
        {
            return BuildPlaylistUrl();
        }

        // Fall back to the pre-computed AbsoluteRemoteUrl
        return AbsoluteRemoteUrl;
    }

    /// <summary>
    /// Builds a video URL with optional parameters.
    /// </summary>
    private Uri? BuildVideoUrl(bool includeTimestamp, bool includePlaylistInfo)
    {
        if (string.IsNullOrEmpty(VideoId))
        {
            return null;
        }

        var urlBuilder = new System.Text.StringBuilder("https://www.youtube.com/watch?v=");
        urlBuilder.Append(VideoId);

        if (includePlaylistInfo && !string.IsNullOrEmpty(PlaylistId))
        {
            urlBuilder.Append("&list=");
            urlBuilder.Append(PlaylistId);

            if (PlaylistIndex.HasValue)
            {
                urlBuilder.Append("&index=");
                urlBuilder.Append(PlaylistIndex.Value);
            }
        }

        if (includeTimestamp && StartTimeSeconds.HasValue && StartTimeSeconds.Value > 0)
        {
            urlBuilder.Append("&t=");
            urlBuilder.Append(StartTimeSeconds.Value);
            urlBuilder.Append('s');
        }

        return new Uri(urlBuilder.ToString());
    }

    /// <summary>
    /// Builds a channel URL from the available channel identifiers.
    /// </summary>
    private Uri? BuildChannelUrl()
    {
        // Prefer channel ID (most stable)
        if (!string.IsNullOrEmpty(ChannelId))
        {
            return new Uri($"https://www.youtube.com/channel/{ChannelId}");
        }

        // Then handle
        if (!string.IsNullOrEmpty(ChannelHandle))
        {
            var handle = ChannelHandle.StartsWith('@') ? ChannelHandle : $"@{ChannelHandle}";
            return new Uri($"https://www.youtube.com/{handle}");
        }

        // Legacy formats - less reliable but still usable
        if (!string.IsNullOrEmpty(ChannelCustomName))
        {
            return new Uri($"https://www.youtube.com/c/{ChannelCustomName}");
        }

        if (!string.IsNullOrEmpty(Username))
        {
            return new Uri($"https://www.youtube.com/user/{Username}");
        }

        return ChannelAbsoluteRemoteUrl;
    }

    /// <summary>
    /// Builds a playlist URL.
    /// </summary>
    private Uri? BuildPlaylistUrl()
    {
        if (string.IsNullOrEmpty(PlaylistId))
        {
            return null;
        }

        return new Uri($"https://www.youtube.com/playlist?list={PlaylistId}");
    }
}
