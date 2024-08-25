using System;

namespace MonoRivUI;

/// <summary>
/// An interface for a button component.
/// </summary>
public interface IButton : IComponent, IHoverable, IStyleable<IButton>
{
    /// <summary>
    /// An event raised when the button is clicked.
    /// </summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    [Style.Stylable]
    public new event EventHandler<IButtonContent<IComponent>>? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    [Style.Stylable]
    public new event EventHandler<IButtonContent<IComponent>>? HoverExited;

    /// <summary>
    /// Gets the component associated with this button.
    /// </summary>
    public IComponent Component { get; }

    /// <summary>
    /// Represents the style of a button component.
    /// </summary>
    public class Style : Style<IButton>
    {
        /// <summary>
        /// Gets or sets the hover entered event handler.
        /// </summary>
        public EventHandler<IComponent>? HoverEntered { get; set; }

        /// <summary>
        /// Gets or sets the hover exited event handler.
        /// </summary>
        public EventHandler<IComponent>? HoverExited { get; set; }
    }
}
