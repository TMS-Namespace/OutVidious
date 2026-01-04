using Bogus;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tests.Unit.Tools.YouTube;

/// <summary>
/// Generates test data for YouTube URL parsing tests.
/// </summary>
public static class YouTubeTestDataGenerator
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Valid YouTube video IDs (11 characters, base64url-safe).
    /// </summary>
    public static readonly string[] ValidVideoIds =
    [
        "dQw4w9WgXcQ",  // Famous Rick Roll
        "-wtIMTCHWuI",  // Starts with hyphen
        "0zM3nApSvMg",  // Starts with number
        "_____------",  // All special chars
        "AAAAAAAAAAA",  // All same char
        "abcdefghijk",  // All lowercase
        "ABCDEFGHIJK",  // All uppercase
        "0123456789a",  // Mostly numbers
        "a1b2c3d4e5f",  // Mixed
        "ve4f400859I",  // Starts with 'v' (edge case)
    ];

    /// <summary>
    /// Invalid YouTube video IDs.
    /// </summary>
    public static readonly string[] InvalidVideoIds =
    [
        "",             // Empty
        "dQw4w9WgXc",   // 10 chars (too short)
        "dQw4w9WgXcQQ", // 12 chars (too long)
        "dQw4w9WgXc!",  // Invalid char
        "dQw4w9Wg XcQ", // Space
        "dQw4w9WgXc\n", // Newline
    ];

    /// <summary>
    /// Valid YouTube channel IDs (UC + 22 chars).
    /// </summary>
    public static readonly string[] ValidChannelIds =
    [
        "UCuAXFkgsw1L7xaCfnd5JJOw",
        "UC-9-kyTW8ZkZNDHQJ6FgpwQ",
        "UCgc00bfF_PvO_2AvqJZHXFg",
    ];

    /// <summary>
    /// Valid YouTube handles.
    /// </summary>
    public static readonly string[] ValidHandles =
    [
        "@youtube",
        "@Google",
        "@user_name",
        "@user-name",
        "@user.name",
        "@123abc",
    ];

    /// <summary>
    /// Valid YouTube playlist IDs.
    /// </summary>
    public static readonly string[] ValidPlaylistIds =
    [
        "PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b",
        "PLGup6kBfcU7Le5laEaCLgTKtlDcxMqGxZ",
        "RDdQw4w9WgXcQ",  // RD prefix (radio/mix)
        "UUuAXFkgsw1L7xaCfnd5JJOw", // UU prefix (uploads)
    ];

    /// <summary>
    /// Standard watch URLs.
    /// </summary>
    public static IEnumerable<object[]> StandardWatchUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/watch?v=" + videoId, videoId];
            yield return ["http://www.youtube.com/watch?v=" + videoId, videoId];
            yield return ["https://youtube.com/watch?v=" + videoId, videoId];
            yield return ["https://m.youtube.com/watch?v=" + videoId, videoId];
            yield return ["www.youtube.com/watch?v=" + videoId, videoId];
            yield return ["youtube.com/watch?v=" + videoId, videoId];
        }
    }

    /// <summary>
    /// Short URLs (youtu.be).
    /// </summary>
    public static IEnumerable<object[]> ShortUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://youtu.be/" + videoId, videoId];
            yield return ["http://youtu.be/" + videoId, videoId];
            yield return ["youtu.be/" + videoId, videoId];
            yield return ["https://youtu.be/" + videoId + "?t=30s", videoId];
            yield return ["https://youtu.be/" + videoId + "?si=B_RZg_I-lLaa7UU-", videoId];
        }
    }

    /// <summary>
    /// Embed URLs.
    /// </summary>
    public static IEnumerable<object[]> EmbedUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/embed/" + videoId, videoId];
            yield return ["http://www.youtube.com/embed/" + videoId, videoId];
            yield return ["https://youtube.com/embed/" + videoId + "?rel=0", videoId];
            yield return ["//www.youtube.com/embed/" + videoId, videoId];
            yield return ["https://www.youtube-nocookie.com/embed/" + videoId, videoId];
            yield return ["https://www.youtube-nocookie.com/embed/" + videoId + "?rel=0", videoId];
        }
    }

    /// <summary>
    /// Legacy v/ URLs (youtube.com/v/...).
    /// </summary>
    public static IEnumerable<object[]> LegacyVUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/v/" + videoId, videoId];
            yield return ["http://www.youtube.com/v/" + videoId + "?fs=1&hl=en_US", videoId];
        }
    }

    /// <summary>
    /// Legacy e/ URLs (youtube.com/e/... - shorthand for embed).
    /// </summary>
    public static IEnumerable<object[]> LegacyEUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/e/" + videoId, videoId];
            yield return ["http://www.youtube.com/e/" + videoId + "?rel=0", videoId];
        }
    }

    /// <summary>
    /// Legacy e/ URLs for classification testing only.
    /// </summary>
    public static IEnumerable<object[]> LegacyEUrlsForClassification()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/e/" + videoId];
            yield return ["http://www.youtube.com/e/" + videoId + "?rel=0"];
        }
    }

    /// <summary>
    /// Shorts URLs.
    /// </summary>
    public static IEnumerable<object[]> ShortsUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/shorts/" + videoId, videoId];
            yield return ["https://youtube.com/shorts/" + videoId + "?feature=share", videoId];
            yield return ["https://m.youtube.com/shorts/" + videoId, videoId];
        }
    }

    /// <summary>
    /// Live URLs.
    /// </summary>
    public static IEnumerable<object[]> LiveUrls()
    {
        foreach (var videoId in ValidVideoIds.Take(3))
        {
            yield return ["https://www.youtube.com/live/" + videoId, videoId];
            yield return ["https://youtube.com/live/" + videoId + "?feature=share", videoId];
        }
    }

    /// <summary>
    /// URLs with extra parameters.
    /// </summary>
    public static IEnumerable<object[]> UrlsWithExtraParameters()
    {
        var videoId = ValidVideoIds[0];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&feature=feedrec_grec_index", videoId];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&t=30s", videoId];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "#t=0m10s", videoId];
        yield return ["https://www.youtube.com/watch?feature=player_embedded&v=" + videoId, videoId];
        yield return ["https://www.youtube.com/watch?app=desktop&v=" + videoId, videoId];
    }

    /// <summary>
    /// URLs with playlist context.
    /// </summary>
    public static IEnumerable<object[]> UrlsWithPlaylist()
    {
        var videoId = ValidVideoIds[0];
        var playlistId = ValidPlaylistIds[0];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&list=" + playlistId, videoId, playlistId];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&list=" + playlistId + "&index=5", videoId, playlistId, 5];
        yield return ["https://youtu.be/" + videoId + "?list=" + playlistId, videoId, playlistId];
    }

    /// <summary>
    /// Playlist URLs.
    /// </summary>
    public static IEnumerable<object[]> PlaylistUrls()
    {
        foreach (var playlistId in ValidPlaylistIds)
        {
            yield return ["https://www.youtube.com/playlist?list=" + playlistId, playlistId];
            yield return ["https://youtube.com/playlist?list=" + playlistId, playlistId];
        }
    }

    /// <summary>
    /// Channel URLs by ID.
    /// </summary>
    public static IEnumerable<object[]> ChannelByIdUrls()
    {
        foreach (var channelId in ValidChannelIds)
        {
            yield return ["https://www.youtube.com/channel/" + channelId, channelId];
            yield return ["https://youtube.com/channel/" + channelId, channelId];
            yield return ["https://www.youtube.com/channel/" + channelId + "/videos", channelId, YouTubeChannelTab.Videos];
        }
    }

    /// <summary>
    /// Channel URLs by handle.
    /// </summary>
    public static IEnumerable<object[]> ChannelByHandleUrls()
    {
        foreach (var handle in ValidHandles.Take(3))
        {
            yield return ["https://www.youtube.com/" + handle, handle];
            yield return ["https://youtube.com/" + handle, handle];
            yield return ["https://www.youtube.com/" + handle + "/videos", handle, YouTubeChannelTab.Videos];
            yield return ["https://www.youtube.com/" + handle + "/shorts", handle, YouTubeChannelTab.Shorts];
        }
    }

    /// <summary>
    /// Edge case URLs that should still parse correctly.
    /// </summary>
    public static IEnumerable<object[]> EdgeCaseUrls()
    {
        var videoId = ValidVideoIds[0];

        // Missing protocol
        yield return ["youtube.com/watch?v=" + videoId, videoId, YouTubeIdentityType.VideoWatch];
        yield return ["www.youtube.com/watch?v=" + videoId, videoId, YouTubeIdentityType.VideoWatch];

        // Protocol-relative
        yield return ["//www.youtube.com/watch?v=" + videoId, videoId, YouTubeIdentityType.VideoWatch];
        yield return ["//youtu.be/" + videoId, videoId, YouTubeIdentityType.VideoShortUrl];

        // Mobile URLs
        yield return ["https://m.youtube.com/watch?v=" + videoId, videoId, YouTubeIdentityType.VideoWatch];

        // Watch without ?v= but with /VIDEO_ID
        yield return ["https://www.youtube.com/watch/" + videoId, videoId, YouTubeIdentityType.VideoWatch];

        // With trailing content
        yield return ["https://youtu.be/" + videoId + "&feature=channel", videoId, YouTubeIdentityType.VideoShortUrl];

        // Video ID starting with 'v' (edge case for regex)
        yield return ["https://www.youtube.com/watch?v=ve4f400859I", "ve4f400859I", YouTubeIdentityType.VideoWatch];
    }

    /// <summary>
    /// Invalid URLs that should not parse successfully.
    /// </summary>
    public static IEnumerable<object[]> InvalidUrls()
    {
        yield return [""];
        yield return ["   "];
        yield return ["not a url"];
        yield return ["https://google.com"];
        yield return ["https://vimeo.com/123456789"];
        yield return ["https://www.youtube.com/watch"];  // No video ID
        yield return ["https://www.youtube.com/watch?v="];  // Empty video ID
        yield return ["https://www.youtube.com/embed/"];  // Empty video ID
    }

    /// <summary>
    /// oEmbed URLs.
    /// </summary>
    public static IEnumerable<object[]> OEmbedUrls()
    {
        var videoId = ValidVideoIds[0];
        yield return [
            "https://www.youtube.com/oembed?url=http%3A//www.youtube.com/watch?v%3D" + videoId + "&format=json",
            videoId
        ];
    }

    /// <summary>
    /// Attribution link URLs.
    /// </summary>
    public static IEnumerable<object[]> AttributionLinkUrls()
    {
        var videoId = "EhxJLojIE_o";
        yield return [
            "https://www.youtube.com/attribution_link?a=JdfC0C9V6ZI&u=%2Fwatch%3Fv%3D" + videoId + "%26feature%3Dshare",
            videoId
        ];
    }

    /// <summary>
    /// URLs with timestamps.
    /// </summary>
    public static IEnumerable<object[]> UrlsWithTimestamps()
    {
        var videoId = ValidVideoIds[0];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&t=30", videoId, 30];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&t=30s", videoId, 30];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&t=1m30s", videoId, 90];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "&t=1h2m30s", videoId, 3750];
        yield return ["https://www.youtube.com/watch?v=" + videoId + "#t=0m10s", videoId, 10];
        yield return ["https://youtu.be/" + videoId + "?t=1", videoId, 1];
        yield return ["https://youtu.be/" + videoId + "?t=1s", videoId, 1];
    }

    /// <summary>
    /// Generates a random valid video ID.
    /// </summary>
    public static string GenerateRandomVideoId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        return new string(Enumerable.Range(0, 11).Select(_ => Faker.PickRandom(chars.ToCharArray())).ToArray());
    }

    /// <summary>
    /// Generates a random valid channel ID.
    /// </summary>
    public static string GenerateRandomChannelId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        return "UC" + new string(Enumerable.Range(0, 22).Select(_ => Faker.PickRandom(chars.ToCharArray())).ToArray());
    }

    /// <summary>
    /// Valid domain hosts for GetDomainType testing.
    /// </summary>
    public static IEnumerable<object[]> DomainHostsAndTypes()
    {
        yield return ["youtube.com", YouTubeDomain.YouTube];
        yield return ["www.youtube.com", YouTubeDomain.YouTube];
        yield return ["m.youtube.com", YouTubeDomain.YouTubeMobile];
        yield return ["youtu.be", YouTubeDomain.YouTuBe];
        yield return ["youtube-nocookie.com", YouTubeDomain.YouTubeNoCookie];
        yield return ["www.youtube-nocookie.com", YouTubeDomain.YouTubeNoCookie];
        yield return ["music.youtube.com", YouTubeDomain.YouTubeMusic];
        yield return ["youtube.googleapis.com", YouTubeDomain.YouTubeApi];
        yield return ["i.ytimg.com", YouTubeDomain.YouTubeImages];
        yield return ["img.youtube.com", YouTubeDomain.YouTubeImagesAlt];
    }

    /// <summary>
    /// Invalid domain hosts for GetDomainType testing.
    /// </summary>
    public static IEnumerable<object[]> InvalidDomainHosts()
    {
        yield return [""];
        yield return ["   "];
        yield return ["google.com"];
        yield return ["vimeo.com"];
        yield return ["facebook.com"];
    }
}
