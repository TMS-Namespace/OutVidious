using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tests.Integration;

/// <summary>
/// Integration tests for YouTubeRssVideoFetcher.
/// </summary>
public sealed class RssFeedTests : IntegrationTestBase
{
    [Fact]
    public async Task GetChannelVideosAsync_WithValidChannelId_ReturnsVideos()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.GoogleDevelopersChannelId);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        var videos = response.Data!;
        foreach (var video in videos)
        {
            video.RemoteIdentity.Should().NotBeNull();
            video.RemoteIdentity.RemoteId.Should().NotBeNullOrWhiteSpace();
            video.Title.Should().NotBeNullOrWhiteSpace();
            video.PublishedAtUtc.Should().NotBeNull();
            video.PublishedAtUtc.Should().BeBefore(DateTimeOffset.UtcNow);
            video.Channel.Should().NotBeNull();
            video.Channel.Name.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetChannelVideosAsync_WithChannelUrl_ReturnsVideos()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.TestChannelUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChannelVideosAsync_WithChannelIdString_ReturnsVideos()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(
            TestConstants.GoogleDevelopersChannelId,
            cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChannelVideosAsync_WithNonExistentChannelId_ReturnsError()
    {
        // Arrange - Use a valid-looking channel URL that doesn't exist
        // Note: RemoteIdentityCommon validates the URL format, so we use a valid format
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act - Use the string overload with a valid but non-existent channel ID
        // YouTube channel IDs must be 24 characters starting with UC
        var response = await Fetcher.GetChannelVideosAsync(
            "UCzzzzzzzzzzzzzzzzzzzzzz",
            cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task GetChannelVideosAsync_VideosHaveValidThumbnails()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.GoogleDevelopersChannelId);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var videos = response.Data!;

        // At least some videos should have thumbnails
        var videosWithThumbnails = videos.Where(v => v.Thumbnails.Count > 0).ToList();
        videosWithThumbnails.Should().NotBeEmpty("RSS feed should provide thumbnails for videos");

        foreach (var video in videosWithThumbnails)
        {
            foreach (var thumbnail in video.Thumbnails)
            {
                thumbnail.RemoteIdentity.Should().NotBeNull();
                thumbnail.RemoteIdentity.AbsoluteRemoteUrl.Should().NotBeNullOrWhiteSpace();
                thumbnail.Width.Should().BeGreaterThan(0);
                thumbnail.Height.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task GetChannelVideosAsync_VideosHaveCorrectChannelMetadata()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.GoogleDevelopersChannelId);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var videos = response.Data!;

        // All videos should reference the same channel
        var channelRemoteIds = videos.Select(v => v.Channel.RemoteIdentity.RemoteId).Distinct().ToList();
        channelRemoteIds.Should().HaveCount(1, "all videos should be from the same channel");
        channelRemoteIds.First().Should().Be(TestConstants.GoogleDevelopersChannelId);

        // Verify channel name is present
        var channelNames = videos.Select(v => v.Channel.Name).Distinct().ToList();
        channelNames.Should().HaveCount(1, "all videos should have the same channel name");
        channelNames.First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetChannelVideosAsync_ReturnsVideosOrderedByDate()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.GoogleDevelopersChannelId);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var videos = response.Data!;

        // YouTube RSS typically returns videos in reverse chronological order (newest first)
        var publishDates = videos
            .Where(v => v.PublishedAtUtc.HasValue)
            .Select(v => v.PublishedAtUtc!.Value)
            .ToList();

        // Check that dates are generally in descending order (allowing for some tolerance)
        for (var i = 0; i < publishDates.Count - 1; i++)
        {
            publishDates[i].Should().BeOnOrAfter(
                publishDates[i + 1].AddHours(-1),
                "videos should be roughly in reverse chronological order");
        }
    }

    [Fact]
    public void GetChannelVideosAsync_WithInvalidIdentityType_ThrowsArgumentException()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Video,
            TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act & Assert
        var act = async () => await Fetcher.GetChannelVideosAsync(videoIdentity, cts.Token);
        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Channel*");
    }

    [Fact]
    public async Task GetChannelVideosAsync_WithCancellationRequested_ReturnsCancelledResponse()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(
            RemoteIdentityTypeCommon.Channel,
            TestConstants.GoogleDevelopersChannelId);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var response = await Fetcher.GetChannelVideosAsync(channelIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsCancelled.Should().BeTrue();
        response.IsSuccess.Should().BeFalse();
    }
}
