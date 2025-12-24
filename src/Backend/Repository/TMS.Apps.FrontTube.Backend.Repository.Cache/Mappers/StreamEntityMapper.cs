using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

namespace TMS.Apps.FrontTube.Backend.Repository.Cache.Mappers;

/// <summary>
/// Maps between database entities and provider contracts for streams.
/// </summary>
public static class StreamEntityMapper
{
    /// <summary>
    /// Converts a StreamEntity to a StreamInfo contract.
    /// </summary>
    public static Common.ProviderCore.Contracts.Stream ToCommon(StreamEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new Common.ProviderCore.Contracts.Stream
        {
            Type = (StreamType)entity.StreamTypeId,
            RemoteUrl = new Uri(entity.Url),
            Container = (VideoContainer)entity.ContainerId,
            VideoCodec = entity.VideoCodecId.HasValue ? (global::TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums.VideoCodec)entity.VideoCodecId.Value : null,
            AudioCodec = entity.AudioCodecId.HasValue ? (global::TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums.AudioCodec)entity.AudioCodecId.Value : null,
            QualityLabel = entity.QualityLabel,
            Width = entity.Width,
            Height = entity.Height,
            FrameRate = entity.FrameRate,
            Bitrate = entity.Bitrate,
            ContentLength = entity.ContentLength,
            AudioSampleRate = entity.AudioSampleRate,
            AudioChannels = entity.AudioChannels,
            AudioQualityLevel = entity.AudioQualityId.HasValue ? (global::TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums.AudioQuality)entity.AudioQualityId.Value : null,
            Projection = (ProjectionType)entity.ProjectionTypeId,
            MimeType = entity.MimeType,
            Itag = entity.Itag
        };
    }

    /// <summary>
    /// Converts a StreamInfo contract to a StreamEntity for database storage.
    /// </summary>
    public static StreamEntity ToEntity(Common.ProviderCore.Contracts.Stream contract, int videoId)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new StreamEntity
        {
            VideoId = videoId,
            Url = contract.RemoteUrl.ToString(),
            StreamTypeId = (int)contract.Type,
            ContainerId = (int)contract.Container,
            VideoCodecId = contract.VideoCodec.HasValue ? (int)contract.VideoCodec.Value : null,
            AudioCodecId = contract.AudioCodec.HasValue ? (int)contract.AudioCodec.Value : null,
            AudioQualityId = contract.AudioQualityLevel.HasValue ? (int)contract.AudioQualityLevel.Value : null,
            ProjectionTypeId = (int)contract.Projection,
            QualityLabel = contract.QualityLabel,
            Width = contract.Width,
            Height = contract.Height,
            FrameRate = contract.FrameRate,
            Bitrate = contract.Bitrate,
            ContentLength = contract.ContentLength,
            AudioSampleRate = contract.AudioSampleRate,
            AudioChannels = contract.AudioChannels,
            MimeType = contract.MimeType,
            Itag = contract.Itag,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing entity with data from a contract.
    /// </summary>
    public static void UpdateEntity(StreamEntity entity, Common.ProviderCore.Contracts.Stream contract)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(contract);

        entity.Url = contract.RemoteUrl.ToString();
        entity.StreamTypeId = (int)contract.Type;
        entity.ContainerId = (int)contract.Container;
        entity.VideoCodecId = contract.VideoCodec.HasValue ? (int)contract.VideoCodec.Value : null;
        entity.AudioCodecId = contract.AudioCodec.HasValue ? (int)contract.AudioCodec.Value : null;
        entity.AudioQualityId = contract.AudioQualityLevel.HasValue ? (int)contract.AudioQualityLevel.Value : null;
        entity.ProjectionTypeId = (int)contract.Projection;
        entity.QualityLabel = contract.QualityLabel;
        entity.Width = contract.Width;
        entity.Height = contract.Height;
        entity.FrameRate = contract.FrameRate;
        entity.Bitrate = contract.Bitrate;
        entity.ContentLength = contract.ContentLength;
        entity.AudioSampleRate = contract.AudioSampleRate;
        entity.AudioChannels = contract.AudioChannels;
        entity.MimeType = contract.MimeType;
        entity.Itag = contract.Itag;
        entity.LastSyncedAt = DateTime.UtcNow;
    }
}
