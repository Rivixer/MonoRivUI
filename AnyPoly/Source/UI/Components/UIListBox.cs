using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AnyPoly.UI;

/// <summary>
/// Represents a list box component.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="UIListBox"/> component serves as a fundamental building
/// block for creating scrollable lists of UI components. It provides a
/// structured layout for arranging various UI components.
/// </para>
/// <para>
/// This component automatically creates a <see cref="ContentContainer"/>
/// when initialized. The content container is designed to host and organize
/// nested UI components within the list box and should be used as a parent for them.
/// </para>
/// <para>
/// After setting the <see cref="ContentContainer"/> as a parent for a component,
/// the component will be queued unit the next update cycle. This allows adding
/// multiple components to the list box without having to recalculate the list
/// box's layout after each addition.
/// </para>
/// <para>
/// Key features of the <see cref="UIListBox"/> component include support for
/// vertical and horizontal orientations, adjustable spacing between components,
/// scrollability for handling large amounts of content, and the option to
/// automatically resize its components to fit the available space
/// when scrollability is disabled.
/// </para>
/// <para>
/// The appearance of the scrollbar can be customized using properties like
/// <see cref="ScrollBarFrameColor"/>, <see cref="ScrollBarThumbColor"/>,
/// and <see cref="ScrollBarRelativeSize"/>.
/// </para>
/// <para>
/// The content components cannot be drawn outside of the list box's boundaries.
/// </para>
/// <para>
/// While it is possible to set the list box as a parent of a component,
/// the component will not be treated as a content component.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a new list box with a vertical orientation:
/// var listBox = new UIListBox()
/// {
///     Orientation = Orientation.Vertical
/// };
/// </code>
/// <code>
/// // Add a new text component to the list box:
/// var text = new UIText(Color.White)
/// {
///     Parent = listBox.ContentContainer
/// };
/// </code>
/// <code>
/// // Make the list box scrollable:
/// listBox.IsScrollable = true;
/// </code>
/// <code>
/// // Custom scrollbar appearance:
/// listBox.ScrollBarFrameColor = Color.White;
/// listBox.ScrollBarThumbColor = Color.Gray;
/// listBox.ScrollBarRelativeSize = 0.1f;
/// </code>
/// <code>
/// // Automatically resize components to fit the available space:
/// listBox.IsScrollable = false; // Disable scrollability, if enabled.
/// listBox.ResizeContent = true;
/// </code>
/// </example>
internal class UIListBox : UIComponent
{
    private readonly List<UIComponent> components = new();
    private readonly Queue<UIComponent> queuedComponents = new();
    private readonly UIContainer container;
    private Orientation orientation;

    private int spacing;
    private float totalLength;
    private float currentOffset;

    private bool isScrollable;
    private UIScrollBar? scrollBar;
    private Color scrollBarFrameColor;
    private Color scrollBarThumbColor;
    private float scrollBarRelativeSize;

    private bool resizeContent;

    private bool isRecalculationNeeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIListBox"/> class.
    /// </summary>
    public UIListBox()
    {
        this.container = new UIContainer() { Parent = this };
        this.container.ChildAdded += this.UIListBox_Container_ChildAdded;
        this.container.ChildRemoved += this.UIListBox_Container_ChildRemoved;

        this.Transform.SizeChanged += this.Transform_SizeChanged;
    }

    /// <summary>
    /// An event that is raised when components are being dequeued
    /// and are being added to the list box content.
    /// </summary>
    public event EventHandler? ComponentsDequeuing;

    /// <summary>
    /// An event that is raised when components have been dequeued
    /// and added to the list box content.
    /// </summary>
    public event EventHandler? ComponentsDequeued;

    /// <summary>
    /// Gets the list box's components.
    /// </summary>
    public IEnumerable<UIComponent> Components => this.components;

    /// <summary>
    /// Gets the list box's queued components.
    /// </summary>
    public IEnumerable<UIComponent> QueuedComponents => this.queuedComponents;

    /// <summary>
    /// Gets the read-only content container of the list box.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The content container serves as a designated rectangle where UI
    /// components can be positioned and organized within the list box.
    /// </para>
    /// <para>
    /// It is automatically created and managed by the <see cref="UIListBox"/>
    /// component, providing a structured layout for nested components.
    /// </para>
    /// <para>
    /// This property is read-only to prevent external modification.
    /// </para>
    /// <para>
    /// Use this inner container as a parent for nesting and
    /// organizing UI components within the list box.
    /// </para>
    /// </remarks>
    public IUIReadOnlyComponent ContentContainer => this.container;

    /// <remarks>
    /// <inheritdoc cref="UIScrollBar.Position"/>
    /// <para>
    /// Returns <see langword="null"/> when the scrollbar is not present.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="UIScrollBar.Position"/>
    public float? ScrollPosition => this.scrollBar?.Position;

    /// <summary>
    /// Gets or sets the color of the scrollbar's frame.
    /// </summary>
    public Color ScrollBarFrameColor
    {
        get => this.scrollBarFrameColor;
        set
        {
            this.scrollBarFrameColor = value;

            if (this.scrollBar is not null)
            {
                this.scrollBar.FrameColor = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the scrollbar's thumb.
    /// </summary>
    public Color ScrollBarThumbColor
    {
        get => this.scrollBarThumbColor;
        set
        {
            this.scrollBarThumbColor = value;

            if (this.scrollBar is not null)
            {
                this.scrollBar.ThumbColor = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the relative size of the scrollbar.
    /// </summary>
    public float ScrollBarRelativeSize
    {
        get => this.scrollBarRelativeSize;
        set
        {
            this.scrollBarRelativeSize = value;

            if (this.scrollBar is not null)
            {
                this.scrollBar.RelativeSize = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the spacing of the components.
    /// </summary>
    /// <remarks>
    /// The spacing is measured in pixels.
    /// </remarks>
    public int Spacing
    {
        get => this.spacing;
        set
        {
            if (this.spacing == value)
            {
                return;
            }

            int spacingCount = this.components.Count > 0 ? this.components.Count - 1 : 0;
            this.totalLength += (this.spacing - value) * spacingCount;
            this.spacing = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the orientation of the list box.
    /// </summary>
    public Orientation Orientation
    {
        get => this.orientation;
        set
        {
            if (this.orientation == value)
            {
                return;
            }

            this.orientation = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list box is scrollable.
    /// </summary>
    /// <remarks>
    /// Cannot be set to true when <see cref="ResizeContent"/> is set to true.
    /// </remarks>
    public bool IsScrollable
    {
        get => this.isScrollable;
        set
        {
            if (this.isScrollable == value)
            {
                return;
            }

            if (this.resizeContent)
            {
                throw new InvalidOperationException(
                    "Cannot make list box scrollable when " +
                    $"{nameof(this.ResizeContent)} is set to true.");
            }

            this.isScrollable = value;
            this.UpdateScrollBarPresence();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list box's components
    /// should be resized to fit the list box.
    /// </summary>
    /// <remarks>
    /// Cannot be set to true when <see cref="IsScrollable"/> is set to true.
    /// </remarks>
    public bool ResizeContent
    {
        get => this.resizeContent;
        set
        {
            if (this.resizeContent == value)
            {
                return;
            }

            if (this.isScrollable)
            {
                throw new InvalidOperationException(
                    "Cannot resize elements when " +
                    $"{nameof(this.IsScrollable)} is set to true.");
            }

            this.resizeContent = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets the list box scrollbar, if it exists.
    /// </summary>
    public IUIReadOnlyComponent? ScrollBar => this.scrollBar;

    /// <summary>
    /// Scrolls the list box to the specified position.
    /// </summary>
    /// <param name="position">
    /// <para>The position to scroll to.</para>
    /// The value is between <c>0.0f</c> and <c>1.0f</c>
    /// where <c>0.0f</c> is the top and <c>1.0f</c> is the bottom.
    /// </param>
    public void ScrollTo(float position)
    {
        this.scrollBar?.ScrollTo(position);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        bool isAnyComponentDequeued = this.queuedComponents.Count > 0;

        if (isAnyComponentDequeued)
        {
            this.ComponentsDequeuing?.Invoke(this, EventArgs.Empty);
            while (this.queuedComponents.Count > 0)
            {
                this.DequeueComponent();
            }
        }

        // We update all components before recalculating ListBox
        // to act on already calculated components.
        foreach (UIComponent component in this.components)
        {
            if (this.IsComponentVisible(component))
            {
                component.Update(gameTime);
            }
        }

        if (this.isRecalculationNeeded)
        {
            this.RecalculateElements();
            this.UpdateScrollBarPresence();
            this.isRecalculationNeeded = false;
        }

        if (isAnyComponentDequeued)
        {
            this.ComponentsDequeued?.Invoke(this, EventArgs.Empty);
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = SpriteBatchController.SpriteBatch;
        spriteBatch.End();

        var rasterizerState = new RasterizerState() { ScissorTestEnable = true };
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: BlendState.AlphaBlend,
            samplerState: null,
            depthStencilState: null,
            rasterizerState: rasterizerState);

        Rectangle tempRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
        spriteBatch.GraphicsDevice.ScissorRectangle = this.ContentContainer.Transform.ScaledRectangle;

        foreach (UIComponent component in this.components)
        {
            if (this.IsComponentVisible(component))
            {
                component.Draw(gameTime);
            }
        }

        spriteBatch.End();
        rasterizerState.Dispose();
        spriteBatch.GraphicsDevice.ScissorRectangle = tempRectangle;
        spriteBatch.Begin();

        base.Draw(gameTime);
    }

    private void ScrollBar_Scrolled(object? sender, ScrolledEventArgs e)
    {
        this.currentOffset = e.Current;
        this.isRecalculationNeeded = true;
    }

    private void Component_Transform_SizeChanged(
        object? sender,
        TransformElementChangedEventArgs<Point> e)
    {
        this.totalLength += this.orientation switch
        {
            Orientation.Vertical => e.After.Y - e.Before.Y,
            Orientation.Horizontal => e.After.X - e.Before.X,
            _ => throw new NotImplementedException(),
        };

        this.isRecalculationNeeded = true;
    }

    private bool IsComponentVisible(UIComponent component)
    {
        return component.Transform.ScaledRectangle.Bottom > this.Transform.ScaledRectangle.Top
            && component.Transform.ScaledRectangle.Top < this.Transform.ScaledRectangle.Bottom;
    }

    /// <summary>
    /// Returns the component unscaled length based on list box orientation.
    /// </summary>
    /// <param name="component">The component to be measured.</param>
    /// <returns>
    /// The component's unscaled height if the <see cref="Orientation"/>
    /// is set to <see cref="Orientation.Vertical"/>. <br/>
    /// The component's unscaled width if the <see cref="Orientation"/>
    /// is set to <see cref="Orientation.Horizontal"/>.
    /// </returns>
    private float GetComponentLength(UIComponent component)
    {
        Point size = component.Transform.UnscaledSize;
        return this.orientation switch
        {
            Orientation.Vertical => size.Y,
            Orientation.Horizontal => size.X,
            _ => throw new NotImplementedException(),
        };
    }

    private void RecalculateElements()
    {
        if (this.resizeContent)
        {
            float remainingSpace = -this.totalLength + this.orientation switch
            {
                Orientation.Vertical => this.Transform.UnscaledSize.Y,
                Orientation.Horizontal => this.Transform.UnscaledSize.X,
                _ => throw new NotImplementedException(),
            };
            this.ResizeElements(remainingSpace);
        }

        float currentOffset = -this.currentOffset;
        foreach (UIComponent component in this.components)
        {
            switch (this.orientation)
            {
                case Orientation.Vertical:
                    component.Transform.SetRelativeOffsetFromUnscaledAbsolute(y: currentOffset);
                    break;
                case Orientation.Horizontal:
                    component.Transform.SetRelativeOffsetFromUnscaledAbsolute(x: currentOffset);
                    break;
            }

            currentOffset += this.GetComponentLength(component) + this.spacing;
        }
    }

    /// <summary>
    /// Updates the size of the container
    /// based on the scrollbar's presence.
    /// </summary>
    private void AdjustContentContainerSize()
    {
        if (this.scrollBar is null)
        {
            this.container.Transform.RelativeSize = Vector2.One;
            return;
        }

        Point containerParentSize = this.container.Parent?.Transform.UnscaledSize
            ?? ScreenController.DefaultSize;
        Point scrollBarSize = this.scrollBar.Transform.UnscaledSize;

        switch (this.orientation)
        {
            case Orientation.Vertical:
                this.container.Transform.SetRelativeSizeFromUnscaledAbsolute(
                    x: containerParentSize.X - scrollBarSize.X);
                break;
            case Orientation.Horizontal:
                this.container.Transform.SetRelativeSizeFromUnscaledAbsolute(
                    y: containerParentSize.Y - scrollBarSize.Y);
                break;
        }
    }

    private void UpdateScrollBarPresence()
    {
        Point containerSize = this.container.Transform.UnscaledSize;
        int containerLength = this.orientation switch
        {
            Orientation.Vertical => containerSize.Y,
            Orientation.Horizontal => containerSize.X,
            _ => throw new NotImplementedException(),
        };

        bool isScrollBarNeeded = this.totalLength > containerLength;

        if (this.isScrollable && isScrollBarNeeded)
        {
            if (this.scrollBar is null)
            {
                this.scrollBar = new UIScrollBar(this.orientation, this.ContentContainer)
                {
                    Parent = this,
                    FrameColor = this.scrollBarFrameColor,
                    ThumbColor = this.scrollBarThumbColor,
                    RelativeSize = this.scrollBarRelativeSize,
                    Transform = { IgnoreParentPadding = true },
                };
                this.scrollBar.Scrolled += this.ScrollBar_Scrolled;
                this.AdjustContentContainerSize();
            }

            this.scrollBar.TotalLength = this.totalLength;
        }
        else if (this.scrollBar is not null)
        {
            this.scrollBar.Scrolled -= this.ScrollBar_Scrolled;
            this.scrollBar.IsEnabled = false;
            this.scrollBar = null;
            this.AdjustContentContainerSize();
        }
    }

    private void ResizeElements(float remainingSpace)
    {
        float totalLengthWithoutSpacing = this.totalLength - ((this.components.Count - 1) * this.spacing);
        float resizeFactor = 1 + (remainingSpace / totalLengthWithoutSpacing);
        foreach (UIComponent component in this.components)
        {
            component.Transform.RelativeSize *= this.orientation switch
            {
                Orientation.Vertical => new Vector2(1, resizeFactor),
                Orientation.Horizontal => new Vector2(resizeFactor, 1),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private void UIListBox_Container_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        // Improve performance by disabling automatic updating and drawing of the
        // child component. It will only be updated and drawn when it is visible.
        // Disabling these options also allows the component
        // to be queued until initialization is done.
        e.Child.AutoUpdate = false;
        e.Child.AutoDraw = false;
        this.queuedComponents.Enqueue(e.Child);
    }

    private void DequeueComponent()
    {
        UIComponent component = this.queuedComponents.Dequeue();

        component.Transform.SizeChanged += this.Component_Transform_SizeChanged;

        if (this.components.Count > 0)
        {
            this.totalLength += this.spacing;
        }

        float componentLength = this.GetComponentLength(component);
        this.totalLength += componentLength;

        this.components.Add(component);
        this.isRecalculationNeeded = true;
    }

    private void UIListBox_Container_ChildRemoved(object? sender, ChildChangedEventArgs e)
    {
        UIComponent child = e.Child;
        child.Transform.SizeChanged -= this.Component_Transform_SizeChanged;

        if (this.components.Count > 0)
        {
            this.totalLength -= this.spacing;
        }

        float componentLength = this.GetComponentLength(child);
        this.totalLength -= componentLength;
        this.UpdateScrollBarPresence();

        _ = this.components.Remove(child);
        this.isRecalculationNeeded = true;
    }

    private void Transform_SizeChanged(object? sender, TransformElementChangedEventArgs<Point> e)
    {
        Vector2 factor = e.Before.ToVector2() / e.After.ToVector2();
        this.Transform.RelativePadding *= new Vector4(factor, factor.X, factor.Y);

        foreach (var component in this.components)
        {
            component.Transform.RelativeSize *= factor;
        }

        this.RecalculateElements();
        this.UpdateScrollBarPresence();
        this.AdjustContentContainerSize();
    }
}
