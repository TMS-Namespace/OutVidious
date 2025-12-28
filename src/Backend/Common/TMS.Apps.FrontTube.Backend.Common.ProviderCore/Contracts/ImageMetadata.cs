using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a thumbnail image with strongly typed dimensions and quality.
/// </summary>
public sealed record ImageMetadata : ICacheableCommon
{
    /// <summary>
    /// Quality level of the thumbnail.
    /// </summary>
    public required ImageQuality Quality { get; init; }

    /// <summary>
    /// Absolute URL to the original image source (e.g., https://i.ytimg.com/...).
    /// Used as the unique identifier for hashing and lookups.
    /// </summary>
    public required Uri AbsoluteRemoteUrl { get; init; }

    /// <summary>
    /// Width of the thumbnail in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Height of the thumbnail in pixels.
    /// </summary>
    public required int Height { get; init; }

    private long? _hash;
    public long Hash => _hash ??= HashHelper.ComputeHash(AbsoluteRemoteUrl.ToString());

    public bool IsMetaData => true;
}
