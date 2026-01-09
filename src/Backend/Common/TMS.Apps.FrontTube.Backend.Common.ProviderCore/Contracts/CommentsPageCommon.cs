using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a page of comments for a video.
/// </summary>
public sealed record CommentsPageCommon : ICommonContract
{
    /// <summary>
    /// The video ID these comments belong to.
    /// </summary>
    public required string VideoId { get; init; }

    /// <summary>
    /// List of comments in this page.
    /// </summary>
    public IReadOnlyList<CommentCommon> Comments { get; init; } = [];

    /// <summary>
    /// Continuation token for fetching the next page.
    /// Null if this is the last page.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// Whether there are more comments to load.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);

    /// <summary>
    /// Total number of comments (if known).
    /// </summary>
    public int? TotalCommentCount { get; init; }

    /// <summary>
    /// The source of the comments (e.g., "youtube", "reddit").
    /// </summary>
    public string Source { get; init; } = "youtube";
}
