namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Configuration;

/// <summary>
/// Configuration for the data repository cache and staleness thresholds.
/// </summary>
public sealed record CacheConfig
{




    public StalenessConfig StalenessConfigs {get; init;} = new StalenessConfig();

    public SecondLevelCacheConfig SecondLevelCache {get; init;} = new();

}
