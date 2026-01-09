namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Video search result item.
/// </summary>
public sealed record SearchResultVideoCommon : SearchResultItemCommon
{
    /// <summary>
    /// The video metadata.
    /// </summary>
    public required VideoMetadataCommon Video { get; init; }

    /// <summary>
    /// Whether the video is in 4K resolution.
    /// </summary>
    public bool Is4K { get; init; }

    /// <summary>
    /// Whether the video is 8K resolution.
    /// </summary>
    public bool Is8K { get; init; }

    /// <summary>
    /// Whether the video is VR 180.
    /// </summary>
    public bool IsVr180 { get; init; }

    /// <summary>
    /// Whether the video is VR 360.
    /// </summary>
    public bool IsVr360 { get; init; }

    /// <summary>
    /// Whether the video is 3D.
    /// </summary>
    public bool Is3D { get; init; }

    /// <summary>
    /// Whether the video has HDR.
    /// </summary>
    public bool IsHdr { get; init; }
}
