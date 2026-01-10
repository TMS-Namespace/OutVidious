using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Enums;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

/// <summary>
/// Represents a media stream (video, audio, or combined) for a video.
/// </summary>
public class CacheStreamEntity : EntityBase, ICacheableEntity
{
    public DateTime CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// XxHash64 hash of the absolute remote URL for unique lookup.
    /// </summary>
    public required long Hash { get; set; }

    /// <summary>
    /// Reference to the video this stream belongs to.
    /// </summary>
    public int VideoId { get; set; }

    /// <summary>
    /// Absolute URL to the stream.
    /// </summary>
    public required string RemoteIdentity { get; set; }

    /// <summary>
    /// Stream type ID (references enum_stream_type).
    /// </summary>
    public int StreamTypeId { get; set; }

    /// <summary>
    /// Container format ID (references enum_video_container).
    /// </summary>
    public int ContainerId { get; set; }

    /// <summary>
    /// Video codec ID (references enum_video_codec). Null for audio-only streams.
    /// </summary>
    public int? VideoCodecId { get; set; }

    /// <summary>
    /// Audio codec ID (references enum_audio_codec). Null for video-only streams.
    /// </summary>
    public int? AudioCodecId { get; set; }

    /// <summary>
    /// Audio quality level ID (references enum_audio_quality). Null for video-only streams.
    /// </summary>
    public int? AudioQualityId { get; set; }

    /// <summary>
    /// Projection type ID (references enum_projection_type).
    /// </summary>
    public int ProjectionTypeId { get; set; }

    /// <summary>
    /// Quality label (e.g., "1080p", "720p60").
    /// </summary>
    public string? QualityLabel { get; set; }

    /// <summary>
    /// Video width in pixels. Null for audio-only streams.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Video height in pixels. Null for audio-only streams.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Frame rate in frames per second. Null for audio-only streams.
    /// </summary>
    public int? FrameRate { get; set; }

    /// <summary>
    /// Bitrate in bits per second.
    /// </summary>
    public long? Bitrate { get; set; }

    /// <summary>
    /// Content length in bytes.
    /// </summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// Audio sample rate in Hz. Null for video-only streams.
    /// </summary>
    public int? AudioSampleRate { get; set; }

    /// <summary>
    /// Number of audio channels. Null for video-only streams.
    /// </summary>
    public int? AudioChannels { get; set; }

    /// <summary>
    /// MIME type string (e.g., "video/mp4; codecs=\"avc1.640028\"").
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Internal tag identifier from the provider.
    /// </summary>
    public int? Itag { get; set; }

    // Navigation properties
    public CacheVideoEntity Video { get; set; } = null!;

    public EnumStreamTypeEntity StreamType { get; set; } = null!;

    public EnumVideoContainerEntity Container { get; set; } = null!;

    public EnumVideoCodecEntity? VideoCodec { get; set; }

    public EnumAudioCodecEntity? AudioCodec { get; set; }

    public EnumProjectionTypeEntity ProjectionType { get; set; } = null!;
}
