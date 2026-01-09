namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Community post from the Invidious API (channel posts).
/// </summary>
internal sealed record CommunityPost
{
    public string Type { get; init; } = string.Empty;

    public string PostId { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorUrl { get; init; } = string.Empty;

    public IReadOnlyList<VideoThumbnail> AuthorThumbnails { get; init; } = [];

    public string Content { get; init; } = string.Empty;

    public string ContentHtml { get; init; } = string.Empty;

    public long Published { get; init; }

    public string PublishedText { get; init; } = string.Empty;

    public int LikeCount { get; init; }

    public int CommentCount { get; init; }

    public CommunityPostAttachment? Attachment { get; init; }
}
