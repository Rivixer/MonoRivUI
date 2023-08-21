using System;
using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI padding component.
/// </summary>
/// <remarks>
/// Padding values are relative to the parent
/// element to which padding is applied.
/// If the element has no parent, padding is relative
/// to the default screen size.
/// </remarks>
internal class UIPadding : UIComponent
{
    private readonly UIContainer container;
    private bool isContainerUpdateNeeded = true;

    private float left;
    private float top;
    private float right;
    private float bottom;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIPadding"/> class.
    /// </summary>
    public UIPadding()
    {
        this.container = new UIContainer()
        {
            Parent = this,
            TransformType = TransformType.Absolute,
        };

        this.ChildAdded += this.UIPadding_ChildAdded;
        this.Transform.Recalculated += this.Transform_Recalculated;
    }

    /// <summary>
    /// Gets or sets the relative left padding value.
    /// </summary>
    public float Left
    {
        get => this.left;
        set
        {
            if (this.left == value)
            {
                return;
            }

            this.left = value;
            this.isContainerUpdateNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative top padding value.
    /// </summary>
    public float Top
    {
        get => this.top;
        set
        {
            if (this.top == value)
            {
                return;
            }

            this.top = value;
            this.isContainerUpdateNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative right padding value.
    /// </summary>
    public float Right
    {
        get => this.right;
        set
        {
            if (this.right == value)
            {
                return;
            }

            this.right = value;
            this.isContainerUpdateNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative bottom padding value.
    /// </summary>
    public float Bottom
    {
        get => this.bottom;
        set
        {
            if (this.bottom == value)
            {
                return;
            }

            this.bottom = value;
            this.isContainerUpdateNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isContainerUpdateNeeded)
        {
            this.UpdateInnerRectangle();
            this.isContainerUpdateNeeded = false;
        }

        base.Update(gameTime);
    }

    private void UpdateInnerRectangle()
    {
        Point location = this.Transform.UnscaledLocation;
        Point size = this.Transform.UnscaledSize;

        Point referenceSize;
        if (this.Transform.TransformType is TransformType.Absolute
            || this.Parent is null
            || this.Parent.Parent is null)
        {
            referenceSize = ScreenController.DefaultSize;
        }
        else
        {
            referenceSize = this.Parent.Parent.Transform.ScaledSize;
        }

        float offsetX = this.Left * referenceSize.X;
        float offsetY = this.Top * referenceSize.Y;
        float offsetZ = -(this.Right * referenceSize.X) - offsetX;
        float offsetW = -(this.Bottom * referenceSize.Y) - offsetY;

        location.X += (int)offsetX;
        location.Y += (int)offsetY;
        size.X += (int)offsetZ;
        size.Y += (int)offsetW;

        this.container.Transform.UnscaledLocation = location;
        this.container.Transform.UnscaledSize = size;
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.UpdateInnerRectangle();
        this.isContainerUpdateNeeded = false;
    }

    private void UIPadding_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        this.ReparentChild(e.Child, this.container);
    }
}
