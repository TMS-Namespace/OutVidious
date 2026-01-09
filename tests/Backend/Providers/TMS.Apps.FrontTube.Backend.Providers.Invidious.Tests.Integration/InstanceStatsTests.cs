using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for instance statistics provider methods.
/// </summary>
public sealed class InstanceStatsTests : IntegrationTestBase
{
    [Fact]
    public async Task GetInstanceStatsAsync_ReturnsStats()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetInstanceStatsAsync(cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();

        var stats = response.Data!;
        // Version might be empty for some instances, just verify we got data
        stats.Should().NotBeNull();
    }
}
