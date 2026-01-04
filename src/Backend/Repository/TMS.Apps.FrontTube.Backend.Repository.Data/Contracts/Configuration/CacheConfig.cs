namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

/// <summary>
/// Configuration for the data repository cache and staleness thresholds.
/// </summary>
public sealed record CacheConfig
{
    public StalenessConfig StalenessConfigs { get; init; } = new();

    public SecondLevelCacheConfig SecondLevelCache { get; init; } = new();
}
