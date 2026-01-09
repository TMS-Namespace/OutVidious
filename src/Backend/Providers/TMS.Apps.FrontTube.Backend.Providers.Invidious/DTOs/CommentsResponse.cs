namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Comments response from the Invidious API.
/// </summary>
internal sealed record CommentsResponse
{
    public int? CommentCount { get; init; }

    public string VideoId { get; init; } = string.Empty;

    public IReadOnlyList<Comment> Comments { get; init; } = [];

    public string? Continuation { get; init; }
}
