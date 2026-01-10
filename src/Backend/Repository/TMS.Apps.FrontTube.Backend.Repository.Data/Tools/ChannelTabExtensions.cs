using DomainEnums = TMS.Apps.FrontTube.Backend.Repository.Data.Enums;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Tools;

/// <summary>
/// Extension methods for ChannelTab enum conversions in the domain layer.
/// </summary>
internal static class ChannelTabExtensions
{
    /// <summary>
    /// Converts a channel tab string to its corresponding domain enum value.
    /// </summary>
    public static DomainEnums.ChannelTab ToChannelTabEnum(this string tabString)
    {
        return tabString.Trim().ToLowerInvariant() switch
        {
            "videos" or "video" => DomainEnums.ChannelTab.Videos,
            "shorts" or "short" => DomainEnums.ChannelTab.Shorts,
            "streams" or "stream" or "live" => DomainEnums.ChannelTab.Streams,
            "playlists" or "playlist" => DomainEnums.ChannelTab.Playlists,
            "community" => DomainEnums.ChannelTab.Community,
            "channels" or "channel" => DomainEnums.ChannelTab.Channels,
            "latest" => DomainEnums.ChannelTab.Latest,
            "podcasts" or "podcast" => DomainEnums.ChannelTab.Podcasts,
            "releases" or "release" => DomainEnums.ChannelTab.Releases,
            _ => DomainEnums.ChannelTab.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a domain ChannelTab enum to its lowercase string representation.
    /// </summary>
    public static string ToLowerString(this DomainEnums.ChannelTab tab)
    {
        return tab switch
        {
            DomainEnums.ChannelTab.Videos => "videos",
            DomainEnums.ChannelTab.Shorts => "shorts",
            DomainEnums.ChannelTab.Streams => "streams",
            DomainEnums.ChannelTab.Playlists => "playlists",
            DomainEnums.ChannelTab.Community => "community",
            DomainEnums.ChannelTab.Channels => "channels",
            DomainEnums.ChannelTab.Latest => "latest",
            DomainEnums.ChannelTab.Podcasts => "podcasts",
            DomainEnums.ChannelTab.Releases => "releases",
            _ => "videos"
        };
    }
}
