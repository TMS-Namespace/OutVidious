using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Mappers;

/// <summary>
/// Maps YouTube RSS feed DTOs to common provider contracts.
/// </summary>
internal static class RssFeedMapper
{
    /// <summary>
    /// Maps an RSS feed video DTO to a VideoMetadataCommon contract.
    /// </summary>
    /// <param name="dto">The RSS feed video DTO.</param>
    /// <param name="channelMetadata">The channel metadata for the video author.</param>
    /// <returns>The mapped VideoMetadataCommon contract.</returns>
    public static VideoMetadataCommon ToVideoMetadata(RssFeedVideo dto, ChannelMetadataCommon channelMetadata)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(channelMetadata);

        var thumbnails = new List<ImageMetadataCommon>();

        if (dto.Thumbnail is not null)
        {
            thumbnails.Add(ToThumbnailInfo(dto.Thumbnail));
        }

        // Use the full video URL for the remote identity
        // This ensures proper parsing by YouTubeIdentityParser
        return new VideoMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Video,
                dto.VideoUrl),
            Title = dto.Title,
            Duration = TimeSpan.Zero, // RSS feed does not provide duration
            ViewCount = dto.ViewCount ?? 0,
            PublishedAtUtc = dto.PublishedAtUtc,
            Channel = channelMetadata,
            Thumbnails = thumbnails,
            IsLive = false, // RSS feed does not provide live status
            IsUpcoming = false, // RSS feed does not provide upcoming status
            IsShort = false // RSS feed does not provide short status
        };
    }

    /// <summary>
    /// Maps an RSS feed channel DTO to a ChannelMetadataCommon contract.
    /// </summary>
    /// <param name="dto">The RSS feed channel DTO.</param>
    /// <returns>The mapped ChannelMetadataCommon contract.</returns>
    public static ChannelMetadataCommon ToChannelMetadata(RssFeedChannel dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Use the channel URL as the remote identity source
        // The yt:channelId element in RSS doesn't include the "UC" prefix
        // but the channel URL does (e.g., https://www.youtube.com/channel/UC_x5XG1OV2P6uZZ5FSM9Ttw)
        return new ChannelMetadataCommon
        {
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Channel,
                dto.ChannelUrl),
            Name = dto.ChannelName,
            SubscriberCount = null, // RSS feed does not provide subscriber count
            Avatars = [] // RSS feed does not provide channel avatars
        };
    }

    /// <summary>
    /// Maps an RSS feed thumbnail DTO to an ImageMetadataCommon contract.
    /// </summary>
    /// <param name="dto">The RSS feed thumbnail DTO.</param>
    /// <returns>The mapped ImageMetadataCommon contract.</returns>
    public static ImageMetadataCommon ToThumbnailInfo(RssFeedThumbnail dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var quality = DetermineQuality(dto.Width, dto.Height);

        return new ImageMetadataCommon
        {
            Quality = quality,
            RemoteIdentity = new RemoteIdentityCommon(
                RemoteIdentityTypeCommon.Image,
                dto.Url),
            Width = dto.Width,
            Height = dto.Height
        };
    }

    /// <summary>
    /// Determines the image quality based on dimensions.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The determined image quality.</returns>
    private static ImageQuality DetermineQuality(int width, int height)
    {
        // Standard YouTube thumbnail sizes:
        // - default: 120x90
        // - medium (mqdefault): 320x180
        // - high (hqdefault): 480x360
        // - standard (sddefault): 640x480
        // - maxres (maxresdefault): 1280x720

        return width switch
        {
            >= 1280 => ImageQuality.MaxRes,
            >= 640 => ImageQuality.Standard,
            >= 480 => ImageQuality.High,
            >= 320 => ImageQuality.Medium,
            _ => ImageQuality.Default
        };
    }
}
