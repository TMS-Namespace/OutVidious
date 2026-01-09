namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

/// <summary>
/// Represents the different sorting options for search results.
/// </summary>
public enum SearchSortType
{
    /// <summary>
    /// Sort by relevance (default).
    /// </summary>
    Relevance,

    /// <summary>
    /// Sort by rating (highest rated first).
    /// </summary>
    Rating,

    /// <summary>
    /// Sort by upload date (newest first).
    /// </summary>
    Date,

    /// <summary>
    /// Sort by view count (most viewed first).
    /// </summary>
    Views
}
