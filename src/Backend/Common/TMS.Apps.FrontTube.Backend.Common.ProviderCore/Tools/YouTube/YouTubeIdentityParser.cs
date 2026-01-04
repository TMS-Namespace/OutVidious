using System.Text.RegularExpressions;
using System.Web;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;

/// <summary>
/// Parses YouTube URLs and IDs into their component parts.
/// Combines classification, validation, and extraction into a comprehensive parsing result.
/// </summary>
public static partial class YouTubeIdentityParser
{
    private const string YouTubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
    private const string YouTubeChannelBaseUrl = "https://www.youtube.com/channel/";
    private const string YouTubePlaylistBaseUrl = "https://www.youtube.com/playlist?list=";

    /// <summary>
    /// Attempts to parse a YouTube URL or ID string into its component parts.
    /// </summary>
    /// <param name="input">The input string to parse (URL or ID).</param>
    /// <param name="parts">The parsed parts if successful, otherwise an error result.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParse(string? input, out YouTubeIdentityParts parts)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(input))
        {
            parts = CreateErrorResult(input ?? string.Empty, ["Input string is null or empty."]);
            return false;
        }

        var trimmedInput = input.Trim();

        // Step 1: Classify the input
        var identityType = YouTubeIdentityClassifier.Classify(trimmedInput);

        if (identityType == YouTubeIdentityType.Unrecognized)
        {
            parts = CreateErrorResult(trimmedInput, ["Unable to recognize input as a valid YouTube URL or ID."]);
            return false;
        }

        // Step 2: Validate the input
        if (!YouTubeIdentityValidator.TryValidate(trimmedInput, identityType, out var validationResult))
        {
            errors.AddRange(validationResult.Errors);
            // Continue parsing to extract what we can, but mark as invalid
        }

        warnings.AddRange(validationResult.Warnings);

        // Step 3: Parse and extract all components
        parts = ParseInput(trimmedInput, identityType, errors, warnings);
        return parts.IsValid;
    }

    /// <summary>
    /// Parses the input and extracts all components.
    /// </summary>
    private static YouTubeIdentityParts ParseInput(
        string input,
        YouTubeIdentityType identityType,
        List<string> errors,
        List<string> warnings)
    {
        // Handle raw IDs first
        if (IsRawId(identityType))
        {
            return ParseRawId(input, identityType, errors, warnings);
        }

        // Parse as URL
        return ParseUrl(input, identityType, errors, warnings);
    }

    /// <summary>
    /// Checks if the identity type represents a raw ID (not a URL).
    /// </summary>
    private static bool IsRawId(YouTubeIdentityType type)
    {
        return type is YouTubeIdentityType.VideoId or
            YouTubeIdentityType.ChannelId or
            YouTubeIdentityType.PlaylistId;
    }

    /// <summary>
    /// Parses a raw ID.
    /// </summary>
    private static YouTubeIdentityParts ParseRawId(
        string input,
        YouTubeIdentityType identityType,
        List<string> errors,
        List<string> warnings)
    {
        return identityType switch
        {
            YouTubeIdentityType.VideoId => new YouTubeIdentityParts
            {
                OriginalInput = input,
                IdentityType = identityType,
                IsValid = errors.Count == 0,
                VideoId = input,
                AbsoluteRemoteUrl = BuildVideoUrl(input),
                Errors = errors,
                Warnings = warnings
            },

            YouTubeIdentityType.ChannelId => new YouTubeIdentityParts
            {
                OriginalInput = input,
                IdentityType = identityType,
                IsValid = errors.Count == 0,
                ChannelId = input,
                AbsoluteRemoteUrl = BuildChannelUrl(input, null),
                ChannelAbsoluteRemoteUrl = BuildChannelUrl(input, null),
                Errors = errors,
                Warnings = warnings
            },

            YouTubeIdentityType.PlaylistId => new YouTubeIdentityParts
            {
                OriginalInput = input,
                IdentityType = identityType,
                IsValid = errors.Count == 0,
                PlaylistId = input,
                AbsoluteRemoteUrl = BuildPlaylistUrl(input),
                Errors = errors,
                Warnings = warnings
            },

            _ => CreateErrorResult(input, ["Unknown raw ID type."])
        };
    }

    /// <summary>
    /// Parses a URL.
    /// </summary>
    private static YouTubeIdentityParts ParseUrl(
        string input,
        YouTubeIdentityType identityType,
        List<string> errors,
        List<string> warnings)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            errors.Add("Unable to parse input as a valid URL.");
            return CreateErrorResult(input, errors);
        }

        // Extract base URL components
        var protocol = uri.Scheme;
        var host = uri.Host;
        var subdomain = ExtractSubdomain(host);
        var domainType = YouTubeIdentityClassifier.GetDomainType(host);
        var path = uri.AbsolutePath;
        var query = HttpUtility.ParseQueryString(uri.Query);
        var fragment = string.IsNullOrEmpty(uri.Fragment) ? null : uri.Fragment.TrimStart('#');

        // Extract query parameters
        var queryParams = new Dictionary<string, string>();
        foreach (string? key in query.AllKeys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                queryParams[key] = query[key] ?? string.Empty;
            }
        }

        // Extract type-specific components
        var videoId = ExtractVideoId(uri, identityType);
        var channelId = ExtractChannelId(uri, identityType);
        var channelHandle = ExtractChannelHandle(uri, identityType);
        var channelCustomName = ExtractChannelCustomName(uri, identityType);
        var username = ExtractUsername(uri, identityType);
        var playlistId = ExtractPlaylistId(uri, identityType);
        var channelTab = ExtractChannelTab(uri, identityType);
        var playlistIndex = ExtractPlaylistIndex(query);
        var startTime = ExtractStartTime(query, fragment);

        // Build canonical URLs
        var absoluteRemoteUrl = BuildAbsoluteRemoteUrl(
            identityType, videoId, channelId, channelHandle, playlistId);
        var channelAbsoluteRemoteUrl = BuildChannelAbsoluteRemoteUrl(
            identityType, channelId, channelHandle, channelCustomName, username);

        return new YouTubeIdentityParts
        {
            OriginalInput = input,
            IdentityType = identityType,
            IsValid = errors.Count == 0,
            Protocol = protocol,
            Subdomain = subdomain,
            Domain = domainType,
            Host = host,
            VideoId = videoId,
            ChannelId = channelId,
            ChannelHandle = channelHandle,
            ChannelCustomName = channelCustomName,
            Username = username,
            PlaylistId = playlistId,
            ChannelTab = channelTab,
            PlaylistIndex = playlistIndex,
            StartTimeSeconds = startTime,
            AbsoluteRemoteUrl = absoluteRemoteUrl,
            ChannelAbsoluteRemoteUrl = channelAbsoluteRemoteUrl,
            QueryParameters = queryParams,
            Fragment = fragment,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Normalizes a URL for parsing.
    /// </summary>
    private static string NormalizeUrl(string input)
    {
        var trimmed = input.Trim();

        if (trimmed.StartsWith("//"))
        {
            return "https:" + trimmed;
        }

        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + trimmed;
        }

        return trimmed;
    }

    /// <summary>
    /// Extracts subdomain from host.
    /// </summary>
    private static string? ExtractSubdomain(string host)
    {
        var parts = host.Split('.');
        if (parts.Length > 2)
        {
            return parts[0];
        }
        return null;
    }

    /// <summary>
    /// Extracts video ID from URL.
    /// </summary>
    private static string? ExtractVideoId(Uri uri, YouTubeIdentityType identityType)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);
        var path = uri.AbsolutePath;

        // Check v= or vi= parameter
        var videoId = query["v"] ?? query["vi"];
        if (!string.IsNullOrEmpty(videoId) && YouTubeIdentityClassifier.VideoIdRegex().IsMatch(videoId))
        {
            return videoId;
        }

        // Extract from path based on type
        return identityType switch
        {
            YouTubeIdentityType.VideoShortUrl =>
                ExtractIdFromShortUrl(path),

            YouTubeIdentityType.VideoEmbed or
            YouTubeIdentityType.VideoNoCookieEmbed =>
                ExtractFromPathPrefix(path, "/embed/"),

            YouTubeIdentityType.VideoLegacyEmbed =>
                ExtractFromPathPrefix(path, "/v/") ?? ExtractFromPathPrefix(path, "/e/"),

            YouTubeIdentityType.VideoShorts =>
                ExtractFromPathPrefix(path, "/shorts/"),

            YouTubeIdentityType.VideoLive =>
                ExtractFromPathPrefix(path, "/live/"),

            YouTubeIdentityType.VideoWatch when path.StartsWith("/watch/") =>
                ExtractFromPathPrefix(path, "/watch/"),

            YouTubeIdentityType.VideoOEmbed =>
                ExtractVideoIdFromOEmbed(query["url"]),

            YouTubeIdentityType.VideoAttributionLink =>
                ExtractVideoIdFromAttributionLink(query["u"]),

            YouTubeIdentityType.ThumbnailImage =>
                ExtractVideoIdFromThumbnailPath(path),

            YouTubeIdentityType.ApiUrl =>
                ExtractFromPathPrefix(path, "/v/"),

            _ => null
        };
    }

    /// <summary>
    /// Extracts ID from short URL path (youtu.be/VIDEO_ID).
    /// </summary>
    private static string? ExtractIdFromShortUrl(string path)
    {
        var segment = path.TrimStart('/').Split('?')[0].Split('&')[0].Split('/')[0];
        if (!string.IsNullOrEmpty(segment) && YouTubeIdentityClassifier.VideoIdRegex().IsMatch(segment))
        {
            return segment;
        }
        return null;
    }

    /// <summary>
    /// Extracts value from path after a prefix.
    /// </summary>
    private static string? ExtractFromPathPrefix(string path, string prefix)
    {
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var value = path.Substring(prefix.Length).Split('?')[0].Split('/')[0].Split('&')[0];

        // Handle embed/watch format like /embed/watch?v=...
        if (value.Equals("watch", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(value) && YouTubeIdentityClassifier.VideoIdRegex().IsMatch(value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Extracts video ID from oEmbed URL.
    /// </summary>
    private static string? ExtractVideoIdFromOEmbed(string? embeddedUrl)
    {
        if (string.IsNullOrEmpty(embeddedUrl))
        {
            return null;
        }

        var decoded = HttpUtility.UrlDecode(embeddedUrl);
        if (Uri.TryCreate(decoded, UriKind.Absolute, out var uri))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        return null;
    }

    /// <summary>
    /// Extracts video ID from attribution link.
    /// </summary>
    private static string? ExtractVideoIdFromAttributionLink(string? uParam)
    {
        if (string.IsNullOrEmpty(uParam))
        {
            return null;
        }

        var decoded = HttpUtility.UrlDecode(uParam);

        if (Uri.TryCreate("https://youtube.com" + decoded, UriKind.Absolute, out var uri))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        return null;
    }

    /// <summary>
    /// Extracts video ID from thumbnail path.
    /// </summary>
    private static string? ExtractVideoIdFromThumbnailPath(string path)
    {
        // Format: /vi/VIDEO_ID/quality.jpg or /vi_webp/VIDEO_ID/quality.webp
        var match = ThumbnailPathRegex().Match(path);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts channel ID from URL.
    /// </summary>
    private static string? ExtractChannelId(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType is not (YouTubeIdentityType.ChannelById or YouTubeIdentityType.ChannelTabById))
        {
            return null;
        }

        var match = ChannelIdPathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts channel handle from URL.
    /// </summary>
    private static string? ExtractChannelHandle(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType is not (YouTubeIdentityType.ChannelByHandle or YouTubeIdentityType.ChannelTabByHandle))
        {
            return null;
        }

        var match = HandlePathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts channel custom name from URL.
    /// </summary>
    private static string? ExtractChannelCustomName(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType != YouTubeIdentityType.ChannelByCustomName)
        {
            return null;
        }

        var match = CustomNamePathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts username from URL.
    /// </summary>
    private static string? ExtractUsername(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType != YouTubeIdentityType.ChannelByUsername)
        {
            return null;
        }

        var match = UsernamePathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts playlist ID from URL.
    /// </summary>
    private static string? ExtractPlaylistId(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType is not (
            YouTubeIdentityType.Playlist or
            YouTubeIdentityType.VideoInPlaylist))
        {
            return null;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        return query["list"];
    }

    /// <summary>
    /// Extracts channel tab from URL.
    /// </summary>
    private static YouTubeChannelTab ExtractChannelTab(Uri uri, YouTubeIdentityType identityType)
    {
        if (identityType is not (
            YouTubeIdentityType.ChannelTabByHandle or
            YouTubeIdentityType.ChannelTabById or
            YouTubeIdentityType.ChannelByCustomName or
            YouTubeIdentityType.ChannelByUsername))
        {
            return YouTubeChannelTab.None;
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Tab is typically the last meaningful segment
        foreach (var segment in segments.Reverse())
        {
            var tab = ParseChannelTab(segment);
            if (tab != YouTubeChannelTab.None)
            {
                return tab;
            }
        }

        return YouTubeChannelTab.None;
    }

    /// <summary>
    /// Parses a path segment into a channel tab.
    /// </summary>
    private static YouTubeChannelTab ParseChannelTab(string segment)
    {
        return segment.ToLowerInvariant() switch
        {
            "featured" => YouTubeChannelTab.Featured,
            "videos" => YouTubeChannelTab.Videos,
            "shorts" => YouTubeChannelTab.Shorts,
            "streams" or "live" => YouTubeChannelTab.Streams,
            "releases" => YouTubeChannelTab.Releases,
            "playlists" => YouTubeChannelTab.Playlists,
            "community" => YouTubeChannelTab.Community,
            "channels" => YouTubeChannelTab.Channels,
            "about" => YouTubeChannelTab.About,
            "search" => YouTubeChannelTab.Search,
            "podcasts" => YouTubeChannelTab.Podcasts,
            "store" => YouTubeChannelTab.Store,
            _ => YouTubeChannelTab.None
        };
    }

    /// <summary>
    /// Extracts playlist index from query.
    /// </summary>
    private static int? ExtractPlaylistIndex(System.Collections.Specialized.NameValueCollection query)
    {
        var indexStr = query["index"];
        if (!string.IsNullOrEmpty(indexStr) && int.TryParse(indexStr, out var index))
        {
            return index;
        }
        return null;
    }

    /// <summary>
    /// Extracts start time in seconds from query or fragment.
    /// </summary>
    private static int? ExtractStartTime(
        System.Collections.Specialized.NameValueCollection query,
        string? fragment)
    {
        // Check t parameter
        var timeStr = query["t"] ?? query["start"] ?? query["time_continue"];

        if (!string.IsNullOrEmpty(timeStr))
        {
            return ParseTimeString(timeStr);
        }

        // Check fragment for time (e.g., #t=1m30s)
        if (!string.IsNullOrEmpty(fragment))
        {
            var match = FragmentTimeRegex().Match(fragment);
            if (match.Success)
            {
                return ParseTimeString(match.Groups[1].Value);
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a time string into seconds.
    /// Supports formats: "123", "123s", "1m30s", "1h2m30s", "0m10s"
    /// </summary>
    private static int? ParseTimeString(string timeStr)
    {
        if (string.IsNullOrEmpty(timeStr))
        {
            return null;
        }

        // Plain seconds
        if (int.TryParse(timeStr.TrimEnd('s'), out var plainSeconds))
        {
            return plainSeconds;
        }

        // Parse h/m/s format
        var match = TimeFormatRegex().Match(timeStr);
        if (match.Success)
        {
            var hours = 0;
            var minutes = 0;
            var seconds = 0;

            if (!string.IsNullOrEmpty(match.Groups[1].Value))
            {
                int.TryParse(match.Groups[1].Value, out hours);
            }
            if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                int.TryParse(match.Groups[2].Value, out minutes);
            }
            if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                int.TryParse(match.Groups[3].Value, out seconds);
            }

            return (hours * 3600) + (minutes * 60) + seconds;
        }

        return null;
    }

    /// <summary>
    /// Builds the canonical absolute remote URL based on identity type.
    /// </summary>
    private static Uri? BuildAbsoluteRemoteUrl(
        YouTubeIdentityType identityType,
        string? videoId,
        string? channelId,
        string? channelHandle,
        string? playlistId)
    {
        if (!string.IsNullOrEmpty(videoId))
        {
            return BuildVideoUrl(videoId);
        }

        if (!string.IsNullOrEmpty(channelId))
        {
            return BuildChannelUrl(channelId, null);
        }

        if (!string.IsNullOrEmpty(channelHandle))
        {
            return BuildChannelUrl(null, channelHandle);
        }

        if (!string.IsNullOrEmpty(playlistId))
        {
            return BuildPlaylistUrl(playlistId);
        }

        return null;
    }

    /// <summary>
    /// Builds the channel absolute remote URL.
    /// </summary>
    private static Uri? BuildChannelAbsoluteRemoteUrl(
        YouTubeIdentityType identityType,
        string? channelId,
        string? channelHandle,
        string? channelCustomName,
        string? username)
    {
        if (!string.IsNullOrEmpty(channelId))
        {
            return BuildChannelUrl(channelId, null);
        }

        if (!string.IsNullOrEmpty(channelHandle))
        {
            return BuildChannelUrl(null, channelHandle);
        }

        // For legacy formats, we can't build a canonical URL
        return null;
    }

    /// <summary>
    /// Builds a canonical video URL.
    /// </summary>
    private static Uri? BuildVideoUrl(string videoId)
    {
        if (string.IsNullOrEmpty(videoId))
        {
            return null;
        }

        return new Uri(YouTubeVideoBaseUrl + videoId);
    }

    /// <summary>
    /// Builds a canonical channel URL.
    /// </summary>
    private static Uri? BuildChannelUrl(string? channelId, string? handle)
    {
        if (!string.IsNullOrEmpty(channelId))
        {
            return new Uri(YouTubeChannelBaseUrl + channelId);
        }

        if (!string.IsNullOrEmpty(handle))
        {
            var normalizedHandle = handle.StartsWith("@") ? handle : "@" + handle;
            return new Uri($"https://www.youtube.com/{normalizedHandle}");
        }

        return null;
    }

    /// <summary>
    /// Builds a canonical playlist URL.
    /// </summary>
    private static Uri? BuildPlaylistUrl(string playlistId)
    {
        if (string.IsNullOrEmpty(playlistId))
        {
            return null;
        }

        return new Uri(YouTubePlaylistBaseUrl + playlistId);
    }

    /// <summary>
    /// Creates an error result.
    /// </summary>
    private static YouTubeIdentityParts CreateErrorResult(string input, IReadOnlyList<string> errors)
    {
        return new YouTubeIdentityParts
        {
            OriginalInput = input,
            IdentityType = YouTubeIdentityType.Unrecognized,
            IsValid = false,
            Errors = errors
        };
    }

    // Regex patterns

    /// <summary>
    /// Matches channel ID in path.
    /// </summary>
    [GeneratedRegex(@"^/channel/(UC[a-zA-Z0-9_-]+)")]
    private static partial Regex ChannelIdPathRegex();

    /// <summary>
    /// Matches handle in path.
    /// </summary>
    [GeneratedRegex(@"^/(@[a-zA-Z0-9_.-]+)")]
    private static partial Regex HandlePathRegex();

    /// <summary>
    /// Matches custom name in path (/c/...).
    /// </summary>
    [GeneratedRegex(@"^/c/([a-zA-Z0-9_.-]+)")]
    private static partial Regex CustomNamePathRegex();

    /// <summary>
    /// Matches username in path (/user/...).
    /// </summary>
    [GeneratedRegex(@"^/user/([a-zA-Z0-9_.-]+)")]
    private static partial Regex UsernamePathRegex();

    /// <summary>
    /// Matches thumbnail path (/vi/VIDEO_ID/...).
    /// </summary>
    [GeneratedRegex(@"^/vi(?:_webp)?/([a-zA-Z0-9_-]{11})/")]
    private static partial Regex ThumbnailPathRegex();

    /// <summary>
    /// Matches time in fragment (#t=...).
    /// </summary>
    [GeneratedRegex(@"^t=(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex FragmentTimeRegex();

    /// <summary>
    /// Matches time format (XhXmXs).
    /// </summary>
    [GeneratedRegex(@"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s?)?$", RegexOptions.IgnoreCase)]
    private static partial Regex TimeFormatRegex();
}
