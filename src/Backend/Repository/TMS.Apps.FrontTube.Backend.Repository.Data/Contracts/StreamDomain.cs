using TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

/// <summary>
/// Represents a media stream (video, audio, or combined) for a video.
/// </summary>
public sealed class StreamDomain : ICacheableDomain
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public required RemoteIdentityDomain RemoteIdentity { get; set; }

    /// <summary>
    /// Reference to the video this stream belongs to.
    /// </summary>
    public int VideoId { get; set; }

    /// <summary>
    /// Stream type ID.
    /// </summary>
    public int StreamTypeId { get; set; }

    /// <summary>
    /// Container format ID.
    /// </summary>
    public int ContainerId { get; set; }

    /// <summary>
    /// Video codec ID (null for audio-only streams).
    /// </summary>
    public int? VideoCodecId { get; set; }

    /// <summary>
    /// Audio codec ID (null for video-only streams).
    /// </summary>
    public int? AudioCodecId { get; set; }

    /// <summary>
    /// Audio quality level ID (null for video-only streams).
    /// </summary>
    public int? AudioQualityId { get; set; }

    /// <summary>
    /// Projection type ID.
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
    public VideoDomain? Video { get; set; }

    public string? FetchingError { get; set; }
}
