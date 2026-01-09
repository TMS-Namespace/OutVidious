using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Providers.Invidious.DTOs;

namespace TMS.Apps.FrontTube.Backend.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious playlist DTOs to common contracts.
/// </summary>
internal static class PlaylistMapper
{
    /// <summary>
    /// Maps a playlist DTO to a PlaylistCommon.
    /// </summary>
    public static PlaylistCommon ToPlaylist(Playlist dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new PlaylistCommon
        {
            PlaylistId = dto.PlaylistId,
            Title = dto.Title,
            Description = dto.Description,
            DescriptionHtml = dto.DescriptionHtml,
            Author = dto.Author,
            AuthorId = dto.AuthorId,
            AuthorThumbnails = dto.AuthorThumbnails.Select(ThumbnailMapper.ToChannelThumbnailInfo).ToList(),
            VideoCount = dto.VideoCount,
            ViewCount = dto.ViewCount,
            UpdatedAt = dto.Updated,
            IsMix = false,
            Videos = dto.Videos.Select(v => ToPlaylistVideo(v, baseUrl)).ToList()
        };
    }

    /// <summary>
    /// Maps a mix DTO to a PlaylistCommon.
    /// </summary>
    public static PlaylistCommon ToPlaylistFromMix(Mix dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new PlaylistCommon
        {
            PlaylistId = dto.MixId,
            Title = dto.Title,
            Description = string.Empty,
            Author = string.Empty,
            AuthorId = string.Empty,
            IsMix = true,
            Videos = dto.Videos.Select(v => ToPlaylistVideo(v, baseUrl)).ToList()
        };
    }

    /// <summary>
    /// Maps a playlist video DTO to a PlaylistVideoCommon.
    /// </summary>
    public static PlaylistVideoCommon ToPlaylistVideo(PlaylistVideo dto, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(baseUrl);

        return new PlaylistVideoCommon
        {
            Title = dto.Title,
            VideoId = dto.VideoId,
            AuthorName = dto.Author,
            AuthorId = dto.AuthorId,
            LengthSeconds = dto.LengthSeconds,
            Index = dto.Index
        };
    }
}
