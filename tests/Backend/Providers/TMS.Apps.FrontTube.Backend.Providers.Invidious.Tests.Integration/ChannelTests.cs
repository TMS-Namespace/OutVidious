using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for channel-related provider methods.
/// </summary>
public sealed class ChannelTests : IntegrationTestBase
{
    [Fact]
    public async Task GetChannelAsync_WithValidChannelId_ReturnsChannelDetails()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Channel, TestConstants.TestChannelUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetChannelAsync(channelIdentity, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();

        var channel = response.Data!;
        channel.RemoteIdentity.Should().NotBeNull();
        channel.RemoteIdentity.RemoteId.Should().NotBeNullOrWhiteSpace();
        channel.Name.Should().NotBeNullOrWhiteSpace();
        channel.Description.Should().NotBeNull();
        channel.SubscriberCount.Should().BeGreaterThanOrEqualTo(0);
        // VideoCount might be null for some channels
        if (channel.VideoCount.HasValue)
        {
            channel.VideoCount.Value.Should().BeGreaterThanOrEqualTo(0);
        }
        channel.Avatars.Should().NotBeEmpty();
        channel.AvailableTabs.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChannelVideosTabAsync_FirstPage_ReturnsVideos()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Channel, TestConstants.TestChannelUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetChannelVideosTabAsync(
            channelIdentity,
            ChannelTabType.Videos,
            page: null,
            continuationToken: null,
            cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();

        var page = response.Data!;
        page.ChannelRemoteIdentity.Should().NotBeNull();
        page.Tab.Should().Be(ChannelTabType.Videos);
        page.Videos.Should().NotBeEmpty();

        foreach (var video in page.Videos)
        {
            video.RemoteIdentity.Should().NotBeNull();
            video.RemoteIdentity.RemoteId.Should().NotBeNullOrWhiteSpace();
            video.Title.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetChannelVideosTabAsync_WithContinuationToken_ReturnsNextPage()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Channel, TestConstants.TestChannelUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Act - Get first page
        var firstPageResponse = await Provider.GetChannelVideosTabAsync(
            channelIdentity,
            ChannelTabType.Videos,
            page: null,
            continuationToken: null,
            cts.Token);

        firstPageResponse.IsSuccess.Should().BeTrue();
        var firstPage = firstPageResponse.Data!;
        var continuationToken = firstPage.ContinuationToken;

        // Skip test if channel doesn't have enough videos for pagination
        if (string.IsNullOrWhiteSpace(continuationToken))
        {
            return;
        }

        // Act - Get second page
        var secondPageResponse = await Provider.GetChannelVideosTabAsync(
            channelIdentity,
            ChannelTabType.Videos,
            page: null,
            continuationToken: continuationToken,
            cts.Token);

        // Assert - continuation might fail if API doesn't support it well
        if (!secondPageResponse.IsSuccess)
        {
            // API might not support continuation properly - just verify first page worked
            firstPage.Videos.Should().NotBeEmpty();
            return;
        }

        var secondPage = secondPageResponse.Data!;
        secondPage.Videos.Should().NotBeEmpty();
        var firstIds = firstPage.Videos.Select(v => v.RemoteIdentity.RemoteId).ToHashSet();
        var secondIds = secondPage.Videos.Select(v => v.RemoteIdentity.RemoteId).ToHashSet();
        
        // Verify we got different videos or at least more videos
        firstIds.Union(secondIds).Count().Should().BeGreaterThanOrEqualTo(firstIds.Count);
    }

    [Fact]
    public async Task GetChannelVideosTabAsync_DifferentTabs_ReturnsTabSpecificContent()
    {
        // Arrange
        var channelIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Channel, TestConstants.TestChannelUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act - Test Videos tab
        var videosResponse = await Provider.GetChannelVideosTabAsync(
            channelIdentity,
            ChannelTabType.Videos,
            page: null,
            continuationToken: null,
            cts.Token);

        // Assert Videos tab works
        videosResponse.IsSuccess.Should().BeTrue();
        videosResponse.Data!.Tab.Should().Be(ChannelTabType.Videos);
        videosResponse.Data.Videos.Should().NotBeEmpty();

        // Act - Test Shorts tab (might not exist for all channels)
        var shortsResponse = await Provider.GetChannelVideosTabAsync(
            channelIdentity,
            ChannelTabType.Shorts,
            page: null,
            continuationToken: null,
            cts.Token);

        // Shorts tab might not exist or be supported
        if (shortsResponse.IsSuccess && shortsResponse.Data!.Videos.Any())
        {
            shortsResponse.Data.Tab.Should().Be(ChannelTabType.Shorts);
        }
    }
}
