using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Tools;

/// <summary>
/// Extension methods for converting enum values to/from API strings.
/// </summary>
internal static class EnumConversionExtensions
{
    /// <summary>
    /// Converts a <see cref="ChannelTabType"/> enum to the corresponding API string.
    /// </summary>
    internal static string ToApiString(this ChannelTabType tab)
    {
        return tab switch
        {
            ChannelTabType.Videos => ApiConstants.ChannelTabVideos,
            ChannelTabType.Shorts => ApiConstants.ChannelTabShorts,
            ChannelTabType.Streams => ApiConstants.ChannelTabStreams,
            ChannelTabType.Playlists => ApiConstants.ChannelTabPlaylists,
            ChannelTabType.Community => ApiConstants.ChannelTabCommunity,
            ChannelTabType.Channels => ApiConstants.ChannelTabChannels,
            ChannelTabType.Latest => ApiConstants.ChannelTabLatest,
            ChannelTabType.Podcasts => ApiConstants.ChannelTabPodcasts,
            ChannelTabType.Releases => ApiConstants.ChannelTabReleases,
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, "Unknown channel tab type.")
        };
    }

    /// <summary>
    /// Converts an API string to a <see cref="ChannelTabType"/> enum.
    /// </summary>
    internal static ChannelTabType ToChannelTabEnum(this string tabString)
    {
        return tabString.ToLowerInvariant().Trim() switch
        {
            ApiConstants.ChannelTabVideos or "video" => ChannelTabType.Videos,
            ApiConstants.ChannelTabShorts or "short" => ChannelTabType.Shorts,
            ApiConstants.ChannelTabStreams or "stream" or "live" => ChannelTabType.Streams,
            ApiConstants.ChannelTabPlaylists or "playlist" => ChannelTabType.Playlists,
            ApiConstants.ChannelTabCommunity => ChannelTabType.Community,
            ApiConstants.ChannelTabChannels or "channel" => ChannelTabType.Channels,
            ApiConstants.ChannelTabLatest => ChannelTabType.Latest,
            ApiConstants.ChannelTabPodcasts or "podcast" => ChannelTabType.Podcasts,
            ApiConstants.ChannelTabReleases or "release" => ChannelTabType.Releases,
            _ => ChannelTabType.Videos // Default to videos tab
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
