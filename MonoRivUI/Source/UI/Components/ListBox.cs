using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a list box, which can contain multiple components.
/// </summary>
/// <remarks>
/// The components are placed in a vertical or horizontal orientation,
/// next to each other.
/// </remarks>
/// <seealso cref="AlignedListBox"/>
/// <seealso cref="FlexListBox"/>
/// <seealso cref="ScrollableListBox"/>
public class ListBox : Component
{
    private readonly List<Component> components = new();

    private Orientation orientation = Orientation.Vertical;
    private int spacing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListBox"/> class.
    /// </summary>
    public ListBox()
    {
        this.ContentContainer = new Container() { Parent = this };
        this.ContentContainer.ChildAdded += this.ContentContainer_ChildAdded;
        this.ContentContainer.ChildRemoved += this.ContentContainer_ChildRemoved;

        this.ChildAdded += this.ListBox_ChildAdded;
        this.Transform.Recalculated += this.ListBox_Transform_Recalculated;
    }

    /// <summary>
    /// Occurs when a child component is about to be added to the list box.
    /// </summary>
    public event EventHandler<Component>? ComponentAdding;

    /// <summary>
    /// Occurs when a child component is added to the list box.
    /// </summary>
    public event EventHandler<Component>? ComponentAdded;

    /// <summary>
    /// Occurs when a child component is about to be removed from the list box.
    /// </summary>
    public event EventHandler<Component>? ComponentRemoving;

    /// <summary>
    /// Occurs when a child component is removed from the list box.
    /// </summary>
    public event EventHandler<Component>? ComponentRemoved;

    /// <summary>
    /// Gets the components of the list box.
    /// </summary>
    public IEnumerable<Component> Components => this.components;

    /// <summary>
    /// Gets or sets the content container of the list box.
    /// </summary>
    public Container ContentContainer { get; protected set; }

    /// <summary>
    /// Gets or sets the orientation of the list box.
    /// </summary>
    public virtual Orientation Orientation
    {
        get => this.orientation;
        set
        {
            if (this.orientation == value)
            {
                return;
            }

            this.orientation = value;
            this.IsRecalulcationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the spacing between the content elements.
    /// </summary>
    public virtual int Spacing
    {
        get => this.spacing;
        set
        {
            if (this.spacing == value)
            {
                return;
            }

            this.spacing = value;
            this.IsRecalulcationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// a recalculation of the content elements is needed.
    /// </summary>
    protected bool IsRecalulcationNeeded { get; set; }

    /// <summary>
    /// Gets the length of the content container in the current orientation.
    /// </summary>
    /// <value>
    /// The height of the content container in vertical orientation,
    /// the width of the content container in horizontal orientation.
    /// </value>
    protected float ContentContainerLength => this.orientation switch
    {
        Orientation.Vertical => this.ContentContainer.Transform.Size.Y,
        Orientation.Horizontal => this.ContentContainer.Transform.Size.X,
        _ => throw new NotSupportedException(),
    };

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.IsRecalulcationNeeded)
        {
            this.RecalculateContentElements();
            this.IsRecalulcationNeeded = false;
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void ForceUpdate(bool? withTransform = null)
    {
        base.ForceUpdate(withTransform);
        this.RecalculateContentElements();
        this.IsRecalulcationNeeded = false;
    }

    /// <summary>
    /// Clears all components from the list box.
    /// </summary>
    public void Clear()
    {
        for (int i = this.components.Count - 1; i >= 0; i--)
        {
            Component component = this.components[i];
            component.Parent = null;
        }

        this.components.Clear();
    }

    /// <summary>
    /// Returns the length of the component in the current orientation.
    /// </summary>
    /// <param name="component">The component to get the length from.</param>
    /// <returns>The length of the component in the current orientation.</returns>
    /// <remarks>
    /// The length is the width of the component in horizontal orientation,
    /// and the height of the component in vertical orientation.
    /// </remarks>
    protected int GetComponentLength(Component component)
    {
        Point size = component.Transform.Size;
        return this.orientation switch
        {
            Orientation.Vertical => size.Y,
            Orientation.Horizontal => size.X,
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Recalculates the position of the content elements.
    /// </summary>
    /// <param name="currentOffset">The current offset of the content elements.</param>
    protected virtual void RecalculateContentElements(int currentOffset = 0)
    {
        switch (this.orientation)
        {
            case Orientation.Vertical:
                this.components.ToList().ForEach(component =>
                {
                    var componentLength = this.GetComponentLength(component);
                    component.Transform.SetRelativeOffsetFromAbsolute(y: currentOffset);
                    currentOffset += this.spacing + this.GetComponentLength(component);
                });
                break;
            case Orientation.Horizontal:
                this.components.ToList().ForEach(component =>
                {
                    component.Transform.SetRelativeOffsetFromAbsolute(x: currentOffset);
                    currentOffset += this.spacing + this.GetComponentLength(component);
                });
                break;
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// A method that is called when a child is added to the content container.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void ContentContainer_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        this.ComponentAdding?.Invoke(this, e.Child);

        e.Child.Transform.SizeChanged += this.ContentElement_SizeChanged;
        this.components.Add(e.Child);
        this.IsRecalulcationNeeded = true;

        this.ComponentAdded?.Invoke(this, e.Child);
    }

    /// <summary>
    /// A method that is called when a child is removed from the content container.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void ContentContainer_ChildRemoved(object? sender, ChildChangedEventArgs e)
    {
        this.ComponentRemoving?.Invoke(this, e.Child);

        e.Child.Transform.SizeChanged -= this.ContentElement_SizeChanged;
        _ = this.components.Remove(e.Child);
        this.IsRecalulcationNeeded = true;

        this.ComponentRemoved?.Invoke(this, e.Child);
    }

    /// <summary>
    /// A method that is called when the size of a content element has changed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    protected virtual void ContentElement_SizeChanged(object? sender, TransformElementChangedEventArgs<Point> e)
    {
        this.IsRecalulcationNeeded = true;
    }

    private void ListBox_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        // Be sure, that the content container is on top
        // Just set the contentContainer parent again
        if (e.Child != this.ContentContainer)
        {
            this.ContentContainer.Parent = null;
            this.ContentContainer.Parent = this;
        }
    }

    private void ListBox_Transform_Recalculated(object? sender, EventArgs e)
    {
        this.IsRecalulcationNeeded = true;
    }
}
