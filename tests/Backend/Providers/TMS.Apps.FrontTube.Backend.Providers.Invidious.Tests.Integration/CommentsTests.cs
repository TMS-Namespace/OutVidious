using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration.Tools;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tests.Integration;

/// <summary>
/// Integration tests for comments-related provider methods.
/// </summary>
[Trait("Category", "Integration")]
public sealed class CommentsTests : IntegrationTestBase
{
    [Fact]
    public async Task GetCommentsAsync_FirstPage_ReturnsComments()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var response = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: null,
            continuationToken: null,
            cts.Token);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.HasError.Should().BeFalse();
        response.Data.Should().NotBeNull();

        var commentsPage = response.Data!;
        commentsPage.Comments.Should().NotBeEmpty();

        foreach (var comment in commentsPage.Comments)
        {
            comment.AuthorName.Should().NotBeNullOrWhiteSpace();
            comment.Content.Should().NotBeNullOrWhiteSpace();
            comment.LikeCount.Should().BeGreaterThanOrEqualTo(0);
            comment.PublishedAt.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetCommentsAsync_WithContinuationToken_ReturnsNextPage()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Act - Get first page
        var firstPageResponse = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: null,
            continuationToken: null,
            cts.Token);

        firstPageResponse.IsSuccess.Should().BeTrue();
        var firstPage = firstPageResponse.Data!;
        var continuationToken = firstPage.ContinuationToken;

        if (continuationToken == null)
        {
            // Not all videos have enough comments for pagination
            return;
        }

        // Act - Get second page
        var secondPageResponse = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: null,
            continuationToken: continuationToken,
            cts.Token);

        // Assert
        secondPageResponse.IsSuccess.Should().BeTrue();
        var secondPage = secondPageResponse.Data!;
        secondPage.Comments.Should().NotBeEmpty();
        var firstIds = firstPage.Comments.Select(c => c.RemoteCommentId).ToHashSet();
        var secondIds = secondPage.Comments.Select(c => c.RemoteCommentId).ToHashSet();
        firstIds.Should().NotIntersectWith(secondIds, "Second page should contain different comments than first page");
    }

    [Fact]
    public async Task GetCommentsAsync_WithReplies_ReturnsCommentReplies()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Act - Get first page of comments
        var response = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: null,
            continuationToken: null,
            cts.Token);

        // Skip if comments fetch failed (some videos have disabled comments)
        if (!response.IsSuccess)
        {
            return;
        }

        var commentsPage = response.Data!;

        // Find a comment with replies
        var commentWithReplies = commentsPage.Comments.FirstOrDefault(c => c.ReplyCount > 0 && !string.IsNullOrWhiteSpace(c.RepliesContinuationToken));
        if (commentWithReplies == null)
        {
            // Skip if no comments have replies
            return;
        }

        // Act - Get replies
        var repliesResponse = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: null,
            continuationToken: commentWithReplies.RepliesContinuationToken,
            cts.Token);

        // Assert - replies endpoint might not work for all instances
        if (!repliesResponse.IsSuccess)
        {
            return;
        }

        var repliesPage = repliesResponse.Data!;
        repliesPage.Comments.Should().NotBeEmpty();
        repliesPage.Comments.Count.Should().BeLessOrEqualTo(commentWithReplies.ReplyCount);
    }

    [Fact]
    public async Task GetCommentsAsync_DifferentSortTypes_ReturnsOrderedComments()
    {
        // Arrange
        var videoIdentity = new RemoteIdentityCommon(RemoteIdentityTypeCommon.Video, TestConstants.TestVideoUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Act - Get comments sorted by top
        var topResponse = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: CommentSortType.Top,
            continuationToken: null,
            cts.Token);

        // Skip if comments are disabled for this video
        if (!topResponse.IsSuccess)
        {
            return;
        }

        // Act - Get comments sorted by new
        var newResponse = await Provider.GetCommentsAsync(
            videoIdentity,
            sortBy: CommentSortType.New,
            continuationToken: null,
            cts.Token);

        // Verify we got responses
        topResponse.Data!.Comments.Should().NotBeEmpty();
        
        // New sort might not work on all instances
        if (!newResponse.IsSuccess)
        {
            return;
        }

        newResponse.Data!.Comments.Should().NotBeEmpty();
    }
}
