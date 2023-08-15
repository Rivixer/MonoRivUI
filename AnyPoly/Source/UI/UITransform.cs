using System;
using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a transformation for a UI component.
/// </summary>
/// <remarks>
/// It is responsible for positioning and sizing the component.
/// </remarks>
internal class UITransform
{
    private Point unscaledLocation;
    private Point unscaledSize;
    private Point scaledLocation;
    private Point scaledSize;

    private TransformType transformType;

    private Point minSize = new Point(1);
    private Point maxSize = new Point(int.MaxValue);
    private Ratio ratio = Ratio.Unspecified;

    private Vector2 relativeOffset = Vector2.Zero;
    private Vector2 relativeSize = Vector2.One;

    private bool needsRecalculation = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UITransform"/> class.
    /// </summary>
    /// <param name="component">The component associated with this transformation.</param>
    public UITransform(UIComponent component)
    {
        this.Component = component;
        component.OnParentChange += this.Component_OnParentChange;

        if (component.Parent is not null)
        {
            component.Parent.Transform.OnRecalculate += this.ParentTransform_OnRecalculate;
        }
        else
        {
            ScreenController.OnScreenChange += this.Recalculate;
        }

        this.needsRecalculation = true;
    }

    /// <summary>
    /// An event raised when the transformation has been recalculated.
    /// </summary>
    public event EventHandler? OnRecalculate;

    /// <summary>
    /// Gets the component associated with this transformation.
    /// </summary>
    public UIComponent Component { get; }

    /// <summary>
    /// Gets or sets the transform type.
    /// </summary>
    public TransformType TransformType
    {
        get => this.transformType;
        set
        {
            if (this.transformType == value)
            {
                return;
            }

            if (value is TransformType.Relative && this.Component.Parent is null)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.TransformType)} to {nameof(TransformType.Relative)} " +
                    $"when {nameof(this.Component)} has no {nameof(this.Component.Parent)}");
            }

            this.transformType = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative size of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="TransformType"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </remarks>
    public Vector2 RelativeSize
    {
        get => this.relativeSize;
        set
        {
            if (this.relativeSize == value)
            {
                return;
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.RelativeSize)} cannot have negative components.");
            }

            this.relativeSize = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative offset of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="TransformType"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </remarks>
    public Vector2 RelativeOffset
    {
        get => this.relativeOffset;
        set
        {
            if (this.relativeOffset == value)
            {
                return;
            }

            this.relativeOffset = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the minimum size of the component.
    /// </summary>
    public Point MinSize
    {
        get => this.minSize;
        set
        {
            if (this.minSize == value)
            {
                return;
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.MinSize)} cannot have negative components.");
            }

            this.minSize = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the maximum size of the component.
    /// </summary>
    public Point MaxSize
    {
        get => this.maxSize;
        set
        {
            if (this.maxSize == value)
            {
                return;
            }

            if (value.X < this.MinSize.X || value.Y < this.MinSize.Y)
            {
                throw new ArgumentException(
                    $"{nameof(this.MaxSize)} cannot be smaller than {nameof(this.MinSize)}.");
            }

            this.maxSize = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the ratio of the component.
    /// </summary>
    public Ratio Ratio
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return this.ratio;
        }

        set
        {
            if (this.ratio == value)
            {
                return;
            }

            this.ratio = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the unscaled location of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="TransformType"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point UnscaledLocation
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return this.unscaledLocation;
        }

        set
        {
            if (this.unscaledLocation == value)
            {
                return;
            }

            if (this.TransformType is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.UnscaledLocation)} " +
                    $"when {nameof(this.TransformType)}" +
                    $"is not {TransformType.Absolute}.");
            }

            this.unscaledLocation = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the unscaled size of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="TransformType"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point UnscaledSize
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return this.unscaledSize;
        }

        set
        {
            if (this.unscaledSize == value)
            {
                return;
            }

            if (this.TransformType is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.UnscaledSize)} " +
                    $"when {nameof(this.TransformType)}" +
                    $"is not {TransformType.Absolute}.");
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.UnscaledSize)} cannot have negative components.");
            }

            this.unscaledSize = value;
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the scaled location of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="TransformType"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point ScaledLocation
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return this.scaledLocation;
        }

        set
        {
            if (this.scaledLocation == value)
            {
                return;
            }

            if (this.TransformType is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.ScaledLocation)} " +
                    $"when {nameof(this.TransformType)}" +
                    $"is not {TransformType.Absolute}.");
            }

            this.unscaledLocation = value.Unscale(ScreenController.Scale);
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets or sets the scaled size of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="TransformType"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point ScaledSize
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return this.scaledSize;
        }

        set
        {
            if (this.scaledSize == value)
            {
                return;
            }

            if (this.TransformType is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.ScaledSize)} " +
                    $"when {nameof(this.TransformType)}" +
                    $"is not {TransformType.Absolute}.");
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.ScaledSize)} cannot have negative components.");
            }

            this.unscaledSize = value.Unscale(ScreenController.Scale);
            this.needsRecalculation = true;
        }
    }

    /// <summary>
    /// Gets the unscaled rectangle of the component.
    /// </summary>
    /// <remarks>
    /// The rectangle is specified for
    /// <see cref="ScreenController.DefaultSize"/> resolution.
    /// </remarks>
    public Rectangle UnscaledRectangle
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return new Rectangle(this.unscaledLocation, this.unscaledSize);
        }
    }

    /// <summary>
    /// Gets the scaled rectangle of the component.
    /// </summary>
    /// <remarks>
    /// The rectangle is scaled to current screen resolution.
    /// </remarks>
    public Rectangle ScaledRectangle
    {
        get
        {
            if (this.needsRecalculation)
            {
                this.Recalculate();
            }

            return new Rectangle(this.scaledLocation, this.scaledSize);
        }
    }

    /// <summary>
    /// Creates a new <see cref="UITransform"/> with default values.
    /// </summary>
    /// <param name="component">
    /// The component associated with the transformation.
    /// </param>
    /// <returns>The new <see cref="UITransform"/> with default values.</returns>
    public static UITransform Default(UIComponent component)
    {
        return new UITransform(component)
        {
            TransformType = TransformType.Absolute,
            unscaledLocation = new Point(0, 0),
            unscaledSize = ScreenController.DefaultSize,
        };
    }

    private void Recalculate()
    {
        switch (this.transformType)
        {
            case TransformType.Relative:
                this.RecalculateRelative();
                break;
            case TransformType.Absolute:
                this.RecalculateAbsolute();
                break;
        }

        this.scaledLocation = this.unscaledLocation.Scale(ScreenController.Scale);
        this.scaledSize = this.unscaledSize.Scale(ScreenController.Scale);
        this.needsRecalculation = false;

        this.OnRecalculate?.Invoke(this, EventArgs.Empty);
    }

    private void RecalculateRelative()
    {
        UITransform reference = this.Component.Parent!.Transform;

        this.unscaledLocation = reference.unscaledLocation;
        this.unscaledSize = reference.unscaledSize;

        this.RecalculateRatio();

        this.unscaledSize = reference.unscaledSize.Scale(this.relativeSize);
        this.unscaledLocation += reference.unscaledSize.Scale(this.relativeOffset);
    }

    private void RecalculateAbsolute()
    {
        this.RecalculateRatio();
    }

    private void RecalculateRatio()
    {
        if (this.ratio == Ratio.Unspecified)
        {
            return;
        }

        var currentRatio = this.unscaledSize.ToRatio();
        if (currentRatio == this.ratio)
        {
            return;
        }

        Point unscaledSize = this.unscaledSize;
        bool heightIsOversized = currentRatio.ToFloat() < this.ratio.ToFloat();
        if (heightIsOversized)
        {
            unscaledSize.Y = (int)(unscaledSize.X / this.ratio.ToFloat());
        }
        else
        {
            unscaledSize.X = (int)(unscaledSize.Y * this.ratio.ToFloat());
        }

        this.unscaledSize = unscaledSize;
    }

    private void ParentTransform_OnRecalculate(object? sender, EventArgs e)
    {
        this.Recalculate();
    }

    private void Component_OnParentChange(object? sender, ParentChangeEventArgs e)
    {
        if (e.OldParent is { } oldParent)
        {
            oldParent.Transform.OnRecalculate -= this.ParentTransform_OnRecalculate;
        }
        else
        {
            ScreenController.OnScreenChange -= this.Recalculate;
        }

        if (e.NewParent is { } newParent)
        {
            newParent.Transform.OnRecalculate += this.ParentTransform_OnRecalculate;
        }
        else
        {
            ScreenController.OnScreenChange += this.Recalculate;
        }
    }
}
