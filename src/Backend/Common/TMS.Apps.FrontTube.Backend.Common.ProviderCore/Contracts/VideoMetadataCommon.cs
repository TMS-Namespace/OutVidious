namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a compact video summary for lists and grids.
/// Contains only essential information for display in thumbnails.
/// </summary>
public sealed record VideoMetadataCommon : VideoBaseCommon
{
    /// <summary>
    /// Whether this is a short-form video.
    /// </summary>
    public bool IsShort { get; init; }
}
