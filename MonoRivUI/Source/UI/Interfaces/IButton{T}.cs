using System;

namespace MonoRivUI;

/// <summary>
/// A generic interface for a button component.
/// </summary>
/// <typeparam name="T">The type of the contained component.</typeparam>
public interface IButton<T> : IButton, IStyleable<IButton<T>>
    where T : IComponent, IButtonContent<IComponent>
{
    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    [Style.Stylable]
    public new event EventHandler<T>? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    [Style.Stylable]
    public new event EventHandler<T>? HoverExited;

    /// <summary>
    /// Gets the component associated with this button.
    /// </summary>
    public new T Component { get; }

    /// <summary>
    /// Represents the style of a button component.
    /// </summary>
    public new class Style : Style<IButton<T>>
    {
        /// <summary>
        /// Gets or sets the hover entered event handler.
        /// </summary>
        public EventHandler<T>? HoverEntered { get; set; }

        /// <summary>
        /// Gets or sets the hover exited event handler.
        /// </summary>
        public EventHandler<T>? HoverExited { get; set; }
    }
}
