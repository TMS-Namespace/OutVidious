using CoreEnums = TMS.Apps.FrontTube.Backend.Core.Enums;
using DomainEnums = TMS.Apps.FrontTube.Backend.Repository.Data.Enums;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

/// <summary>
/// Extension methods for ChannelTab enum conversions in the Core layer.
/// </summary>
public static class ChannelTabExtensions
{
    /// <summary>
    /// Converts a channel tab string to its corresponding Core enum value.
    /// </summary>
    public static CoreEnums.ChannelTab ToChannelTabEnum(this string tabString)
    {
        return tabString.Trim().ToLowerInvariant() switch
        {
            "videos" or "video" => CoreEnums.ChannelTab.Videos,
            "shorts" or "short" => CoreEnums.ChannelTab.Shorts,
            "streams" or "stream" or "live" => CoreEnums.ChannelTab.Streams,
            "playlists" or "playlist" => CoreEnums.ChannelTab.Playlists,
            "community" => CoreEnums.ChannelTab.Community,
            "channels" or "channel" => CoreEnums.ChannelTab.Channels,
            "latest" => CoreEnums.ChannelTab.Latest,
            "podcasts" or "podcast" => CoreEnums.ChannelTab.Podcasts,
            "releases" or "release" => CoreEnums.ChannelTab.Releases,
            _ => CoreEnums.ChannelTab.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a Core ChannelTab enum to its lowercase string representation.
    /// </summary>
    public static string ToLowerString(this CoreEnums.ChannelTab tab)
    {
        return tab switch
        {
            CoreEnums.ChannelTab.Videos => "videos",
            CoreEnums.ChannelTab.Shorts => "shorts",
            CoreEnums.ChannelTab.Streams => "streams",
            CoreEnums.ChannelTab.Playlists => "playlists",
            CoreEnums.ChannelTab.Community => "community",
            CoreEnums.ChannelTab.Channels => "channels",
            CoreEnums.ChannelTab.Latest => "latest",
            CoreEnums.ChannelTab.Podcasts => "podcasts",
            CoreEnums.ChannelTab.Releases => "releases",
            _ => "videos"
        };
    }

    /// <summary>
    /// Converts a Core ChannelTab enum to Domain ChannelTab enum.
    /// </summary>
    public static DomainEnums.ChannelTab ToDomainChannelTab(this CoreEnums.ChannelTab coreTab)
    {
        return coreTab switch
        {
            CoreEnums.ChannelTab.Videos => DomainEnums.ChannelTab.Videos,
            CoreEnums.ChannelTab.Shorts => DomainEnums.ChannelTab.Shorts,
            CoreEnums.ChannelTab.Streams => DomainEnums.ChannelTab.Streams,
            CoreEnums.ChannelTab.Playlists => DomainEnums.ChannelTab.Playlists,
            CoreEnums.ChannelTab.Community => DomainEnums.ChannelTab.Community,
            CoreEnums.ChannelTab.Channels => DomainEnums.ChannelTab.Channels,
            CoreEnums.ChannelTab.Latest => DomainEnums.ChannelTab.Latest,
            CoreEnums.ChannelTab.Podcasts => DomainEnums.ChannelTab.Podcasts,
            CoreEnums.ChannelTab.Releases => DomainEnums.ChannelTab.Releases,
            _ => DomainEnums.ChannelTab.Videos
        };
    }
}
