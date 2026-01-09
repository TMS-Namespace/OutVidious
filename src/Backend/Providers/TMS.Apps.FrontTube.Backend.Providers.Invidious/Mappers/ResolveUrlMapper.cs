using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious resolve URL DTOs to common contracts.
/// </summary>
internal static class ResolveUrlMapper
{
    /// <summary>
    /// Maps resolve URL DTO to common contract.
    /// </summary>
    public static ResolvedUrlCommon ToResolvedUrl(ResolveUrl dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var resolvedType = dto.Type.ToLowerInvariant() switch
        {
            "video" => ResolvedUrlType.Video,
            "playlist" => ResolvedUrlType.Playlist,
            "channel" => ResolvedUrlType.Channel,
            "clip" => ResolvedUrlType.Clip,
            _ => ResolvedUrlType.Unknown
        };

        return new ResolvedUrlCommon
        {
            Type = resolvedType,
            VideoId = dto.VideoId,
            PlaylistId = dto.PlaylistId,
            ChannelId = dto.ChannelId,
            StartTimeSeconds = dto.StartTimeSeconds
        };
    }
}
