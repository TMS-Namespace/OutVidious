using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Base record for search result items.
/// </summary>
public abstract record SearchResultItemCommon : ICommonContract
{
    /// <summary>
    /// The type of search result.
    /// </summary>
    public required SearchResultType Type { get; init; }
}
