using TMS.Apps.FrontTube.Backend.Common.DataEnums.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a media stream (video, audio, or combined).
/// </summary>
public sealed record StreamMetadataCommon : ICacheableCommon
{
    /// <summary>
    /// Type of stream (video only, audio only, or combined).
    /// </summary>
    public required StreamType Type { get; init; }

    public required RemoteIdentityCommon RemoteIdentity { get; init; }

    /// <summary>
    /// Container format.
    /// </summary>
    public required VideoContainer Container { get; init; }

    /// <summary>
    /// Video codec (null for audio-only streams).
    /// </summary>
    public VideoCodec? VideoCodec { get; init; }

    /// <summary>
    /// Audio codec (null for video-only streams).
    /// </summary>
    public AudioCodecType? AudioCodec { get; init; }

    /// <summary>
    /// Quality label (e.g., "1080p", "720p60").
    /// </summary>
    public string? QualityLabel { get; init; }

    /// <summary>
    /// Video width in pixels (null for audio-only streams).
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Video height in pixels (null for audio-only streams).
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Frame rate in frames per second (null for audio-only streams).
    /// </summary>
    public int? FrameRate { get; init; }

    /// <summary>
    /// Bitrate in bits per second.
    /// </summary>
    public long? Bitrate { get; init; }

    /// <summary>
    /// Content length in bytes.
    /// </summary>
    public long? ContentLength { get; init; }

    /// <summary>
    /// Audio sample rate in Hz (null for video-only streams).
    /// </summary>
    public int? AudioSampleRate { get; init; }

    /// <summary>
    /// Number of audio channels (null for video-only streams).
    /// </summary>
    public int? AudioChannels { get; init; }

    // /// <summary>
    // /// Audio quality level (null for video-only streams).
    // /// </summary>
    // public AudioQuality? AudioQualityLevel { get; init; }

    /// <summary>
    /// Projection type for VR/360 videos.
    /// </summary>
    public ProjectionType Projection { get; init; } = ProjectionType.Rectangular;

    /// <summary>
    /// MIME type string (e.g., "video/mp4; codecs=\"avc1.640028\"").
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Internal tag identifier from the provider.
    /// </summary>
    public int? Itag { get; init; }

    public bool IsMetaData => true;
}
