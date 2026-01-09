namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Arguments for player mouse events.
/// </summary>
public sealed record PlayerMouseEventArgs
{
    /// <summary>
    /// The X coordinate of the mouse relative to the player surface.
    /// </summary>
    public double ClientX { get; init; }

    /// <summary>
    /// The Y coordinate of the mouse relative to the player surface.
    /// </summary>
    public double ClientY { get; init; }

    /// <summary>
    /// The X coordinate of the mouse relative to the player surface element.
    /// </summary>
    public double OffsetX { get; init; }

    /// <summary>
    /// The Y coordinate of the mouse relative to the player surface element.
    /// </summary>
    public double OffsetY { get; init; }

    /// <summary>
    /// The mouse button that was pressed (0 = left, 1 = middle, 2 = right).
    /// </summary>
    public int Button { get; init; }

    /// <summary>
    /// Whether the Ctrl key was pressed.
    /// </summary>
    public bool CtrlKey { get; init; }

    /// <summary>
    /// Whether the Shift key was pressed.
    /// </summary>
    public bool ShiftKey { get; init; }

    /// <summary>
    /// Whether the Alt key was pressed.
    /// </summary>
    public bool AltKey { get; init; }
}
