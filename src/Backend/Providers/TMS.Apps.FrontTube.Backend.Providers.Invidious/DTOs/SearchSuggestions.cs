namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

/// <summary>
/// Search suggestions response from the Invidious API.
/// </summary>
internal sealed record SearchSuggestions
{
    public string Query { get; init; } = string.Empty;

    public IReadOnlyList<string> Suggestions { get; init; } = [];
}
