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
    public static DomainEnums.ChannelTabType ToChannelTabEnum(this string tabString)
    {
        return tabString.Trim().ToLowerInvariant() switch
        {
            "videos" or "video" => DomainEnums.ChannelTabType.Videos,
            "shorts" or "short" => DomainEnums.ChannelTabType.Shorts,
            "streams" or "stream" or "live" => DomainEnums.ChannelTabType.Streams,
            "playlists" or "playlist" => DomainEnums.ChannelTabType.Playlists,
            "community" => DomainEnums.ChannelTabType.Community,
            "channels" or "channel" => DomainEnums.ChannelTabType.Channels,
            "latest" => DomainEnums.ChannelTabType.Latest,
            "podcasts" or "podcast" => DomainEnums.ChannelTabType.Podcasts,
            "releases" or "release" => DomainEnums.ChannelTabType.Releases,
            _ => DomainEnums.ChannelTabType.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a domain ChannelTab enum to its lowercase string representation.
    /// </summary>
    public static string ToLowerString(this DomainEnums.ChannelTabType tab)
    {
        return tab switch
        {
            DomainEnums.ChannelTabType.Videos => "videos",
            DomainEnums.ChannelTabType.Shorts => "shorts",
            DomainEnums.ChannelTabType.Streams => "streams",
            DomainEnums.ChannelTabType.Playlists => "playlists",
            DomainEnums.ChannelTabType.Community => "community",
            DomainEnums.ChannelTabType.Channels => "channels",
            DomainEnums.ChannelTabType.Latest => "latest",
            DomainEnums.ChannelTabType.Podcasts => "podcasts",
            DomainEnums.ChannelTabType.Releases => "releases",
            _ => "videos"
        };
    }
}
