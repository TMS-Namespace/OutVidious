using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents search suggestions/autocomplete results.
/// </summary>
public sealed record SearchSuggestionsCommon : ICommonContract
{
    /// <summary>
    /// The original query.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// List of suggested search terms.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];
}
