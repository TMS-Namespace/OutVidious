namespace TMS.Apps.Web.OutVidious.Core.Enums;

/// <summary>
/// Represents the loading state of a video.
/// </summary>
public enum VideoLoadState
{
    /// <summary>
    /// Video is not loaded.
    /// </summary>
    NotLoaded,

    /// <summary>
    /// Video is currently loading.
    /// </summary>
    Loading,

    /// <summary>
    /// Video loaded successfully.
    /// </summary>
    Loaded,

    /// <summary>
    /// Video failed to load.
    /// </summary>
    Error
}
