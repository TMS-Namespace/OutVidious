using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents trending/popular videos list.
/// </summary>
public sealed record TrendingVideosCommon : ICommonContract
{
    /// <summary>
    /// The trending category/type.
    /// </summary>
    public TrendingCategory Category { get; init; } = TrendingCategory.Default;

    /// <summary>
    /// The region code (e.g., "US", "GB").
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// List of trending videos.
    /// </summary>
    public IReadOnlyList<VideoMetadataCommon> Videos { get; init; } = [];
}
