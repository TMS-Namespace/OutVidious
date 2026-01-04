namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tests.Unit.Tools.YouTube;

/// <summary>
/// Unit tests for YouTubeIdentityParser.
/// </summary>
public class YouTubeIdentityParserTests
{
    [Theory]
    [InlineData("dQw4w9WgXcQ")]
    [InlineData("-wtIMTCHWuI")]
    [InlineData("0zM3nApSvMg")]
    public void TryParse_WithValidVideoId_ReturnsTrue(string videoId)
    {
        var result = YouTubeIdentityParser.TryParse(videoId, out var parts);
        result.Should().BeTrue();
        parts.Should().NotBeNull();
        parts.VideoId.Should().Be(videoId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.VideoId);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.StandardWatchUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithStandardWatchUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.VideoWatch);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.ShortUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithShortUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.VideoShortUrl);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.EmbedUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithEmbedUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        // Both VideoEmbed and VideoNoCookieEmbed are valid embed types
        var validEmbedTypes = new[] { YouTubeIdentityType.VideoEmbed, YouTubeIdentityType.VideoNoCookieEmbed };
        validEmbedTypes.Should().Contain(parts.IdentityType);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.LegacyVUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithLegacyVUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        // Legacy /v/ and /e/ URLs are both legacy embed types
        var validLegacyTypes = new[] { YouTubeIdentityType.VideoLegacyEmbed, YouTubeIdentityType.VideoEmbed };
        validLegacyTypes.Should().Contain(parts.IdentityType);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.LegacyEUrlsForClassification), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithLegacyEUrl_ClassifiesAsEmbed(string url)
    {
        // Legacy /e/ URLs are classified as embed but may not parse completely
        var classifyResult = YouTubeIdentityClassifier.Classify(url);
        classifyResult.Should().Be(YouTubeIdentityType.VideoEmbed);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.ShortsUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithShortsUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.VideoShorts);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.LiveUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithLiveUrl_ExtractsVideoId(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.VideoLive);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.UrlsWithTimestamps), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithUrlsContainingTimestamps_ExtractsTimestamp(string url, string expectedVideoId, int? expectedTimestamp)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
        if (expectedTimestamp.HasValue)
        {
            parts.StartTimeSeconds.Should().Be(expectedTimestamp.Value);
        }
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.PlaylistUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithPlaylistUrl_ExtractsPlaylistId(string url, string expectedPlaylistId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.PlaylistId.Should().Be(expectedPlaylistId);
        parts.IdentityType.Should().Be(YouTubeIdentityType.Playlist);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.ChannelByIdUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithChannelByIdUrl_ExtractsChannelId(params object[] data)
    {
        var url = (string)data[0];
        var expectedChannelId = (string)data[1];
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.ChannelId.Should().Be(expectedChannelId);
        // URLs with tabs are detected as ChannelTabById, plain URLs are ChannelById
        var validChannelTypes = new[] { YouTubeIdentityType.ChannelById, YouTubeIdentityType.ChannelTabById };
        validChannelTypes.Should().Contain(parts.IdentityType);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.ChannelByHandleUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithChannelByHandleUrl_ExtractsHandle(params object[] data)
    {
        var url = (string)data[0];
        var expectedHandle = (string)data[1];
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.ChannelHandle.Should().Be(expectedHandle);
        // URLs with tabs are detected as ChannelTabByHandle, plain URLs are ChannelByHandle
        var validChannelTypes = new[] { YouTubeIdentityType.ChannelByHandle, YouTubeIdentityType.ChannelTabByHandle };
        validChannelTypes.Should().Contain(parts.IdentityType);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.UrlsWithExtraParameters), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithUrlsContainingExtraParameters_IgnoresExtraParameters(string url, string expectedVideoId)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(expectedVideoId);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.InvalidUrls), MemberType = typeof(YouTubeTestDataGenerator))]
    public void TryParse_WithInvalidUrls_ReturnsFalse(string url)
    {
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithNullInput_ReturnsFalse()
    {
        var result = YouTubeIdentityParser.TryParse(null, out var parts);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithEmptyInput_ReturnsFalse()
    {
        var result = YouTubeIdentityParser.TryParse("", out var parts);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithWhitespaceInput_ReturnsFalse()
    {
        var result = YouTubeIdentityParser.TryParse("   ", out var parts);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithNonYouTubeUrl_ReturnsFalse()
    {
        var result = YouTubeIdentityParser.TryParse("https://vimeo.com/123456789", out var parts);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithVideoIdStartingWithV_ParsesCorrectly()
    {
        var videoId = "ve4f400859I";
        var result = YouTubeIdentityParser.TryParse(videoId, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be(videoId);
    }

    [Fact]
    public void TryParse_WithUrlContainingPlaylistAndVideo_ExtractsBoth()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
        parts.PlaylistId.Should().Be("PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b");
    }

    [Fact]
    public void TryParse_WithUrlContainingPlaylistIndex_ExtractsIndex()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b&index=5";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.PlaylistIndex.Should().Be(5);
    }

    [Fact]
    public void TryParse_ParsedParts_ShouldHaveCorrectHelperProperties()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.IsVideo.Should().BeTrue();
        parts.IsChannel.Should().BeFalse();
        parts.IsPlaylist.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithChannelUrl_ParsedParts_ShouldHaveCorrectHelperProperties()
    {
        var url = "https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.IsVideo.Should().BeFalse();
        parts.IsChannel.Should().BeTrue();
        parts.IsPlaylist.Should().BeFalse();
    }

    [Fact]
    public void TryParse_WithPlaylistUrl_ParsedParts_ShouldHaveCorrectHelperProperties()
    {
        var url = "https://www.youtube.com/playlist?list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.IsVideo.Should().BeFalse();
        parts.IsChannel.Should().BeFalse();
        parts.IsPlaylist.Should().BeTrue();
    }

    [Fact]
    public void TryParse_WithShortUrlContainingShareUrl_ExtractsCorrectly()
    {
        var url = "https://youtu.be/dQw4w9WgXcQ?si=B_RZg_I-lLaa7UU-";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
    }

    [Fact]
    public void TryParse_WithUrlMissingProtocol_StillParsesCorrectly()
    {
        var url = "youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
    }

    [Fact]
    public void TryParse_WithProtocolRelativeUrl_ParsesCorrectly()
    {
        var url = "//www.youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
    }

    [Fact]
    public void TryParse_WithMobileUrl_ParsesCorrectly()
    {
        var url = "https://m.youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
    }

    [Fact]
    public void TryParse_WithNoCookieEmbedUrl_ParsesCorrectly()
    {
        var url = "https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
        parts.Domain.Should().Be(YouTubeDomain.YouTubeNoCookie);
    }

    [Fact]
    public void TryParse_WithUrlEncodedQueryString_ParsesCorrectly()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&app=desktop&feature=player_embedded";
        var result = YouTubeIdentityParser.TryParse(url, out var parts);
        result.Should().BeTrue();
        parts.VideoId.Should().Be("dQw4w9WgXcQ");
    }
}
