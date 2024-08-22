using System;

namespace MonoRivUI;

/// <summary>
/// An interface for a hoverable component.
/// </summary>
public interface IHoverable
{
    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    public event EventHandler? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    public event EventHandler? HoverExited;

    /// <summary>
    /// Gets a value indicating whether the button
    /// is currently being hovered over.
    /// </summary>
    public bool IsHovered { get; }

    /// <summary>
    /// Gets a value indicating whether the button
    /// was hovered over in the previous frame.
    /// </summary>
    public bool WasHovered { get; }

    /// <summary>
    /// Resets the hover state of the button.
    /// </summary>
    public void ResetHover();
}
