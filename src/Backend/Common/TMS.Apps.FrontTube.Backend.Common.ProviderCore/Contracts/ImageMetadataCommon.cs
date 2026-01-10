using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a thumbnail image with strongly typed dimensions and quality.
/// </summary>
public sealed record ImageMetadataCommon : ICacheableCommon
{
    // /// <summary>
    // /// Quality level of the thumbnail.
    // /// </summary>
    // public required ImageQuality Quality { get; init; }

    public required RemoteIdentityCommon RemoteIdentity { get; init; }

    /// <summary>
    /// Width of the thumbnail in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Height of the thumbnail in pixels.
    /// </summary>
    public required int Height { get; init; }

    public bool IsMetaData => true;
}
