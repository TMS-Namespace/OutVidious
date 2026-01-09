using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

/// <summary>
/// Extension methods for converting enum values to/from API strings.
/// </summary>
internal static class EnumConversionExtensions
{
    /// <summary>
    /// Converts a <see cref="ChannelTab"/> enum to the corresponding API string.
    /// </summary>
    internal static string ToApiString(this ChannelTab tab)
    {
        return tab switch
        {
            ChannelTab.Videos => ApiConstants.ChannelTabVideos,
            ChannelTab.Shorts => ApiConstants.ChannelTabShorts,
            ChannelTab.Streams => ApiConstants.ChannelTabStreams,
            ChannelTab.Playlists => ApiConstants.ChannelTabPlaylists,
            ChannelTab.Community => ApiConstants.ChannelTabCommunity,
            ChannelTab.Channels => ApiConstants.ChannelTabChannels,
            ChannelTab.Latest => ApiConstants.ChannelTabLatest,
            ChannelTab.Podcasts => ApiConstants.ChannelTabPodcasts,
            ChannelTab.Releases => ApiConstants.ChannelTabReleases,
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, "Unknown channel tab type.")
        };
    }

    /// <summary>
    /// Converts an API string to a <see cref="ChannelTab"/> enum.
    /// </summary>
    internal static ChannelTab ToChannelTabEnum(this string tabString)
    {
        return tabString.ToLowerInvariant().Trim() switch
        {
            ApiConstants.ChannelTabVideos or "video" => ChannelTab.Videos,
            ApiConstants.ChannelTabShorts or "short" => ChannelTab.Shorts,
            ApiConstants.ChannelTabStreams or "stream" or "live" => ChannelTab.Streams,
            ApiConstants.ChannelTabPlaylists or "playlist" => ChannelTab.Playlists,
            ApiConstants.ChannelTabCommunity => ChannelTab.Community,
            ApiConstants.ChannelTabChannels or "channel" => ChannelTab.Channels,
            ApiConstants.ChannelTabLatest => ChannelTab.Latest,
            ApiConstants.ChannelTabPodcasts or "podcast" => ChannelTab.Podcasts,
            ApiConstants.ChannelTabReleases or "release" => ChannelTab.Releases,
            _ => ChannelTab.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a <see cref="CommentSortType"/> enum to the corresponding API string.
    /// </summary>
    internal static string ToApiString(this CommentSortType sortType)
    {
        return sortType switch
        {
            CommentSortType.Top => ApiConstants.CommentSortTop,
            CommentSortType.New => ApiConstants.CommentSortNew,
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "Unknown comment sort type.")
        };
    }

    /// <summary>
    /// Converts a <see cref="SearchSortType"/> enum to the corresponding API string.
    /// </summary>
    internal static string ToApiString(this SearchSortType sortType)
    {
        return sortType switch
        {
            SearchSortType.Relevance => ApiConstants.SearchSortRelevance,
            SearchSortType.Rating => ApiConstants.SearchSortRating,
            SearchSortType.Date => ApiConstants.SearchSortDate,
            SearchSortType.Views => ApiConstants.SearchSortViews,
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "Unknown search sort type.")
        };
    }

    /// <summary>
    /// Converts a <see cref="SearchType"/> enum to the corresponding API string.
    /// </summary>
    internal static string ToApiString(this SearchType searchType)
    {
        return searchType switch
        {
            SearchType.All => "all",
            SearchType.Video => "video",
            SearchType.Channel => "channel",
            SearchType.Playlist => "playlist",
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "Unknown search type.")
        };
    }

    /// <summary>
    /// Converts a <see cref="RegionCode"/> enum to the corresponding ISO 3166-1 alpha-2 country code string.
    /// </summary>
    internal static string ToApiString(this RegionCode region)
    {
        return region.ToString();
    }
}
