namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Comment from the Invidious API.
/// </summary>
internal sealed record Comment
{
    public string Author { get; init; } = string.Empty;

    public IReadOnlyList<AuthorThumbnail> AuthorThumbnails { get; init; } = [];

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public bool IsEdited { get; init; }

    public bool IsPinned { get; init; }

    public bool? IsSponsor { get; init; }

    public string? SponsorIconUrl { get; init; }

    public string Content { get; init; } = string.Empty;

    public string ContentHtml { get; init; } = string.Empty;

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public int LikeCount { get; init; }

    public string CommentId { get; init; } = string.Empty;

    public bool AuthorIsChannelOwner { get; init; }

    public CreatorHeart? CreatorHeart { get; init; }

    public CommentReplies? Replies { get; init; }
}
