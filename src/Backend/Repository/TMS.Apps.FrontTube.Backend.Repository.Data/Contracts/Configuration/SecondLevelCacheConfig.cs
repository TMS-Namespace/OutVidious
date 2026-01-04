namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

public sealed record SecondLevelCacheConfig
{
    public bool Enable { get; init; } = true;

    public string CacheKeyPrefix { get; init; } = "FT_";

    public bool EnableLogging { get; init; } = false;

    public bool CacheAllQueries { get; init; } = true;

    public int CacheAllQueriesExpirationMode { get; init; } = 1;

    public TimeSpan CacheAllQueriesTimeout { get; init; } = TimeSpan.FromMinutes(5);

    public bool UseDbCallsIfCachingProviderIsDown { get; init; } = true;

    public bool EnableSensitiveDataLogging { get; init; } = false;

    public bool EnableDetailedErrors { get; init; } = true;

    public TimeSpan DbCallsIfCachingProviderIsDownTimeout { get; init; } = TimeSpan.FromMinutes(1);
}
