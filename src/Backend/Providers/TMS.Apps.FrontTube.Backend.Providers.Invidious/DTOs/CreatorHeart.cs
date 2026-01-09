namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Creator heart on a comment.
/// </summary>
internal sealed record CreatorHeart
{
    public string CreatorThumbnail { get; init; } = string.Empty;

    public string CreatorName { get; init; } = string.Empty;
}
