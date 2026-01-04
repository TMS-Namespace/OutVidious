using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;

/// <summary>
/// Extension methods for YouTube identity types.
/// </summary>
public static class YouTubeExtensions
{
    /// <summary>
    /// Determines whether the YouTube identity is supported for operations.
    /// </summary>
    /// <param name="parts">The YouTube identity parts to check.</param>
    /// <returns>True if the identity type is supported, false otherwise.</returns>
    public static bool IsSupported(this YouTubeIdentityParts parts)
    {
        return parts.IdentityType switch
        {
            YouTubeIdentityType.VideoId or
            YouTubeIdentityType.VideoWatch or
            YouTubeIdentityType.VideoShortUrl or
            YouTubeIdentityType.VideoEmbed or
            YouTubeIdentityType.VideoLegacyEmbed or
            YouTubeIdentityType.VideoShorts or
            YouTubeIdentityType.VideoLive => true,

            YouTubeIdentityType.ChannelById or
            YouTubeIdentityType.ChannelId or
            YouTubeIdentityType.ChannelTabById => true,

            _ => false
        };
    }
}