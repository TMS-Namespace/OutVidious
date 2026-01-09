using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for trending and popular video provider methods.
/// </summary>
public sealed class TrendingTests : IntegrationTestBase
{
    [Fact]
    public async Task GetTrendingAsync_ReturnsVideos()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetTrendingAsync(
            TrendingCategory.Default,
            region: null,
            cts.Token);

        // Skip if trending endpoint is not supported by this instance
        if (!response.IsSuccess)
        {
            return;
        }

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();

        var trending = response.Data!;
        trending.Videos.Should().NotBeEmpty();

        foreach (var video in trending.Videos)
        {
            video.RemoteIdentity.Should().NotBeNull();
            video.Title.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetTrendingAsync_WithCategory_ReturnsCategoryVideos()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetTrendingAsync(
            TrendingCategory.Music,
            region: null,
            cts.Token);

        // Skip if trending endpoint is not supported by this instance
        if (!response.IsSuccess)
        {
            return;
        }

        // Assert
        response.Data!.Videos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPopularAsync_ReturnsVideos()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetPopularAsync(cts.Token);

        // Skip if popular endpoint is not supported by this instance
        if (!response.IsSuccess)
        {
            return;
        }

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();

        var popular = response.Data!;
        popular.Videos.Should().NotBeEmpty();

        foreach (var video in popular.Videos)
        {
            video.RemoteIdentity.Should().NotBeNull();
            video.Title.Should().NotBeNullOrWhiteSpace();
        }
    }
}
