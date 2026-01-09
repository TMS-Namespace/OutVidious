using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for video-related provider methods.
/// </summary>
public sealed class VideoTests : IntegrationTestBase
{

    [Fact]
    public async Task GetVideoAsync_WithValidVideoId_ReturnsVideoDetails()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetVideoAsync(videoIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();

        var video = response.Data!;
        video.RemoteIdentity.Should().NotBeNull();
        video.RemoteIdentity.RemoteId.Should().NotBeNullOrWhiteSpace();
        video.Title.Should().NotBeNullOrWhiteSpace();
        video.DescriptionText.Should().NotBeNull();
        video.Channel.Name.Should().NotBeNullOrWhiteSpace();
        video.ViewCount.Should().BeGreaterThanOrEqualTo(0);
        video.LikeCount.Should().BeGreaterThanOrEqualTo(0);
        video.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        video.PublishedAtUtc.Should().BeBefore(DateTimeOffset.UtcNow);
        video.Thumbnails.Should().NotBeEmpty();
        video.AdaptiveStreams.Should().NotBeEmpty();
        video.MutexStreams.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetVideoAsync_WithInvalidVideoId_ReturnsError()
    {
        // Arrange - Use a valid-looking video ID format that doesn't exist
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, "https://www.youtube.com/watch?v=XXXXXXXXXXX");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetVideoAsync(videoIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.HasError.Should().BeTrue();
    }

    [Fact]
    public void GetEmbedVideoPlayerUri_WithValidVideoId_ReturnsValidUri()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);

        // Act
        var embedUri = Provider.GetEmbedVideoPlayerUri(videoIdentity);

        // Assert
        embedUri.Should().NotBeNull();
        embedUri.Scheme.Should().Be("https");
        embedUri.AbsoluteUri.Should().Contain("embed");
    }
}
