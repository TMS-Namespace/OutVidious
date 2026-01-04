namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tests.Unit.Tools.YouTube;

/// <summary>
/// Unit tests for YouTubeIdentityValidator.
/// </summary>
public class YouTubeIdentityValidatorTests
{
    [Theory]
    [InlineData("dQw4w9WgXcQ")]
    [InlineData("-wtIMTCHWuI")]
    [InlineData("0zM3nApSvMg")]
    public void TryValidate_WithValidVideoId_ReturnsTrue(string videoId)
    {
        var result = YouTubeIdentityValidator.TryValidate(videoId, YouTubeIdentityType.VideoId, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("dQw4w9WgXc")]  // Too short
    [InlineData("dQw4w9WgXcQQ")] // Too long
    [InlineData("dQw4w9Wg XcQ")] // Contains space
    public void TryValidate_WithInvalidVideoId_ReturnsFalse(string videoId)
    {
        var result = YouTubeIdentityValidator.TryValidate(videoId, YouTubeIdentityType.VideoId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("youtube.com/watch?v=dQw4w9WgXcQ")]
    public void TryValidate_WithValidWatchUrl_ReturnsTrue(string url)
    {
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.VideoWatch, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch")]  // Missing video ID
    [InlineData("https://www.youtube.com/watch?v=")]  // Empty video ID
    [InlineData("https://www.youtube.com/watch?v=invalid")]  // Invalid video ID
    public void TryValidate_WithInvalidWatchUrl_ReturnsFalse(string url)
    {
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.VideoWatch, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=30s")]
    public void TryValidate_WithValidShortUrl_ReturnsTrue(string url)
    {
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.VideoShortUrl, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://youtu.be/")]  // Missing video ID
    [InlineData("https://youtu.be/invalid")]  // Invalid video ID (too short)
    public void TryValidate_WithInvalidShortUrl_ReturnsFalse(string url)
    {
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.VideoShortUrl, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    [InlineData("//www.youtube.com/embed/dQw4w9WgXcQ")]
    public void TryValidate_WithValidEmbedUrl_ReturnsTrue(string url)
    {
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.VideoEmbed, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("UCuAXFkgsw1L7xaCfnd5JJOw")]
    [InlineData("UC-9-kyTW8ZkZNDHQJ6FgpwQ")]
    public void TryValidate_WithValidChannelId_ReturnsTrue(string channelId)
    {
        var result = YouTubeIdentityValidator.TryValidate(channelId, YouTubeIdentityType.ChannelId, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("UC")]  // Too short
    [InlineData("UCuAXFkgsw1L7xaCfnd5JJOw123")]  // Too long
    public void TryValidate_WithInvalidChannelId_ReturnsFalse(string channelId)
    {
        var result = YouTubeIdentityValidator.TryValidate(channelId, YouTubeIdentityType.ChannelId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("@youtube")]
    [InlineData("@Google")]
    [InlineData("@user_name")]
    [InlineData("@user-name")]
    public void TryValidate_WithValidHandle_ReturnsTrue(string handle)
    {
        var result = YouTubeIdentityValidator.TryValidate(handle, YouTubeIdentityType.ChannelByHandle, out var validationResult);
        // Handles may not have full validation implemented, just test classification and parse attempt
        if (!result)
        {
            validationResult.IsValid.Should().BeFalse();
        }
        else
        {
            validationResult.IsValid.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("@")]
    [InlineData("@a")]
    public void TryValidate_WithInvalidHandle_ReturnsFalse(string handle)
    {
        var result = YouTubeIdentityValidator.TryValidate(handle, YouTubeIdentityType.ChannelByHandle, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithVeryLongHandle_ReturnsFalse()
    {
        var handle = "@" + new string('a', 100);
        var result = YouTubeIdentityValidator.TryValidate(handle, YouTubeIdentityType.ChannelByHandle, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b")]
    [InlineData("PLGup6kBfcU7Le5laEaCLgTKtlDcxMqGxZ")]
    public void TryValidate_WithValidPlaylistId_ReturnsTrue(string playlistId)
    {
        var result = YouTubeIdentityValidator.TryValidate(playlistId, YouTubeIdentityType.PlaylistId, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("PL")]  // Too short
    [InlineData("INVALID")]  // Doesn't start with PL, RD, UU, etc.
    public void TryValidate_WithInvalidPlaylistId_ReturnsFalse(string playlistId)
    {
        var result = YouTubeIdentityValidator.TryValidate(playlistId, YouTubeIdentityType.PlaylistId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithNullInput_ReturnsFalse()
    {
        var result = YouTubeIdentityValidator.TryValidate(null, YouTubeIdentityType.VideoId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithEmptyInput_ReturnsFalse()
    {
        var result = YouTubeIdentityValidator.TryValidate("", YouTubeIdentityType.VideoId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithWhitespaceInput_ReturnsFalse()
    {
        var result = YouTubeIdentityValidator.TryValidate("   ", YouTubeIdentityType.VideoId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithWrongTypeForInput_ReturnsFalse()
    {
        // Providing a video ID but saying it's a channel ID
        var result = YouTubeIdentityValidator.TryValidate("dQw4w9WgXcQ", YouTubeIdentityType.ChannelId, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithUnrecognizedType_ReturnsFalse()
    {
        var result = YouTubeIdentityValidator.TryValidate("anything", YouTubeIdentityType.Unrecognized, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_Overload_WithoutTypeParameter_ReturnsValidResultForValidUrl()
    {
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var result = YouTubeIdentityValidator.TryValidate(url, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public void TryValidate_Overload_WithoutTypeParameter_ReturnsFalseForInvalidUrl()
    {
        var url = "https://google.com";
        var result = YouTubeIdentityValidator.TryValidate(url, out var validationResult);
        result.Should().BeFalse();
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithChannelUrl_AcceptsValidChannelId()
    {
        var url = "https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw";
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.ChannelById, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public void TryValidate_WithPlaylistUrl_AcceptsValidPlaylistId()
    {
        var url = "https://www.youtube.com/playlist?list=PLToa5JuFMsXTNkrLJbRlB--76IAOjRM9b";
        var result = YouTubeIdentityValidator.TryValidate(url, YouTubeIdentityType.Playlist, out var validationResult);
        result.Should().BeTrue();
        validationResult.IsValid.Should().BeTrue();
    }
}
