namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Arguments for player scroll/wheel events.
/// </summary>
public sealed record PlayerWheelEventArgs
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
    /// The horizontal scroll amount.
    /// </summary>
    public double DeltaX { get; init; }

    /// <summary>
    /// The vertical scroll amount. Negative = scroll up, Positive = scroll down.
    /// </summary>
    public double DeltaY { get; init; }

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
