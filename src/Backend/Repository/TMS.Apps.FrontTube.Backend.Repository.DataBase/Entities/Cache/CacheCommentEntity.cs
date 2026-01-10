using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

/// <summary>
/// Represents a single comment.
/// </summary>
public class CacheCommentEntity : EntityBase, ICacheableEntity
{
    public required string AuthorName { get; init; }

    public required string RemoteIdentity { get; set; } 

    public required int VideoId { get; set; }

    public required int AuthorChannelId { get; set; }

    public int? ParentCommentId { get; set; }

    /// <summary>
    /// Whether the author is verified.
    /// </summary>
    public bool IsAuthorVerified { get; set; }

    /// <summary>
    /// Whether the comment is from the video author.
    /// </summary>
    public bool IsVideoAuthor { get; set; }

    /// <summary>
    /// Whether this comment has been hearted by the creator.
    /// </summary>
    public bool IsCreatorHearted { get; set; }

    /// <summary>
    /// Plain text content of the comment.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// HTML formatted content of the comment.
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// Number of likes on the comment.
    /// </summary>
    public int LikeCount { get; set; }

    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Whether this comment is pinned.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Whether this comment has been edited.
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// Number of replies to this comment.
    /// </summary>
    public int ReplyCount { get; set; }

    /// <summary>
    /// Continuation token for fetching replies.
    /// </summary>
    public string? RepliesContinuationToken { get; set; }

    /// <summary>
    /// Whether there are replies to load.
    /// </summary>
    public bool HasReplies { get; set; }

    public long Hash { get; set; }

    public DateTime? LastSyncedAt {get; set; }
}
