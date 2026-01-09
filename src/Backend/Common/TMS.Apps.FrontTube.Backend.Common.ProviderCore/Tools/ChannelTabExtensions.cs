using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;

/// <summary>
/// Extension methods for ChannelTab enum conversions.
/// </summary>
public static class ChannelTabExtensions
{
    /// <summary>
    /// Converts a channel tab string to its corresponding enum value.
    /// </summary>
    /// <param name="tabString">The tab string to convert.</param>
    /// <returns>The corresponding ChannelTab enum value, defaults to Videos if unknown.</returns>
    public static ChannelTab ToChannelTabEnum(this string tabString)
    {
        return tabString.ToLowerInvariant() switch
        {
            "videos" or "video" => ChannelTab.Videos,
            "shorts" or "short" => ChannelTab.Shorts,
            "streams" or "stream" or "live" => ChannelTab.Streams,
            "playlists" or "playlist" => ChannelTab.Playlists,
            "community" => ChannelTab.Community,
            "channels" or "channel" => ChannelTab.Channels,
            "latest" => ChannelTab.Latest,
            "podcasts" or "podcast" => ChannelTab.Podcasts,
            "releases" or "release" => ChannelTab.Releases,
            _ => ChannelTab.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a ChannelTab enum to its lowercase string representation.
    /// </summary>
    /// <param name="tab">The ChannelTab enum value.</param>
    /// <returns>The lowercase string representation.</returns>
    public static string ToLowerString(this ChannelTab tab)
    {
        return tab switch
        {
            ChannelTab.Videos => "videos",
            ChannelTab.Shorts => "shorts",
            ChannelTab.Streams => "streams",
            ChannelTab.Playlists => "playlists",
            ChannelTab.Community => "community",
            ChannelTab.Channels => "channels",
            ChannelTab.Latest => "latest",
            ChannelTab.Podcasts => "podcasts",
            ChannelTab.Releases => "releases",
            _ => "videos"
        };
    }
}
