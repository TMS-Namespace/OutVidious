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
    [Obsolete("Use VideoIdentity instead.")]
    public required string VideoId { get; init; }

    public required RemoteIdentityCommon VideoIdentity { get; init; }

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
    /// Total number of comments (if known).
    /// </summary>
    public int? TotalCommentCount { get; init; }

    /// <summary>
    /// The source of the comments (e.g., "youtube", "reddit").
    /// </summary>
    [Obsolete]
    public string Source { get; init; } = "youtube";
}
