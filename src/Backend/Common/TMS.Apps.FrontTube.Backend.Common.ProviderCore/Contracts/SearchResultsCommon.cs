using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents search results from a provider.
/// </summary>
public sealed record SearchResultsCommon : ICommonContract
{
    /// <summary>
    /// The search query that produced these results.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// List of search result items (videos, channels, playlists, etc.).
    /// </summary>
    public IReadOnlyList<SearchResultItemCommon> Items { get; init; } = [];

    /// <summary>
    /// Continuation token for fetching more results.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// Whether there are more results available.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);

    /// <summary>
    /// Estimated total number of results (if provided by the API).
    /// </summary>
    public long? EstimatedTotalResults { get; init; }
}
