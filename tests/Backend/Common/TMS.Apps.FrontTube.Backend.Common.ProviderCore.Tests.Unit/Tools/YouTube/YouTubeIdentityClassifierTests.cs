namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tests.Unit.Tools.YouTube;

/// <summary>
/// Unit tests for YouTubeIdentityClassifier.
/// </summary>
public class YouTubeIdentityClassifierTests
{
    [Theory]
    [InlineData("dQw4w9WgXcQ")]
    [InlineData("-wtIMTCHWuI")]
    [InlineData("0zM3nApSvMg")]
    public void Classify_WithValidVideoId_ReturnsVideoId(string videoId)
    {
        var result = YouTubeIdentityClassifier.Classify(videoId);
        result.Should().Be(YouTubeIdentityType.VideoId);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("youtube.com/watch?v=dQw4w9WgXcQ")]
    public void Classify_WithStandardWatchUrl_ReturnsVideoWatch(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoWatch);
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/dQw4w9WgXcQ")]
    [InlineData("youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=30s")]
    public void Classify_WithShortUrl_ReturnsVideoShortUrl(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoShortUrl);
    }

    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("//www.youtube.com/embed/dQw4w9WgXcQ")]
    public void Classify_WithEmbedUrl_ReturnsVideoEmbed(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoEmbed);
    }

    [Theory]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/v/dQw4w9WgXcQ")]
    public void Classify_WithLegacyVUrl_ReturnsVideoLegacyEmbed(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoLegacyEmbed);
    }

    [Theory]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/shorts/dQw4w9WgXcQ")]
    public void Classify_WithShortsUrl_ReturnsVideoShorts(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoShorts);
    }

    [Theory]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/live/dQw4w9WgXcQ")]
    public void Classify_WithLiveUrl_ReturnsVideoLive(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoLive);
    }

    [Theory]
    [InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
    [InlineData("https://youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
    public void Classify_WithChannelByIdUrl_ReturnsChannelById(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.ChannelById);
    }

    [Theory]
    [InlineData("https://www.youtube.com/@youtube")]
    [InlineData("https://youtube.com/@Google")]
    [InlineData("https://www.youtube.com/@user_name")]
    public void Classify_WithChannelByHandleUrl_ReturnsChannelByHandle(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.ChannelByHandle);
    }

    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b")]
    [InlineData("https://youtube.com/playlist?list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b")]
    public void Classify_WithPlaylistUrl_ReturnsPlaylist(string url)
    {
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.Playlist);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("not a url")]
    public void Classify_WithInvalidInput_ReturnsUnrecognized(string? input)
    {
        var result = YouTubeIdentityClassifier.Classify(input);
        result.Should().Be(YouTubeIdentityType.Unrecognized);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.DomainHostsAndTypes), MemberType = typeof(YouTubeTestDataGenerator))]
    public void GetDomainType_WithVariousHosts_ReturnsCorrectDomain(string host, YouTubeDomain expectedDomain)
    {
        var result = YouTubeIdentityClassifier.GetDomainType(host);
        result.Should().Be(expectedDomain);
    }

    [Theory]
    [MemberData(nameof(YouTubeTestDataGenerator.InvalidDomainHosts), MemberType = typeof(YouTubeTestDataGenerator))]
    public void GetDomainType_WithInvalidInput_ReturnsUnknown(string? input)
    {
        var result = YouTubeIdentityClassifier.GetDomainType(input);
        result.Should().Be(YouTubeDomain.Unknown);
    }

    [Fact]
    public void Classify_WithMobileUrl_ClassifiesCorrectly()
    {
        var url = "https://m.youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoWatch);
    }

    [Fact]
    public void Classify_WithNoCookieEmbedUrl_ClassifiesAsNoCookieEmbed()
    {
        var url = "https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ";
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoNoCookieEmbed);
    }

    [Fact]
    public void Classify_WithUrlEncodedParameters_ClassifiesCorrectly()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=feedrec_grec_index";
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoWatch);
    }

    [Fact]
    public void Classify_WithPlaylistInWatchUrl_IdentifiesAsVideoInPlaylist()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b";
        var result = YouTubeIdentityClassifier.Classify(url);
        result.Should().Be(YouTubeIdentityType.VideoInPlaylist);
    }
}
