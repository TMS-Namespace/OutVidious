namespace TMS.Apps.FrontTube.Backend.Core.Enums;

/// <summary>
/// Represents the loading state of an image.
/// </summary>
public enum LoadingState
{
    /// <summary>
    /// Image has not started loading.
    /// </summary>
    NotLoaded,

    /// <summary>
    /// Image is currently being loaded.
    /// </summary>
    Loading,

    /// <summary>
    /// Image has been successfully loaded.
    /// </summary>
    Loaded,

    /// <summary>
    /// Image loading failed.
    /// </summary>
    Failed
}