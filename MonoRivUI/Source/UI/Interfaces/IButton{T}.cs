using System;

namespace MonoRivUI;

/// <summary>
/// A generic interface for a button component.
/// </summary>
/// <typeparam name="T">The type of the contained component.</typeparam>
public interface IButton<T> : IButton
    where T : Component, IButtonContent<Component>
{
    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    public new event EventHandler<T>? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    public new event EventHandler<T>? HoverExited;
}
