using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using static MonoRivUI.Style;

namespace MonoRivUI;

/// <summary>
/// Represents a button component with a specific component as its content.
/// </summary>
/// <typeparam name="T">The type of the contained component.</typeparam>
public class Button<T> : Component, IButton<T>, IStyleable<Button<T>>
    where T : Component, IButtonContent<Component>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Button{T}"/> class.
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
    /// The button's <see cref="Transform.Ratio"/> is set to the
    /// <paramref name="component"/>'s <see cref="Transform.Ratio"/>.
    /// </para>
    /// </remarks>
    public Button(T component)
    {
        this.Component = component;
        component.Parent = this;

        this.Transform.Ratio = component.Transform.Ratio;
    }

    /// <inheritdoc/>
    public event EventHandler? Clicking;

    /// <inheritdoc/>
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    [Stylable]
    public event EventHandler<T>? HoverEntered;

    /// <inheritdoc/>
    [Stylable]
    public event EventHandler<T>? HoverExited;

    /// <inheritdoc/>
    event EventHandler<IButtonContent<Component>>? IButton.HoverEntered
    {
        add => this.HoverEntered += (s, e) => value?.Invoke(s, e);
        remove => this.HoverEntered -= (s, e) => value?.Invoke(s, e);
    }

    /// <inheritdoc/>
    event EventHandler<IButtonContent<Component>>? IButton.HoverExited
    {
        add => this.HoverExited += (s, e) => value?.Invoke(s, e);
        remove => this.HoverExited -= (s, e) => value?.Invoke(s, e);
    }

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
    /// Gets the component associated with this button.
    /// </summary>
    [SubStylable]
    public T Component { get; }

    /// <inheritdoc/>
    public Button<T> ApplyStyle(Style<Button<T>> style)
    {
        style.Apply(this);
        return this;
    }

    /// <inheritdoc/>
    public Style<Button<T>> GetStyle()
    {
        var style = new Style()
        {
            HoverEntered = this.HoverEntered,
            HoverExited = this.HoverExited,
        };

        var componentType = this.Component.GetType();
        MethodInfo? getStyleMethod = componentType.GetMethod("GetStyle");
        var componentStyle = (MonoRivUI.Style?)getStyleMethod?.Invoke(this.Component, null);
        style.ComponentStyle = componentStyle;

        return style;
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        Point mousePosition = MouseController.Position;
        bool isFocused = MouseController.IsComponentFocused(this);

        this.WasHovered = this.IsHovered;
        this.IsHovered = isFocused
            && MouseController.DraggedComponent is null
            && !MouseController.WasDragStateChanged
            && this.Component.IsButtonContentHovered(mousePosition);

        if (isFocused && !this.WasHovered && this.IsHovered)
        {
            this.HoverEntered?.Invoke(this, this.Component);
        }
        else if (this.WasHovered && !this.IsHovered)
        {
            this.HoverExited?.Invoke(this, this.Component);
        }

        if (isFocused && MouseController.IsLeftButtonClicked() && this.IsHovered)
        {
            this.Clicked?.Invoke(this, EventArgs.Empty);
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// Represents the class for styling a <see cref="Button{T}"/>.
    /// </summary>
    public class Style : Style<Button<T>>
    {
        /// <summary>
        /// Gets or sets the hover entered event handler.
        /// </summary>
        public EventHandler<T>? HoverEntered { get; set; }

        /// <summary>
        /// Gets or sets the hover exited event handler.
        /// </summary>
        public EventHandler<T>? HoverExited { get; set; }

        /// <summary>
        /// Gets or sets the button's component style.
        /// </summary>
        [Name("Component")]
        public MonoRivUI.Style? ComponentStyle { get; set; }
    }
}
