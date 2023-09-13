using Microsoft.Xna.Framework;
using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents a button component with a specific component as its content.
/// </summary>
/// <typeparam name="T">The type of the contained component.</typeparam>
internal class UIButton<T> : UIComponent
    where T : UIComponent, IUIButtonContent<UIComponent>
{
    private readonly T component;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIButton{T}"/> class.
    /// </summary>
    /// <param name="component">The contained component.</param>
    /// <remarks>
    /// <para>
    /// This constructor initializes a button with
    /// <paramref name="component"/> as its content.
    /// </para>
    /// <para>
    /// The <paramref name="component"/>'s parent is set to this button.
    /// </para>
    /// <para>
    /// <see cref="HoverMethod"/> is set to
    /// <see cref="IUIButtonContent{T}.IsButtonContentHovered"/>.
    /// </para>
    /// <para>
    /// The button's <see cref="UITransform.Ratio"/> is set to the
    /// <paramref name="component"/>'s <see cref="UITransform.Ratio"/>.
    /// </para>
    /// </remarks>
    public UIButton(T component)
    {
        this.component = component;
        component.Parent = this;

        this.HoverMethod = component.IsButtonContentHovered;

        this.Transform.Ratio = component.Transform.Ratio;
    }

    /// <summary>
    /// An event raised when the button is clicked.
    /// </summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// An event raised when the button is hovered over.
    /// </summary>
    public event EventHandler<HoverStateChangedEventArgs<T>>? HoverEntered;

    /// <summary>
    /// An event raised when the button is no longer hovered over.
    /// </summary>
    public event EventHandler<HoverStateChangedEventArgs<T>>? HoverExited;

    /// <summary>
    /// Gets a value indicating whether the button
    /// is currently being hovered over.
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the button
    /// was hovered over in the previous frame.
    /// </summary>
    public bool WasHovered { get; private set; }

    /// <summary>
    /// Gets or sets the method used to determine
    /// if the button is being hovered over.
    /// </summary>
    /// <remarks>
    /// Setting this method does not exempt checking if the
    /// cursor is within <see cref="UITransform.ScaledRectangle"/>.
    /// Both conditions must be met for the button
    /// to be considered as hovered.
    /// </remarks>
    public Func<Point, bool> HoverMethod { get; set; }

    /// <summary>
    /// Gets the read-only component associated with this button.
    /// </summary>
    public IUIReadOnlyComponent Component => this.component;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        Point cursorPosition = MouseController.Position;
        bool isCursorInRect = this.Transform.ScaledRectangle.Contains(cursorPosition);

        this.WasHovered = this.IsHovered;
        this.IsHovered = isCursorInRect
            && (this.HoverMethod?.Invoke(cursorPosition) ?? true);

        if (!this.WasHovered && this.IsHovered)
        {
            this.InvokeHoverEvent(this.HoverEntered);
        }
        else if (this.WasHovered && !this.IsHovered)
        {
            this.InvokeHoverEvent(this.HoverExited);
        }

        if (MouseController.IsLeftButtonClicked() && this.IsHovered)
        {
            this.Clicked?.Invoke(this, EventArgs.Empty);
        }

        base.Update(gameTime);
    }

    private void InvokeHoverEvent(EventHandler<HoverStateChangedEventArgs<T>>? hoverEvent)
    {
        if (hoverEvent is not null)
        {
            var eventArgs = new HoverStateChangedEventArgs<T>(this.component);
            hoverEvent.Invoke(this, eventArgs);
        }
    }
}
