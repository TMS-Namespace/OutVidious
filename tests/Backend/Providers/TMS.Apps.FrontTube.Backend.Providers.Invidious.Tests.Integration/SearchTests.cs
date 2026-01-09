using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for search-related provider methods.
/// </summary>
public sealed class SearchTests : IntegrationTestBase
{
    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsResults()
    {
        // Arrange
        const string query = "test";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.SearchAsync(
            query,
            page: 1,
            sortBy: null,
            type: null,
            cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();

        var results = response.Data!;
        results.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithTypeFilter_ReturnsFilteredResults()
    {
        // Arrange
        const string query = "test";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.SearchAsync(
            query,
            page: 1,
            sortBy: null,
            type: SearchType.Video,
            cts.Token);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var results = response.Data!;
        results.Items.Should().NotBeEmpty();

        // All results should be videos
        results.Items.Should().AllSatisfy(r => r.Type.Should().Be(SearchResultType.Video));
    }

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsMultiplePages()
    {
        // Arrange
        const string query = "test";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Act - Get first page
        var page1Response = await Provider.SearchAsync(
            query,
            page: 1,
            sortBy: null,
            type: null,
            cts.Token);

        // Act - Get second page
        var page2Response = await Provider.SearchAsync(
            query,
            page: 2,
            sortBy: null,
            type: null,
            cts.Token);

        // Assert
        page1Response.IsSuccess.Should().BeTrue();
        page2Response.IsSuccess.Should().BeTrue();

        var page1Results = page1Response.Data!.Items;
        var page2Results = page2Response.Data!.Items;

        page1Results.Should().NotBeEmpty();
        page2Results.Should().NotBeEmpty();

        // Pagination should work - second page should have results
        // Note: Some overlap might occur due to API behavior, just verify we got results
        var page1Ids = page1Results.Select(GetResultId).Where(id => id is not null).ToHashSet();
        var page2Ids = page2Results.Select(GetResultId).Where(id => id is not null).ToHashSet();
        
        // At least some results should be different between pages
        page1Ids.Union(page2Ids).Count().Should().BeGreaterThan(page1Ids.Count, "Second page should provide some new results");
    }

    [Fact]
    public async Task GetSearchSuggestionsAsync_WithPartialQuery_ReturnsSuggestions()
    {
        // Arrange
        const string query = "test";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetSearchSuggestionsAsync(query, cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();

        var suggestions = response.Data!;
        suggestions.Query.Should().Be(query);
        suggestions.Suggestions.Should().NotBeEmpty();
    }

    private static string? GetResultId(SearchResultItemCommon result)
    {
        return result switch
        {
            SearchResultVideoCommon video => video.Video.RemoteIdentity.RemoteId,
            SearchResultChannelCommon channel => channel.Channel.RemoteIdentity.RemoteId,
            SearchResultPlaylistCommon playlist => playlist.PlaylistId,
            _ => null
        };
    }
}
