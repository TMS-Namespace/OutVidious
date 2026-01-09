namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Comment replies metadata.
/// </summary>
internal sealed record CommentReplies
{
    public int ReplyCount { get; init; }

    public string Continuation { get; init; } = string.Empty;
}
