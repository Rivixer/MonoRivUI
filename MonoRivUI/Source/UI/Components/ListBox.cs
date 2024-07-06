using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a list box component.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ListBox"/> component serves as a fundamental building
/// block for creating scrollable lists of components. It provides a
/// structured layout for arranging various components.
/// </para>
/// <para>
/// This component automatically creates a <see cref="ContentContainer"/>
/// when initialized. The content container is designed to host and organize
/// nested components within the list box and should be used as a parent for them.
/// </para>
/// <para>
/// After setting the <see cref="ContentContainer"/> as a parent for a component,
/// the component will be queued unit the next update cycle. This allows adding
/// multiple components to the list box without having to recalculate the list
/// box's layout after each addition.
/// </para>
/// <para>
/// Key features of the <see cref="ListBox"/> component include support for
/// vertical and horizontal orientations, adjustable spacing between components,
/// scrollability for handling large amounts of content, and the option to
/// automatically resize its components to fit the available space
/// when scrollability is disabled.
/// </para>
/// <para>
/// The appearance of the scrollbar can be customized
/// using the <see cref="ScrollBar"/> property.
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
/// var listBox = new ListBox()
/// {
///     Orientation = Orientation.Vertical
/// };
/// </code>
/// <code>
/// // Add a new text component to the list box:
/// var text = new Text(Color.White)
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
/// ScrollBar =
/// {
///     FrameColor = Color.White;
///     ThumbColor = Color.Gray;
///     FrameThickness = 2;
///     BackgroundColor = Color.Black;
///     RelativeSize = 0.1f;
/// }
/// </code>
/// <code>
/// // Automatically resize components to fit the available space:
/// listBox.IsScrollable = false; // Disable scrollability, if enabled.
/// listBox.ResizeContent = true;
/// </code>
/// </example>
public class ListBox : Component
{
    private readonly List<Component> components = new();
    private readonly Queue<Component> queuedComponents = new();
    private readonly Container innerContainer;
    private readonly Container contentContainer;
    private readonly ScrollBar scrollBar;
    private Orientation orientation;

    private int spacing;
    private float totalLength;
    private float currentOffset;

    private bool isScrollable;
    private bool resizeContent;

    private bool isRecalculationNeeded;
    private bool isContentResized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListBox"/> class.
    /// </summary>
    public ListBox()
    {
        this.innerContainer = new Container() { Parent = this };

        this.contentContainer = new Container() { Parent = this.innerContainer };
        this.contentContainer.ChildAdded += this.ListBox_Container_ChildAdded;
        this.contentContainer.ChildRemoved += this.ListBox_Container_ChildRemoved;

        this.scrollBar = new ScrollBar(this.ContentContainer)
        {
            IsEnabled = false,
            Parent = this,
            Transform = { IgnoreParentPadding = true },
        };

        this.scrollBar.Scrolled += this.ScrollBar_Scrolled;
        this.AdjustContentContainerSize();
    }

    /// <summary>
    /// An event that is raised when a component is being dequeued
    /// and is being added to the list box content.
    /// </summary>
    public event EventHandler<Component>? ComponentDequeuing;

    /// <summary>
    /// An event that is raised when a component has been dequeued
    /// and added to the list box content.
    /// </summary>
    public event EventHandler<Component>? ComponentDequeued;

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
    public IEnumerable<Component> Components => this.components;

    /// <summary>
    /// Gets the list box's queued components.
    /// </summary>
    public IEnumerable<Component> QueuedComponents => this.queuedComponents;

    /// <summary>
    /// Gets the read-only content container of the list box.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The content container serves as a designated rectangle where
    /// components can be positioned and organized within the list box.
    /// </para>
    /// <para>
    /// It is automatically created and managed by <see cref="ListBox"/>,
    /// providing a structured layout for nested components.
    /// </para>
    /// <para>
    /// This property is read-only to prevent external modification.
    /// </para>
    /// <para>
    /// Use this container as a parent for nesting and
    /// organizing components within the list box.
    /// </para>
    /// </remarks>
    public IReadOnlyComponent ContentContainer => this.contentContainer;

    /// <summary>
    /// Gets or sets the margin of the content container relative to the list box.
    /// </summary>
    public Vector4 ContentContainerRelativeMargin
    {
        get => this.innerContainer.Transform.RelativePadding;
        set => this.innerContainer.Transform.RelativePadding = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content can be drawn on the margin.
    /// </summary>
    /// <remarks>
    /// When <see cref="ContentContainerRelativeMargin"/> is set, the content
    /// will be also drawn on the margin if this property is set to true.
    /// </remarks>
    public bool DrawContentOnMargin { get; set; }

    /// <summary>
    /// Gets the scrollbar.
    /// </summary>
    /// <remarks>
    /// Cannot be accessed and will return <see langword="null"/>
    /// when <see cref="IsScrollable"/> is set to false.
    /// </remarks>
    public ScrollBar? ScrollBar => this.isScrollable
        ? this.scrollBar
        : null;

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

            int spacingCount = Math.Max(0, this.components.Count - 1);
            this.TotalLength += (value - this.spacing) * spacingCount;
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
            this.scrollBar.Orientation = value;
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
    /// <para>Cannot be set to true when <see cref="IsScrollable"/> is set to true.</para>
    /// <para>Each component will be have the same size to fit all space.</para>
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
    /// Gets the total length of the listbox's components.
    /// </summary>
    public float TotalLength
    {
        get
        {
            if (this.isRecalculationNeeded)
            {
                this.RecalculateElements();
            }

            return this.totalLength;
        }

        private set
        {
            if (this.totalLength == value)
            {
                return;
            }

            this.totalLength = value;
            this.scrollBar.TotalLength = value;
        }
    }

    private bool IsScrollBarNeeded => this.TotalLength > this.ContainerLength;

    private float ContainerLength => this.orientation switch
    {
        Orientation.Vertical => this.contentContainer.Transform.Size.Y,
        Orientation.Horizontal => (float)this.contentContainer.Transform.Size.X,
        _ => throw new NotImplementedException(),
    };

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.DequeueComponents();

        // We update all components before recalculating ListBox
        // to act on already calculated components.
        // It is safe, because we disable AutoUpdate for components,
        // so base.Update(GameTime) will not update them again.
        foreach (Component component in this.components)
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

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        SpriteBatch spriteBatch = SpriteBatchController.SpriteBatch;
        spriteBatch.End();

        var rasterizerState = new RasterizerState() { ScissorTestEnable = true };
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: BlendState.NonPremultiplied,
            samplerState: null,
            depthStencilState: null,
            rasterizerState: rasterizerState);

        Rectangle tempRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

        Rectangle scissorRect = (this.DrawContentOnMargin ? this.innerContainer : this.contentContainer).Transform.DestRectangle;
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;

        foreach (Component component in this.components)
        {
            if (this.IsComponentVisible(component))
            {
                component.Draw(gameTime);
            }
        }

        spriteBatch.End();
        rasterizerState.Dispose();
        spriteBatch.GraphicsDevice.ScissorRectangle = tempRectangle;
        spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Dequeues all components that are queued for initialization.
    /// </summary>
    public void DequeueComponents()
    {
        this.ComponentsDequeuing?.Invoke(this, EventArgs.Empty);

        Component? component = null;
        while (this.queuedComponents.Count > 0)
        {
            component = this.queuedComponents.Dequeue();
            this.ComponentDequeuing?.Invoke(this, component);

            float componentLength = this.GetComponentLength(component);
            this.TotalLength += componentLength;
            component.Transform.SizeChanged += this.Component_Transform_SizeChanged;

            if (this.components.Count > 0)
            {
                this.TotalLength += this.spacing;
            }

            this.components.Add(component);
            this.isRecalculationNeeded = true;
            this.isContentResized = false;
            component.Transform.ForceRecalulcation();
            this.ComponentDequeued?.Invoke(this, component);
        }

        // If any component was dequeued, recalculate elements.
        if (component is not null)
        {
            this.RecalculateElements();
        }

        this.ComponentsDequeued?.Invoke(this, EventArgs.Empty);
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
        this.TotalLength += this.orientation switch
        {
            Orientation.Vertical => e.After.Y - e.Before.Y,
            Orientation.Horizontal => e.After.X - e.Before.X,
            _ => throw new NotImplementedException(),
        };

        this.isRecalculationNeeded = true;
    }

    private bool IsComponentVisible(Component component)
    {
        return component.Transform.DestRectangle.Bottom > this.Transform.DestRectangle.Top
            && component.Transform.DestRectangle.Top < this.Transform.DestRectangle.Bottom;
    }

    /// <summary>
    /// Returns the component length based on list box orientation.
    /// </summary>
    /// <param name="component">The component to be measured.</param>
    /// <returns>
    /// The component's height if the <see cref="Orientation"/>
    /// is set to <see cref="Orientation.Vertical"/>. <br/>
    /// The component's width if the <see cref="Orientation"/>
    /// is set to <see cref="Orientation.Horizontal"/>.
    /// </returns>
    private float GetComponentLength(Component component)
    {
        Point size = component.Transform.Size;
        return this.orientation switch
        {
            Orientation.Vertical => size.Y,
            Orientation.Horizontal => size.X,
            _ => throw new NotImplementedException(),
        };
    }

    private void RecalculateElements()
    {
        if (this.resizeContent && !this.isContentResized)
        {
            this.ResizeElements();
            this.isContentResized = true;
        }

        float currentOffset = -this.currentOffset;
        foreach (Component component in this.components)
        {
            switch (this.orientation)
            {
                case Orientation.Vertical:
                    component.Transform.SetRelativeOffsetFromAbsolute(y: currentOffset);
                    break;
                case Orientation.Horizontal:
                    component.Transform.SetRelativeOffsetFromAbsolute(x: currentOffset);
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
        if (!this.isScrollable || !this.IsScrollBarNeeded)
        {
            this.contentContainer.Transform.RelativeSize = Vector2.One;
            return;
        }

        Point containerParentSize = this.contentContainer.Parent!.Transform.Size;
        Point scrollBarSize = this.scrollBar.Transform.Size;

        switch (this.orientation)
        {
            case Orientation.Vertical:
                this.contentContainer.Transform.SetRelativeSizeFromAbsolute(
                    x: containerParentSize.X - scrollBarSize.X);
                break;
            case Orientation.Horizontal:
                this.contentContainer.Transform.SetRelativeSizeFromAbsolute(
                    y: containerParentSize.Y - scrollBarSize.Y);
                break;
        }
    }

    private void UpdateScrollBarPresence()
    {
        if (this.isScrollable && this.IsScrollBarNeeded)
        {
            if (!this.scrollBar.IsEnabled)
            {
                this.scrollBar.IsEnabled = true;
                this.AdjustContentContainerSize();
            }
        }
        else
        {
            this.scrollBar.IsEnabled = false;
            this.AdjustContentContainerSize();
            this.currentOffset = 0;
        }
    }

    private void ResizeElements()
    {
        int spacingLength = this.spacing * (this.components.Count - 1);
        float resizeFactor = (1f - (spacingLength / this.ContainerLength)) / this.components.Count;
        foreach (Component component in this.components)
        {
            component.Transform.RelativeSize = this.orientation switch
            {
                Orientation.Vertical => new Vector2(1, resizeFactor),
                Orientation.Horizontal => new Vector2(resizeFactor, 1),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private void ListBox_Container_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        // Improve performance by disabling automatic updating and drawing of the
        // child component. It will only be updated and drawn when it is visible.
        // Disabling these options also allows the component
        // to be queued until initialization is done.
        e.Child.AutoUpdate = false;
        e.Child.AutoDraw = false;
        this.queuedComponents.Enqueue(e.Child);
    }

    private void ListBox_Container_ChildRemoved(object? sender, ChildChangedEventArgs e)
    {
        Component child = e.Child;
        child.Transform.SizeChanged -= this.Component_Transform_SizeChanged;

        if (this.components.Count > 0)
        {
            this.TotalLength -= this.spacing;
        }

        float componentLength = this.GetComponentLength(child);
        this.TotalLength -= componentLength;
        this.UpdateScrollBarPresence();

        _ = this.components.Remove(child);
        this.isRecalculationNeeded = true;
        this.isContentResized = false;
    }
}
