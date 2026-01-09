namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Storyboard information from the Invidious API.
/// </summary>
internal sealed record Storyboard
{
    public string Url { get; init; } = string.Empty;

    public string TemplateUrl { get; init; } = string.Empty;

    public int Width { get; init; }

    public int Height { get; init; }

    public int Count { get; init; }

    public int Interval { get; init; }

    public int StoryboardWidth { get; init; }

    public int StoryboardHeight { get; init; }

    public int StoryboardCount { get; init; }
}
