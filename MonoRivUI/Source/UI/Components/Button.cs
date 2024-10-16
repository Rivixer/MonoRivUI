using System;
using System.Linq;
using Microsoft.Xna.Framework;
using static MonoRivUI.Style;

namespace MonoRivUI;

/// <summary>
/// Represents a button component with a specific component as its content.
/// </summary>
/// <typeparam name="T">The type of the contained component.</typeparam>
public class Button<T> : Component, IButton<T>
    where T : Component, IButtonContent<IComponent>
{
    private bool isFocusBlockedByOverlay;
    private bool wasFocusBlockedByOverlay;

    static Button()
    {
        Scene.SceneChanged += (s, e) =>
        {
            // Reset all hovered buttons when the scene changes.
            e.Previous?.BaseComponent
                .GetAllDescendants<Button<T>>(x => x.IsEnabled)
                .ToList()
                .ForEach(btn =>
                {
                    btn.IsHovered = btn.WasHovered = false;
                    btn.OnHoverExited();
                });
        };
    }

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
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    [Stylable]
    public event EventHandler<T>? HoverEntered;

    /// <inheritdoc/>
    [Stylable]
    public event EventHandler<T>? HoverExited;

    /// <inheritdoc/>
    event EventHandler<IButtonContent<IComponent>>? IButton.HoverEntered
    {
        add => this.HoverEntered += (s, e) => value?.Invoke(s, e);
        remove => this.HoverEntered -= (s, e) => value?.Invoke(s, e);
    }

    /// <inheritdoc/>
    event EventHandler<IButtonContent<IComponent>>? IButton.HoverExited
    {
        add => this.HoverExited += (s, e) => value?.Invoke(s, e);
        remove => this.HoverExited -= (s, e) => value?.Invoke(s, e);
    }

    /// <inheritdoc/>
    event EventHandler? IHoverable.HoverEntered
    {
        add => this.HoverEntered += (s, e) => value?.Invoke(s, EventArgs.Empty);
        remove => this.HoverEntered -= (s, e) => value?.Invoke(s, EventArgs.Empty);
    }

    /// <inheritdoc/>
    event EventHandler? IHoverable.HoverExited
    {
        add => this.HoverExited += (s, e) => value?.Invoke(s, EventArgs.Empty);
        remove => this.HoverExited -= (s, e) => value?.Invoke(s, EventArgs.Empty);
    }

    /// <inheritdoc/>
    IComponent IButton.Component => this.Component;

    /// <inheritdoc/>
    public T Component { get; }

    /// <inheritdoc/>
    public bool IsHovered { get; private set; }

    /// <inheritdoc/>
    public bool WasHovered { get; private set; }

    /// <inheritdoc/>
    public void ApplyStyle(Style<IButton> style)
    {
        style.Apply(this);
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<IButton<T>> style)
    {
        style.Apply(this);
    }

    /// <inheritdoc/>
    public void ResetHover()
    {
        if (this.IsHovered)
        {
            this.IsHovered = this.WasHovered = false;
            this.OnHoverExited();
        }
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

        this.wasFocusBlockedByOverlay = this.isFocusBlockedByOverlay;

        // Temporary solution to fix the issue with the overlay blocking the focus.
        IOverlay? overlay = ScreenController.DisplayedOverlays
            .Select(x => x.Value)
            .FirstOrDefault(x => x is IOverlayScene o && o.BaseComponent == (IComponent)this.Root);
        overlay ??= this.Root as IOverlay;
        this.isFocusBlockedByOverlay = isFocused && ScreenController.IsFocusBlockedByOverlay(overlay);

        if (isFocused && (!this.WasHovered || this.wasFocusBlockedByOverlay) && this.IsHovered && !this.isFocusBlockedByOverlay)
        {
            this.OnHoverEntered();
        }
        else if (this.WasHovered && (!this.IsHovered || this.isFocusBlockedByOverlay))
        {
            this.OnHoverExited();
        }

        if (isFocused && MouseController.IsLeftButtonClicked() && this.IsHovered && !this.wasFocusBlockedByOverlay)
        {
            this.Clicked?.Invoke(this, EventArgs.Empty);
        }

        base.Update(gameTime);
    }

    private void OnHoverEntered()
    {
        this.HoverEntered?.Invoke(this, this.Component);
    }

    private void OnHoverExited()
    {
        this.HoverExited?.Invoke(this, this.Component);
    }
}
