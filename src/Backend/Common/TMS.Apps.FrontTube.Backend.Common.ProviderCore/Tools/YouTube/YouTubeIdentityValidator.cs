using System.Text.RegularExpressions;
using System.Web;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube;

/// <summary>
/// Validates YouTube URLs and IDs against their expected format.
/// This class performs detailed validation based on the identity type.
/// </summary>
public static partial class YouTubeIdentityValidator
{
    /// <summary>
    /// Validates the input string as the specified identity type.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="expectedType">The expected identity type.</param>
    /// <param name="result">The validation result with errors if any.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool TryValidate(
        string? input,
        YouTubeIdentityType expectedType,
        out YouTubeValidationResult result)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(input))
        {
            result = YouTubeValidationResult.Failure("Input string is null or empty.");
            return false;
        }

        var trimmedInput = input.Trim();

        // Classify the input first
        var classifiedType = YouTubeIdentityClassifier.Classify(trimmedInput);

        // Check if classification matches expected type
        if (classifiedType != expectedType)
        {
            // Allow some flexibility - e.g., VideoWatch and VideoInPlaylist are related
            if (!AreTypesCompatible(classifiedType, expectedType))
            {
                errors.Add($"Input classified as '{classifiedType}' but expected '{expectedType}'.");
                result = YouTubeValidationResult.Failure(errors);
                return false;
            }
            warnings.Add($"Input classified as '{classifiedType}', treating as compatible with '{expectedType}'.");
        }

        // Perform type-specific validation
        var typeValidationErrors = ValidateByType(trimmedInput, expectedType);
        errors.AddRange(typeValidationErrors);

        if (errors.Count > 0)
        {
            result = YouTubeValidationResult.Failure(errors);
            return false;
        }

        result = warnings.Count > 0
            ? YouTubeValidationResult.SuccessWithWarnings(warnings)
            : YouTubeValidationResult.Success();
        return true;
    }

    /// <summary>
    /// Validates the input string without requiring a specific type.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="result">The validation result with errors if any.</param>
    /// <returns>True if valid as any YouTube identity, false otherwise.</returns>
    public static bool TryValidate(string? input, out YouTubeValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = YouTubeValidationResult.Failure("Input string is null or empty.");
            return false;
        }

        var trimmedInput = input.Trim();
        var classifiedType = YouTubeIdentityClassifier.Classify(trimmedInput);

        if (classifiedType == YouTubeIdentityType.Unrecognized)
        {
            result = YouTubeValidationResult.Failure("Input does not match any known YouTube URL or ID format.");
            return false;
        }

        return TryValidate(input, classifiedType, out result);
    }

    /// <summary>
    /// Checks if two identity types are compatible for validation purposes.
    /// </summary>
    private static bool AreTypesCompatible(YouTubeIdentityType actual, YouTubeIdentityType expected)
    {
        // Video types that are compatible with each other
        var videoTypes = new HashSet<YouTubeIdentityType>
        {
            YouTubeIdentityType.VideoId,
            YouTubeIdentityType.VideoWatch,
            YouTubeIdentityType.VideoShortUrl,
            YouTubeIdentityType.VideoEmbed,
            YouTubeIdentityType.VideoLegacyEmbed,
            YouTubeIdentityType.VideoShorts,
            YouTubeIdentityType.VideoLive,
            YouTubeIdentityType.VideoInPlaylist,
            YouTubeIdentityType.VideoOEmbed,
            YouTubeIdentityType.VideoAttributionLink,
            YouTubeIdentityType.VideoNoCookieEmbed
        };

        // Check if both are video types
        if (videoTypes.Contains(actual) && videoTypes.Contains(expected))
        {
            return true;
        }

        // Channel types that are compatible
        var channelTypes = new HashSet<YouTubeIdentityType>
        {
            YouTubeIdentityType.ChannelId,
            YouTubeIdentityType.ChannelById,
            YouTubeIdentityType.ChannelByHandle,
            YouTubeIdentityType.ChannelByCustomName,
            YouTubeIdentityType.ChannelByUsername,
            YouTubeIdentityType.ChannelTabByHandle,
            YouTubeIdentityType.ChannelTabById
        };

        if (channelTypes.Contains(actual) && channelTypes.Contains(expected))
        {
            return true;
        }

        // Playlist types
        var playlistTypes = new HashSet<YouTubeIdentityType>
        {
            YouTubeIdentityType.PlaylistId,
            YouTubeIdentityType.Playlist
        };

        if (playlistTypes.Contains(actual) && playlistTypes.Contains(expected))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Performs type-specific validation.
    /// </summary>
    private static List<string> ValidateByType(string input, YouTubeIdentityType type)
    {
        var errors = new List<string>();

        switch (type)
        {
            case YouTubeIdentityType.VideoId:
                ValidateVideoId(input, errors);
                break;

            case YouTubeIdentityType.VideoWatch:
            case YouTubeIdentityType.VideoShortUrl:
            case YouTubeIdentityType.VideoEmbed:
            case YouTubeIdentityType.VideoLegacyEmbed:
            case YouTubeIdentityType.VideoShorts:
            case YouTubeIdentityType.VideoLive:
            case YouTubeIdentityType.VideoInPlaylist:
            case YouTubeIdentityType.VideoOEmbed:
            case YouTubeIdentityType.VideoAttributionLink:
            case YouTubeIdentityType.VideoNoCookieEmbed:
                ValidateVideoUrl(input, type, errors);
                break;

            case YouTubeIdentityType.ChannelId:
                ValidateChannelId(input, errors);
                break;

            case YouTubeIdentityType.ChannelById:
            case YouTubeIdentityType.ChannelByHandle:
            case YouTubeIdentityType.ChannelByCustomName:
            case YouTubeIdentityType.ChannelByUsername:
            case YouTubeIdentityType.ChannelTabByHandle:
            case YouTubeIdentityType.ChannelTabById:
                ValidateChannelUrl(input, type, errors);
                break;

            case YouTubeIdentityType.PlaylistId:
                ValidatePlaylistId(input, errors);
                break;

            case YouTubeIdentityType.Playlist:
                ValidatePlaylistUrl(input, errors);
                break;

            case YouTubeIdentityType.YouTubeMusic:
            case YouTubeIdentityType.ThumbnailImage:
            case YouTubeIdentityType.ApiUrl:
            case YouTubeIdentityType.Feed:
                // Basic URL validation
                ValidateUrlFormat(input, errors);
                break;

            case YouTubeIdentityType.Unrecognized:
                errors.Add("Unrecognized YouTube identity type.");
                break;
        }

        return errors;
    }

    /// <summary>
    /// Validates a video ID.
    /// </summary>
    private static void ValidateVideoId(string input, List<string> errors)
    {
        if (input.Length != 11)
        {
            errors.Add($"Video ID must be exactly 11 characters, got {input.Length}.");
        }

        if (!YouTubeIdentityClassifier.VideoIdRegex().IsMatch(input))
        {
            errors.Add("Video ID contains invalid characters. Only alphanumeric, underscore, and hyphen are allowed.");
        }
    }

    /// <summary>
    /// Validates a channel ID.
    /// </summary>
    private static void ValidateChannelId(string input, List<string> errors)
    {
        if (!input.StartsWith("UC"))
        {
            errors.Add("Channel ID must start with 'UC'.");
        }

        if (input.Length != 24)
        {
            errors.Add($"Channel ID must be exactly 24 characters, got {input.Length}.");
        }

        if (!YouTubeIdentityClassifier.ChannelIdRegex().IsMatch(input))
        {
            errors.Add("Channel ID contains invalid characters.");
        }
    }

    /// <summary>
    /// Validates a playlist ID.
    /// </summary>
    private static void ValidatePlaylistId(string input, List<string> errors)
    {
        var validPrefixes = new[] { "PL", "RD", "UU", "LL", "FL", "WL", "OL", "EL", "EC", "PP" };
        var hasValidPrefix = validPrefixes.Any(p => input.StartsWith(p, StringComparison.Ordinal));

        if (!hasValidPrefix)
        {
            errors.Add($"Playlist ID should start with one of: {string.Join(", ", validPrefixes)}.");
        }

        if (!YouTubeIdentityClassifier.PlaylistIdRegex().IsMatch(input))
        {
            errors.Add("Playlist ID format is invalid.");
        }
    }

    /// <summary>
    /// Validates a video URL and extracts the video ID for validation.
    /// </summary>
    private static void ValidateVideoUrl(string input, YouTubeIdentityType type, List<string> errors)
    {
        if (!ValidateUrlFormat(input, errors))
        {
            return;
        }

        // Extract video ID based on type
        var videoId = ExtractVideoIdFromUrl(input, type);

        if (string.IsNullOrEmpty(videoId))
        {
            errors.Add("Could not extract video ID from URL.");
            return;
        }

        ValidateVideoId(videoId, errors);
    }

    /// <summary>
    /// Validates a channel URL.
    /// </summary>
    private static void ValidateChannelUrl(string input, YouTubeIdentityType type, List<string> errors)
    {
        if (!ValidateUrlFormat(input, errors))
        {
            return;
        }

        // Validate based on channel type
        switch (type)
        {
            case YouTubeIdentityType.ChannelById:
            case YouTubeIdentityType.ChannelTabById:
                var channelId = ExtractChannelIdFromUrl(input);
                if (string.IsNullOrEmpty(channelId))
                {
                    errors.Add("Could not extract channel ID from URL.");
                }
                else
                {
                    ValidateChannelId(channelId, errors);
                }
                break;

            case YouTubeIdentityType.ChannelByHandle:
            case YouTubeIdentityType.ChannelTabByHandle:
                var handle = ExtractHandleFromUrl(input);
                if (string.IsNullOrEmpty(handle))
                {
                    errors.Add("Could not extract handle from URL.");
                }
                else if (!handle.StartsWith("@"))
                {
                    errors.Add("Channel handle must start with '@'.");
                }
                else if (!HandleRegex().IsMatch(handle))
                {
                    errors.Add("Channel handle contains invalid characters.");
                }
                break;

            case YouTubeIdentityType.ChannelByCustomName:
            case YouTubeIdentityType.ChannelByUsername:
                // These are legacy formats, basic validation only
                break;
        }
    }

    /// <summary>
    /// Validates a playlist URL.
    /// </summary>
    private static void ValidatePlaylistUrl(string input, List<string> errors)
    {
        if (!ValidateUrlFormat(input, errors))
        {
            return;
        }

        var playlistId = ExtractPlaylistIdFromUrl(input);
        if (string.IsNullOrEmpty(playlistId))
        {
            errors.Add("Could not extract playlist ID from URL.");
            return;
        }

        ValidatePlaylistId(playlistId, errors);
    }

    /// <summary>
    /// Validates URL format.
    /// </summary>
    private static bool ValidateUrlFormat(string input, List<string> errors)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            errors.Add("Input is not a valid URL format.");
            return false;
        }

        var domainType = YouTubeIdentityClassifier.GetDomainType(uri.Host);
        if (domainType == YouTubeDomain.Unknown)
        {
            errors.Add($"Host '{uri.Host}' is not a recognized YouTube domain.");
            return false;
        }

        return true;
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
    /// Extracts video ID from various URL formats.
    /// </summary>
    private static string? ExtractVideoIdFromUrl(string input, YouTubeIdentityType type)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        var path = uri.AbsolutePath;

        // Check v= or vi= parameter first
        var videoId = query["v"] ?? query["vi"];
        if (!string.IsNullOrEmpty(videoId) && YouTubeIdentityClassifier.VideoIdRegex().IsMatch(videoId))
        {
            return videoId;
        }

        // Path-based extraction
        return type switch
        {
            YouTubeIdentityType.VideoShortUrl =>
                path.TrimStart('/').Split('?')[0].Split('&')[0],

            YouTubeIdentityType.VideoEmbed or
            YouTubeIdentityType.VideoNoCookieEmbed =>
                ExtractFromPath(path, "/embed/"),

            YouTubeIdentityType.VideoLegacyEmbed =>
                ExtractFromPath(path, "/v/") ?? ExtractFromPath(path, "/e/"),

            YouTubeIdentityType.VideoShorts =>
                ExtractFromPath(path, "/shorts/"),

            YouTubeIdentityType.VideoLive =>
                ExtractFromPath(path, "/live/"),

            YouTubeIdentityType.VideoWatch when path.StartsWith("/watch/") =>
                path.Substring("/watch/".Length).Split('?')[0].Split('/')[0],

            YouTubeIdentityType.VideoOEmbed =>
                ExtractFromOEmbedUrl(query["url"]),

            YouTubeIdentityType.VideoAttributionLink =>
                ExtractFromAttributionLink(query["u"]),

            _ => null
        };
    }

    /// <summary>
    /// Extracts a value from a path after a prefix.
    /// </summary>
    private static string? ExtractFromPath(string path, string prefix)
    {
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var value = path.Substring(prefix.Length).Split('?')[0].Split('/')[0].Split('&')[0];
        return string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    /// Extracts video ID from oEmbed URL parameter.
    /// </summary>
    private static string? ExtractFromOEmbedUrl(string? embeddedUrl)
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
    /// Extracts video ID from attribution link u parameter.
    /// </summary>
    private static string? ExtractFromAttributionLink(string? uParam)
    {
        if (string.IsNullOrEmpty(uParam))
        {
            return null;
        }

        var decoded = HttpUtility.UrlDecode(uParam);

        // Parse the embedded URL fragment
        if (Uri.TryCreate("https://youtube.com" + decoded, UriKind.Absolute, out var uri))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        return null;
    }

    /// <summary>
    /// Extracts channel ID from URL.
    /// </summary>
    private static string? ExtractChannelIdFromUrl(string input)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var match = ChannelIdPathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts handle from URL.
    /// </summary>
    private static string? ExtractHandleFromUrl(string input)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var match = HandlePathRegex().Match(uri.AbsolutePath);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts playlist ID from URL.
    /// </summary>
    private static string? ExtractPlaylistIdFromUrl(string input)
    {
        var normalizedUrl = NormalizeUrl(input);

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        return query["list"];
    }

    // Regex patterns

    /// <summary>
    /// Matches channel handle format (@username).
    /// </summary>
    [GeneratedRegex(@"^@[a-zA-Z0-9_.-]{1,100}$")]
    private static partial Regex HandleRegex();

    /// <summary>
    /// Matches channel ID in path (/channel/UC...).
    /// </summary>
    [GeneratedRegex(@"^/channel/(UC[a-zA-Z0-9_-]+)")]
    private static partial Regex ChannelIdPathRegex();

    /// <summary>
    /// Matches handle in path (/@username).
    /// </summary>
    [GeneratedRegex(@"^/(@[a-zA-Z0-9_.-]+)")]
    private static partial Regex HandlePathRegex();
}
