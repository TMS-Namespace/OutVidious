namespace TMS.Apps.FTube.Backend.DataRepository;

/// <summary>
/// Wrapper for cached items that tracks when they were last synced.
/// </summary>
/// <typeparam name="T">The type of data being cached.</typeparam>
public sealed record CachedItem<T>
{
    /// <summary>
    /// Creates a new cached item.
    /// </summary>
    /// <param name="data">The data to cache.</param>
    /// <param name="lastSyncedAt">When the data was last synced from the source.</param>
    public CachedItem(T data, DateTime lastSyncedAt)
    {
        Data = data;
        LastSyncedAt = lastSyncedAt;
    }

    /// <summary>
    /// The cached data.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// When the data was last synced from the source.
    /// </summary>
    public DateTime LastSyncedAt { get; }
}
