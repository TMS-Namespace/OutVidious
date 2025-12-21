namespace TMS.Apps.Web.OutVidious.Core.Models;

/// <summary>
/// Represents a video thumbnail from the Invidious API.
/// </summary>
public sealed record VideoThumbnail
{
    public string Quality { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
