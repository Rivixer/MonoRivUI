using System;

namespace MonoRivUI;

/// <summary>
/// An interface for a button component.
/// </summary>
public interface IButton : IReadOnlyComponent, IHoverable
{
    /// <summary>
    /// An event raised when the button is clicking.
    /// </summary>
    public event EventHandler? Clicking;

    /// <summary>
    /// An event raised when the button is clicked.
    /// </summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    public new event EventHandler<IButtonContent<Component>>? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    public new event EventHandler<IButtonContent<Component>>? HoverExited;
}
