namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Enums;

/// <summary>
/// Types of remote identities that can be referenced.
/// </summary>
public enum RemoteIdentityTypeCommon
{
    /// <summary>
    /// A video resource.
    /// </summary>
    Video,

    /// <summary>
    /// A channel resource.
    /// </summary>
    Channel,

    /// <summary>
    /// An image resource.
    /// </summary>
    Image,

    /// <summary>
    /// A caption/subtitle resource.
    /// </summary>
    Caption,

    /// <summary>
    /// A stream resource.
    /// </summary>
    Stream,

    /// <summary>
    /// A playlist resource.
    /// </summary>
    Playlist,

    /// <summary>
    /// A mix resource (auto-generated playlist).
    /// </summary>
    Mix,

    Comment,
}
