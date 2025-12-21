namespace TMS.Apps.Web.OutVidious.Core.Models;

/// <summary>
/// Represents an author/channel thumbnail from the Invidious API.
/// </summary>
public sealed record AuthorThumbnail
{
    public string Url { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }
}
