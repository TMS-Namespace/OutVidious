using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
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
    public static ChannelTabType ToChannelTabEnum(this string tabString)
    {
        return tabString.ToLowerInvariant() switch
        {
            "videos" or "video" => ChannelTabType.Videos,
            "shorts" or "short" => ChannelTabType.Shorts,
            "streams" or "stream" or "live" => ChannelTabType.Streams,
            "playlists" or "playlist" => ChannelTabType.Playlists,
            "community" => ChannelTabType.Community,
            "channels" or "channel" => ChannelTabType.Channels,
            "latest" => ChannelTabType.Latest,
            "podcasts" or "podcast" => ChannelTabType.Podcasts,
            "releases" or "release" => ChannelTabType.Releases,
            _ => ChannelTabType.Videos // Default to videos tab
        };
    }

    /// <summary>
    /// Converts a ChannelTab enum to its lowercase string representation.
    /// </summary>
    /// <param name="tab">The ChannelTab enum value.</param>
    /// <returns>The lowercase string representation.</returns>
    public static string ToLowerString(this ChannelTabType tab)
    {
        return tab switch
        {
            ChannelTabType.Videos => "videos",
            ChannelTabType.Shorts => "shorts",
            ChannelTabType.Streams => "streams",
            ChannelTabType.Playlists => "playlists",
            ChannelTabType.Community => "community",
            ChannelTabType.Channels => "channels",
            ChannelTabType.Latest => "latest",
            ChannelTabType.Podcasts => "podcasts",
            ChannelTabType.Releases => "releases",
            _ => "videos"
        };
    }
}
