namespace TMS.Apps.Web.OutVidious.Common.ProvidersCore.Configuration;

/// <summary>
/// Configuration for the data repository cache and staleness thresholds.
/// </summary>
public sealed record DataRepositoryConfig
{
    /// <summary>
    /// Database connection configuration.
    /// </summary>
    public required DataBaseConfig DataBase { get; init; }

    // ─────────────────────────────────────────────────────────────────────────────
    // Staleness Thresholds
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// How long video data is considered fresh before re-fetching from provider.
    /// Default: 3 hours.
    /// </summary>
    public TimeSpan VideoStalenessThreshold { get; init; } = TimeSpan.FromHours(3);

    /// <summary>
    /// How long channel data is considered fresh before re-fetching from provider.
    /// Default: 2 days.
    /// </summary>
    public TimeSpan ChannelStalenessThreshold { get; init; } = TimeSpan.FromDays(2);

    /// <summary>
    /// How long image/thumbnail data is considered fresh before re-fetching from provider.
    /// Default: 5 hours.
    /// </summary>
    public TimeSpan ImageStalenessThreshold { get; init; } = TimeSpan.FromHours(5);

    /// <summary>
    /// How long caption data is considered fresh before re-fetching from provider.
    /// Default: 1 day.
    /// </summary>
    public TimeSpan CaptionStalenessThreshold { get; init; } = TimeSpan.FromDays(1);

    // ─────────────────────────────────────────────────────────────────────────────
    // Memory Cache Settings
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maximum number of videos to keep in memory cache.
    /// Default: 500.
    /// </summary>
    public int VideoMemoryCacheCapacity { get; init; } = 500;

    /// <summary>
    /// Maximum number of channels to keep in memory cache.
    /// Default: 200.
    /// </summary>
    public int ChannelMemoryCacheCapacity { get; init; } = 200;

    /// <summary>
    /// Maximum number of images to keep in memory cache.
    /// Default: 1000.
    /// </summary>
    public int ImageMemoryCacheCapacity { get; init; } = 1000;

    /// <summary>
    /// Time-to-live for memory cache entries.
    /// Entries are evicted from memory after this duration regardless of staleness.
    /// Default: 1 hour.
    /// </summary>
    public TimeSpan MemoryCacheTtl { get; init; } = TimeSpan.FromHours(1);
}
