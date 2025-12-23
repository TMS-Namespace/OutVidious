using System.Text.RegularExpressions;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Enums;
using TMS.Apps.Web.OutVidious.Providers.Invidious.ApiModels;

namespace TMS.Apps.Web.OutVidious.Providers.Invidious.Mappers;

/// <summary>
/// Maps Invidious stream DTOs to common StreamInfo contracts.
/// </summary>
public static partial class StreamMapper
{
    /// <summary>
    /// Maps an Invidious adaptive format DTO to a StreamInfo contract.
    /// </summary>
    public static StreamInfo ToStreamInfo(InvidiousAdaptiveFormatDto dto)
    {
        var streamType = DetermineStreamType(dto.Type);
        var (width, height) = ParseResolution(dto.Resolution);

        return new StreamInfo
        {
            Type = streamType,
            Url = new Uri(dto.Url, UriKind.RelativeOrAbsolute),
            Container = ParseContainer(dto.Container),
            VideoCodec = streamType != StreamType.Audio ? ParseVideoCodec(dto.Encoding) : null,
            AudioCodec = streamType != StreamType.Video ? ParseAudioCodec(dto.Encoding) : null,
            QualityLabel = dto.QualityLabel,
            Width = width,
            Height = height,
            FrameRate = dto.Fps,
            Bitrate = TryParseLong(dto.Bitrate),
            ContentLength = TryParseLong(dto.Clen),
            AudioSampleRate = TryParseInt(dto.AudioSampleRate),
            AudioChannels = TryParseInt(dto.AudioChannels),
            AudioQualityLevel = ParseAudioQuality(dto.AudioQuality),
            Projection = ParseProjectionType(dto.ProjectionType),
            MimeType = dto.Type,
            Itag = TryParseInt(dto.Itag)
        };
    }

    /// <summary>
    /// Maps an Invidious format stream DTO to a StreamInfo contract.
    /// </summary>
    public static StreamInfo ToStreamInfo(InvidiousFormatStreamDto dto)
    {
        var (width, height) = ParseResolution(dto.Resolution);

        return new StreamInfo
        {
            Type = StreamType.VideoAndAudio,
            Url = new Uri(dto.Url, UriKind.RelativeOrAbsolute),
            Container = ParseContainer(dto.Container),
            VideoCodec = ParseVideoCodec(dto.Encoding),
            AudioCodec = AudioCodec.Aac, // Combined formats typically use AAC
            QualityLabel = dto.QualityLabel,
            Width = width,
            Height = height,
            Bitrate = TryParseLong(dto.Bitrate),
            MimeType = dto.Type,
            Itag = TryParseInt(dto.Itag)
        };
    }

    private static StreamType DetermineStreamType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return StreamType.Unknown;
        }

        var lowerType = mimeType.ToLowerInvariant();
        
        if (lowerType.StartsWith("video/"))
        {
            return StreamType.Video;
        }
        
        if (lowerType.StartsWith("audio/"))
        {
            return StreamType.Audio;
        }

        return StreamType.Unknown;
    }

    private static VideoContainer ParseContainer(string? container)
    {
        if (string.IsNullOrWhiteSpace(container))
        {
            return VideoContainer.Unknown;
        }

        return container.ToLowerInvariant() switch
        {
            "mp4" => VideoContainer.Mp4,
            "webm" => VideoContainer.WebM,
            "mkv" => VideoContainer.Mkv,
            "m4a" => VideoContainer.M4a,
            "opus" => VideoContainer.Opus,
            _ => VideoContainer.Unknown
        };
    }

    private static VideoCodec? ParseVideoCodec(string? encoding)
    {
        if (string.IsNullOrWhiteSpace(encoding))
        {
            return null;
        }

        var lowerEncoding = encoding.ToLowerInvariant();
        
        if (lowerEncoding.Contains("avc1") || lowerEncoding.Contains("h264") || lowerEncoding.Contains("h.264"))
        {
            return VideoCodec.H264;
        }
        
        if (lowerEncoding.Contains("hvc1") || lowerEncoding.Contains("h265") || lowerEncoding.Contains("h.265") || lowerEncoding.Contains("hevc"))
        {
            return VideoCodec.H265;
        }
        
        if (lowerEncoding.Contains("vp9"))
        {
            return VideoCodec.Vp9;
        }
        
        if (lowerEncoding.Contains("vp8"))
        {
            return VideoCodec.Vp8;
        }
        
        if (lowerEncoding.Contains("av01") || lowerEncoding.Contains("av1"))
        {
            return VideoCodec.Av1;
        }

        return null;
    }

    private static AudioCodec? ParseAudioCodec(string? encoding)
    {
        if (string.IsNullOrWhiteSpace(encoding))
        {
            return null;
        }

        var lowerEncoding = encoding.ToLowerInvariant();
        
        if (lowerEncoding.Contains("mp4a") || lowerEncoding.Contains("aac"))
        {
            return AudioCodec.Aac;
        }
        
        if (lowerEncoding.Contains("opus"))
        {
            return AudioCodec.Opus;
        }
        
        if (lowerEncoding.Contains("vorbis"))
        {
            return AudioCodec.Vorbis;
        }
        
        if (lowerEncoding.Contains("mp3"))
        {
            return AudioCodec.Mp3;
        }

        return null;
    }

    private static AudioQuality? ParseAudioQuality(string? quality)
    {
        if (string.IsNullOrWhiteSpace(quality))
        {
            return null;
        }

        return quality.ToLowerInvariant() switch
        {
            "audio_quality_low" or "low" => AudioQuality.Low,
            "audio_quality_medium" or "medium" => AudioQuality.Medium,
            "audio_quality_high" or "high" => AudioQuality.High,
            _ => AudioQuality.Unknown
        };
    }

    private static ProjectionType ParseProjectionType(string? projectionType)
    {
        if (string.IsNullOrWhiteSpace(projectionType))
        {
            return ProjectionType.Rectangular;
        }

        return projectionType.ToLowerInvariant() switch
        {
            "rectangular" => ProjectionType.Rectangular,
            "360" or "spherical" => ProjectionType.Spherical360,
            "mesh" => ProjectionType.Mesh,
            _ => ProjectionType.Unknown
        };
    }

    private static (int? Width, int? Height) ParseResolution(string? resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution))
        {
            return (null, null);
        }

        var match = ResolutionRegex().Match(resolution);
        if (match.Success 
            && int.TryParse(match.Groups[1].Value, out var width) 
            && int.TryParse(match.Groups[2].Value, out var height))
        {
            return (width, height);
        }

        return (null, null);
    }

    private static long? TryParseLong(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return long.TryParse(value, out var result) ? result : null;
    }

    private static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, out var result) ? result : null;
    }

    [GeneratedRegex(@"(\d+)x(\d+)")]
    private static partial Regex ResolutionRegex();
}
