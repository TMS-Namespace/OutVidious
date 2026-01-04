using System.Text.RegularExpressions;
using System.Web;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;

/// <summary>
/// Classifies YouTube URLs and IDs into their corresponding <see cref="YouTubeIdentityType"/>.
/// This class performs rough classification based on URL patterns without full validation.
/// </summary>
public static partial class YouTubeIdentityClassifier
{
    // YouTube domain patterns
    private static readonly string[] YouTubeDomains =
    [
        "youtube.com",
        "www.youtube.com",
        "m.youtube.com",
        "youtu.be",
        "youtube-nocookie.com",
        "www.youtube-nocookie.com",
        "music.youtube.com",
        "youtube.googleapis.com",
        "i.ytimg.com",
        "img.youtube.com"
    ];

    /// <summary>
    /// Classifies the given input string into a YouTube identity type.
    /// </summary>
    /// <param name="input">The input string to classify (URL or ID).</param>
    /// <returns>The classified <see cref="YouTubeIdentityType"/>.</returns>
    public static YouTubeIdentityType Classify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return YouTubeIdentityType.Unrecognized;
        }

        var trimmedInput = input.Trim();

        // First, check if it's a raw ID (no URL structure)
        if (!ContainsUrlIndicators(trimmedInput))
        {
            return ClassifyRawId(trimmedInput);
        }

        // Try to parse as URL
        return ClassifyUrl(trimmedInput);
    }

    /// <summary>
    /// Gets the YouTube domain type from a host string.
    /// </summary>
    /// <param name="host">The host string to check.</param>
    /// <returns>The <see cref="YouTubeDomain"/> type.</returns>
    public static YouTubeDomain GetDomainType(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return YouTubeDomain.Unknown;
        }

        var normalizedHost = host.ToLowerInvariant();

        return normalizedHost switch
        {
            "youtu.be" => YouTubeDomain.YouTuBe,
            "youtube-nocookie.com" or "www.youtube-nocookie.com" => YouTubeDomain.YouTubeNoCookie,
            "music.youtube.com" => YouTubeDomain.YouTubeMusic,
            "youtube.googleapis.com" => YouTubeDomain.YouTubeApi,
            "i.ytimg.com" or "i1.ytimg.com" or "i2.ytimg.com" or "i3.ytimg.com" or "i4.ytimg.com" => YouTubeDomain.YouTubeImages,
            "img.youtube.com" => YouTubeDomain.YouTubeImagesAlt,
            "m.youtube.com" => YouTubeDomain.YouTubeMobile,
            "youtube.com" or "www.youtube.com" => YouTubeDomain.YouTube,
            _ when normalizedHost.EndsWith(".youtube.com") => YouTubeDomain.YouTube,
            _ when normalizedHost.EndsWith(".youtu.be") => YouTubeDomain.YouTuBe,
            _ => YouTubeDomain.Unknown
        };
    }

    /// <summary>
    /// Checks if the input string appears to be a URL.
    /// </summary>
    private static bool ContainsUrlIndicators(string input)
    {
        return input.Contains("://") ||
               input.Contains('/') ||
               input.Contains('?') ||
               input.Contains('.') && (
                   input.Contains("youtu") ||
                   input.Contains("ytimg"));
    }

    /// <summary>
    /// Classifies a raw ID (not a URL).
    /// </summary>
    private static YouTubeIdentityType ClassifyRawId(string input)
    {
        // Video ID: 11 characters, base64-like
        if (VideoIdRegex().IsMatch(input))
        {
            return YouTubeIdentityType.VideoId;
        }

        // Channel ID: starts with UC, 24 characters
        if (ChannelIdRegex().IsMatch(input))
        {
            return YouTubeIdentityType.ChannelId;
        }

        // Playlist ID: various prefixes (PL, RD, UU, LL, FL, etc.)
        if (PlaylistIdRegex().IsMatch(input))
        {
            return YouTubeIdentityType.PlaylistId;
        }

        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies a URL string.
    /// </summary>
    private static YouTubeIdentityType ClassifyUrl(string input)
    {
        // Normalize URL: handle protocol-relative and missing protocol
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            // Try without scheme for URLs like "youtube.com/..."
            if (!Uri.TryCreate("https://" + input.TrimStart('/'), UriKind.Absolute, out uri))
            {
                return YouTubeIdentityType.Unrecognized;
            }
        }

        var domainType = GetDomainType(uri.Host);
        if (domainType == YouTubeDomain.Unknown)
        {
            return YouTubeIdentityType.Unrecognized;
        }

        return ClassifyByDomainAndPath(uri, domainType);
    }

    /// <summary>
    /// Normalizes a URL string for parsing.
    /// </summary>
    private static string NormalizeUrl(string input)
    {
        var trimmed = input.Trim();

        // Handle protocol-relative URLs
        if (trimmed.StartsWith("//"))
        {
            return "https:" + trimmed;
        }

        // Handle URLs without protocol
        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + trimmed;
        }

        return trimmed;
    }

    /// <summary>
    /// Classifies URL based on domain type and path.
    /// </summary>
    private static YouTubeIdentityType ClassifyByDomainAndPath(Uri uri, YouTubeDomain domainType)
    {
        var path = uri.AbsolutePath.ToLowerInvariant();
        var query = HttpUtility.ParseQueryString(uri.Query);

        return domainType switch
        {
            YouTubeDomain.YouTuBe => ClassifyShortUrl(uri),
            YouTubeDomain.YouTubeNoCookie => ClassifyNoCookieUrl(path),
            YouTubeDomain.YouTubeMusic => YouTubeIdentityType.YouTubeMusic,
            YouTubeDomain.YouTubeApi => ClassifyApiUrl(path),
            YouTubeDomain.YouTubeImages or YouTubeDomain.YouTubeImagesAlt => YouTubeIdentityType.ThumbnailImage,
            YouTubeDomain.YouTube or YouTubeDomain.YouTubeMobile => ClassifyMainYouTubeUrl(path, query),
            _ => YouTubeIdentityType.Unrecognized
        };
    }

    /// <summary>
    /// Classifies youtu.be short URL.
    /// </summary>
    private static YouTubeIdentityType ClassifyShortUrl(Uri uri)
    {
        var pathSegment = uri.AbsolutePath.TrimStart('/').Split('?')[0].Split('&')[0];

        // youtu.be URLs are video short URLs
        if (!string.IsNullOrEmpty(pathSegment) && VideoIdRegex().IsMatch(pathSegment))
        {
            // Check for playlist parameter
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(query["list"]))
            {
                return YouTubeIdentityType.VideoInPlaylist;
            }

            return YouTubeIdentityType.VideoShortUrl;
        }

        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies youtube-nocookie.com URL.
    /// </summary>
    private static YouTubeIdentityType ClassifyNoCookieUrl(string path)
    {
        if (path.StartsWith("/embed/") || path.StartsWith("/v/"))
        {
            return YouTubeIdentityType.VideoNoCookieEmbed;
        }

        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies youtube.googleapis.com URL.
    /// </summary>
    private static YouTubeIdentityType ClassifyApiUrl(string path)
    {
        if (path.StartsWith("/v/"))
        {
            return YouTubeIdentityType.ApiUrl;
        }

        return YouTubeIdentityType.ApiUrl;
    }

    /// <summary>
    /// Classifies main youtube.com URL.
    /// </summary>
    private static YouTubeIdentityType ClassifyMainYouTubeUrl(
        string path,
        System.Collections.Specialized.NameValueCollection query)
    {
        // Check for watch URLs first
        if (path == "/watch" || path.StartsWith("/watch?") || path.StartsWith("/watch/"))
        {
            return ClassifyWatchUrl(query, path);
        }

        // Embed URLs
        if (path.StartsWith("/embed/"))
        {
            return YouTubeIdentityType.VideoEmbed;
        }

        // Legacy v/ URLs
        if (path.StartsWith("/v/"))
        {
            return YouTubeIdentityType.VideoLegacyEmbed;
        }

        // e/ URLs (shorthand for embed)
        if (path.StartsWith("/e/"))
        {
            return YouTubeIdentityType.VideoEmbed;
        }

        // Shorts URLs
        if (path.StartsWith("/shorts/"))
        {
            return YouTubeIdentityType.VideoShorts;
        }

        // Live URLs
        if (path.StartsWith("/live/"))
        {
            return YouTubeIdentityType.VideoLive;
        }

        // Playlist URLs
        if (path == "/playlist" || path.StartsWith("/playlist?"))
        {
            return YouTubeIdentityType.Playlist;
        }

        // Channel by ID
        if (path.StartsWith("/channel/"))
        {
            return ClassifyChannelByIdUrl(path);
        }

        // Channel by handle (@)
        if (path.StartsWith("/@"))
        {
            return ClassifyHandleUrl(path);
        }

        // Legacy channel by custom name (/c/)
        if (path.StartsWith("/c/"))
        {
            return ClassifyLegacyChannelUrl(path, YouTubeIdentityType.ChannelByCustomName);
        }

        // Legacy channel by username (/user/)
        if (path.StartsWith("/user/"))
        {
            return ClassifyLegacyChannelUrl(path, YouTubeIdentityType.ChannelByUsername);
        }

        // oEmbed URLs
        if (path.StartsWith("/oembed"))
        {
            return YouTubeIdentityType.VideoOEmbed;
        }

        // Attribution links
        if (path.StartsWith("/attribution_link"))
        {
            return YouTubeIdentityType.VideoAttributionLink;
        }

        // Feed URLs
        if (path.StartsWith("/feed/"))
        {
            return YouTubeIdentityType.Feed;
        }

        // ytscreeningroom (legacy)
        if (path.StartsWith("/ytscreeningroom"))
        {
            if (!string.IsNullOrEmpty(query["v"]))
            {
                return YouTubeIdentityType.VideoWatch;
            }
        }

        // Check for v= in query on root path
        if (!string.IsNullOrEmpty(query["v"]) || !string.IsNullOrEmpty(query["vi"]))
        {
            if (!string.IsNullOrEmpty(query["list"]))
            {
                return YouTubeIdentityType.VideoInPlaylist;
            }
            return YouTubeIdentityType.VideoWatch;
        }

        // Check for list= in query (playlist)
        if (!string.IsNullOrEmpty(query["list"]))
        {
            return YouTubeIdentityType.Playlist;
        }

        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies watch URL based on query parameters.
    /// </summary>
    private static YouTubeIdentityType ClassifyWatchUrl(
        System.Collections.Specialized.NameValueCollection query,
        string path)
    {
        // Check for v= or vi= parameter
        var hasVideo = !string.IsNullOrEmpty(query["v"]) || !string.IsNullOrEmpty(query["vi"]);
        var hasList = !string.IsNullOrEmpty(query["list"]);

        // Also check for /watch/VIDEO_ID format
        if (!hasVideo && path.StartsWith("/watch/"))
        {
            var videoId = path.Substring("/watch/".Length).Split('?')[0].Split('/')[0];
            if (VideoIdRegex().IsMatch(videoId))
            {
                hasVideo = true;
            }
        }

        if (hasVideo && hasList)
        {
            return YouTubeIdentityType.VideoInPlaylist;
        }

        if (hasVideo)
        {
            return YouTubeIdentityType.VideoWatch;
        }

        if (hasList)
        {
            return YouTubeIdentityType.Playlist;
        }

        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies channel by ID URL.
    /// </summary>
    private static YouTubeIdentityType ClassifyChannelByIdUrl(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2)
        {
            // Check for tab
            if (segments.Length >= 3 && IsChannelTab(segments[2]))
            {
                return YouTubeIdentityType.ChannelTabById;
            }
            return YouTubeIdentityType.ChannelById;
        }
        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies handle URL (@username).
    /// </summary>
    private static YouTubeIdentityType ClassifyHandleUrl(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 1)
        {
            // Check for tab
            if (segments.Length >= 2 && IsChannelTab(segments[1]))
            {
                return YouTubeIdentityType.ChannelTabByHandle;
            }
            return YouTubeIdentityType.ChannelByHandle;
        }
        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Classifies legacy channel URL (/c/ or /user/).
    /// </summary>
    private static YouTubeIdentityType ClassifyLegacyChannelUrl(string path, YouTubeIdentityType baseType)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2)
        {
            // Check for tab
            if (segments.Length >= 3 && IsChannelTab(segments[2]))
            {
                // For legacy URLs, we still classify as the base type
                // The parser will extract tab information
                return baseType;
            }
            return baseType;
        }
        return YouTubeIdentityType.Unrecognized;
    }

    /// <summary>
    /// Checks if a path segment is a channel tab.
    /// </summary>
    private static bool IsChannelTab(string segment)
    {
        var tabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "featured", "videos", "shorts", "streams", "releases",
            "playlists", "community", "channels", "about", "search",
            "podcasts", "store"
        };
        return tabNames.Contains(segment);
    }

    // Regex patterns for ID validation

    /// <summary>
    /// Matches a YouTube video ID (11 characters, base64url-safe characters).
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    internal static partial Regex VideoIdRegex();

    /// <summary>
    /// Matches a YouTube channel ID (starts with UC, 24 characters total).
    /// </summary>
    [GeneratedRegex(@"^UC[a-zA-Z0-9_-]{22}$")]
    internal static partial Regex ChannelIdRegex();

    /// <summary>
    /// Matches a YouTube playlist ID (various prefixes, 13-34+ characters).
    /// Common prefixes: PL, RD, UU, LL, FL, WL, OL, EL, EC, PP
    /// </summary>
    [GeneratedRegex(@"^(PL|RD|UU|LL|FL|WL|OL|EL|EC|PP)[a-zA-Z0-9_-]{10,50}$")]
    internal static partial Regex PlaylistIdRegex();
}
