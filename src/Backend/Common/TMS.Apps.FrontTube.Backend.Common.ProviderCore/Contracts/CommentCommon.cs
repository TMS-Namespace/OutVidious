using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a single comment.
/// </summary>
public sealed record CommentCommon : ICacheableCommon
{
    /// <summary>
    /// Comment ID.
    /// </summary>
    [Obsolete("Use RemoteIdentity instead.")]
    public required string RemoteCommentId { get; init; }

    /// <summary>
    /// Author of the comment.
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Author's channel ID.
    /// </summary>
    [Obsolete("Use AuthorChannel instead.")]
    public required string AuthorId { get; init; }

    /// <summary>
    /// Author's channel URL.
    /// </summary>
    [Obsolete("Use AuthorChannel instead.")]
    public string? AuthorUrl { get; init; }

    public required RemoteIdentityCommon RemoteIdentity { get; init; }

    public required ChannelMetadataCommon AuthorChannel { get; init; }

    /// <summary>
    /// Author's thumbnail images.
    /// </summary>
    [Obsolete("Use AuthorChannel instead.")]
    public IReadOnlyList<ImageMetadataCommon> Avatars { get; init; } = [];

    /// <summary>
    /// Whether the author is verified.
    /// </summary>
    public bool IsAuthorVerified { get; init; }

    /// <summary>
    /// Whether the comment is from the video author.
    /// </summary>
    public bool IsVideoAuthor { get; init; }

    /// <summary>
    /// Whether this comment has been hearted by the creator.
    /// </summary>
    public bool IsCreatorHearted { get; init; }

    /// <summary>
    /// Plain text content of the comment.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// HTML formatted content of the comment.
    /// </summary>
    public string? ContentHtml { get; init; }

    /// <summary>
    /// Number of likes on the comment.
    /// </summary>
    public int LikeCount { get; init; }

    /// <summary>
    /// Publication timestamp (Unix epoch).
    /// </summary>
    public DateTime PublishedAtUTC { get; init; }

    /// <summary>
    /// Human-readable publication time (e.g., "2 hours ago").
    /// </summary>
    [Obsolete("Use PublishedAtUTC instead.")]
    public string? PublishedText { get; init; }

    /// <summary>
    /// Whether this comment is pinned.
    /// </summary>
    public bool IsPinned { get; init; }

    /// <summary>
    /// Whether this comment has been edited.
    /// </summary>
    public bool IsEdited { get; init; }

    /// <summary>
    /// Number of replies to this comment.
    /// </summary>
    public int ReplyCount { get; init; }

    /// <summary>
    /// Continuation token for fetching replies.
    /// </summary>
    public string? RepliesContinuationToken { get; init; }

    /// <summary>
    /// Whether there are replies to load.
    /// </summary>
    public bool HasReplies => ReplyCount > 0 || !string.IsNullOrEmpty(RepliesContinuationToken);

    public bool IsMetaData => false;
}
