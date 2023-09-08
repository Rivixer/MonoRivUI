using Microsoft.Xna.Framework;
using System;
using System.Linq;

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

    private Point minSize = new(1);
    private Point maxSize = new(int.MaxValue);
    private Ratio ratio = Ratio.Unspecified;
    private Alignment alignment = Alignment.TopLeft;

    private Vector2 relativeOffset = Vector2.Zero;
    private Vector2 relativeSize = Vector2.One;

    /// <summary>
    /// Initializes a new instance of the <see cref="UITransform"/> class.
    /// </summary>
    /// <param name="component">The component associated with this transformation.</param>
    public UITransform(UIComponent component)
    {
        this.Component = component;
        component.ParentChanged += this.Component_ParentChanged;

        if (component.Parent is null)
        {
            ScreenController.ScreenChanged += this.ScreenController_ScreenChanged;
        }

        this.IsRecalculationNeeded = true;
    }

    /// <summary>
    /// An event raised when the transformation has been recalculated.
    /// </summary>
    public event EventHandler? Recalculated;

    /// <summary>
    /// An event raised when the location has been changed.
    /// </summary>
    public event EventHandler<TransformElementChangedEventArgs<Point>>? LocationChanged;

    /// <summary>
    /// An event raised when the size has been changed.
    /// </summary>
    public event EventHandler<TransformElementChangedEventArgs<Point>>? SizeChanged;

    /// <summary>
    /// Gets the component associated with this transformation.
    /// </summary>
    public UIComponent Component { get; }

    /// <summary>
    /// Gets or sets the transform type.
    /// </summary>
    public TransformType Type
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
                    $"Cannot set {nameof(this.Type)} to {nameof(TransformType.Relative)} " +
                    $"when {nameof(this.Component)} has no {nameof(this.Component.Parent)}");
            }

            this.transformType = value;
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the transformation needs to be recalculated.
    /// </summary>
    public bool IsRecalculationNeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the relative size of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="Type"/>
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
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative offset of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="Type"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </remarks>
    /// <seealso cref="SetRelativeOffsetFromScaledAbsolute(float?, float?)"/>
    /// <seealso cref="SetRelativeOffsetFromUnscaledAbsolute(float?, float?)"/>
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
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the minimum size of the component,
    /// regardless of the screen size.
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
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the maximum size of the component,
    /// regardless of the screen size.
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
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the ratio of the component.
    /// </summary>
    public Ratio Ratio
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.ratio;
        }

        set
        {
            if (this.ratio == value)
            {
                return;
            }

            this.ratio = value;
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the alignment of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="Type"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </remarks>
    public Alignment Alignment
    {
        get => this.alignment;
        set
        {
            if (this.alignment == value)
            {
                return;
            }

            this.alignment = value;
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the unscaled location of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point UnscaledLocation
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.unscaledLocation;
        }

        set
        {
            if (this.unscaledLocation == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.UnscaledLocation)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            this.unscaledLocation = value;
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the unscaled size of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point UnscaledSize
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.unscaledSize;
        }

        set
        {
            if (this.unscaledSize == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.UnscaledSize)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.UnscaledSize)} cannot have negative components.");
            }

            this.unscaledSize = value;
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the scaled location of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point ScaledLocation
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.scaledLocation;
        }

        set
        {
            if (this.scaledLocation == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.ScaledLocation)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            this.unscaledLocation = value.Unscale(ScreenController.Scale);
            this.IsRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the scaled size of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point ScaledSize
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.scaledSize;
        }

        set
        {
            if (this.scaledSize == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.ScaledSize)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            if (value.X < 0 || value.Y < 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.ScaledSize)} cannot have negative components.");
            }

            this.unscaledSize = value.Unscale(ScreenController.Scale);
            this.IsRecalculationNeeded = true;
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
            this.RecalculateIfNeeded();
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
            this.RecalculateIfNeeded();
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
            Type = TransformType.Absolute,
            unscaledLocation = new Point(0, 0),
            unscaledSize = ScreenController.DefaultSize,
        };
    }

    /// <summary>
    /// Sets the <see cref="RelativeOffset"/> property using a scaled absolute offset.
    /// </summary>
    /// <param name="x">The scaled absolute X offset. Default is null.</param>
    /// <param name="y">The scaled absolute Y offset. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative offset of the component based
    /// on a scaled absolute offset and a reference size. If the component
    /// has a parent, the reference size is the scaled size of the parent
    /// component. Otherwise, it's the current screen size
    /// (<see cref="ScreenController.CurrentSize"/>).
    /// </para>
    /// <para>
    /// The relative offset is calculated by dividing
    /// the scaled absolute offset by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative offset is not changed.</para>
    /// </remarks>
    public void SetRelativeOffsetFromScaledAbsolute(float? x = null, float? y = null)
    {
        Point reference = this.Component.Parent is { } parent
            ? parent.Transform.ScaledSize
            : ScreenController.CurrentSize;

        this.RelativeOffset = new Vector2(
            x / reference.X ?? this.relativeOffset.X,
            y / reference.Y ?? this.relativeOffset.Y);
    }

    /// <summary>
    /// Sets the <see cref="RelativeOffset"/> property using an unscaled absolute offset.
    /// </summary>
    /// <param name="x">The unscaled absolute X offset. Default is null.</param>
    /// <param name="y">The unscaled absolute Y offset. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative offset of the component based
    /// on an unscaled absolute offset and a reference size. If the component
    /// has a parent, the reference size is the unscaled size of the parent
    /// component. Otherwise, it's the default screen size
    /// (<see cref="ScreenController.DefaultSize"/>).
    /// </para>
    /// <para>
    /// The relative offset is calculated by dividing
    /// the unscaled absolute offset by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative offset is not changed.</para>
    /// </remarks>
    public void SetRelativeOffsetFromUnscaledAbsolute(float? x = null, float? y = null)
    {
        Point reference = this.Component.Parent is { } parent
            ? parent.Transform.UnscaledSize
            : ScreenController.DefaultSize;

        this.RelativeOffset = new Vector2(
            x / reference.X ?? this.relativeOffset.X,
            y / reference.Y ?? this.relativeOffset.Y);
    }

    /// <summary>
    /// Sets the <see cref="RelativeSize"/> property using a scaled absolute size.
    /// </summary>
    /// <param name="x">The scaled absolute X size. Default is null.</param>
    /// <param name="y">The scaled absolute Y size. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative size of the component based
    /// on a scaled absolute size and a reference size. If the component
    /// has a parent, the reference size is the scaled size of the parent
    /// component. Otherwise, it's the current screen size
    /// (<see cref="ScreenController.CurrentSize"/>).
    /// </para>
    /// <para>
    /// The relative size is calculated by dividing
    /// the scaled absolute size by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative size is not changed.</para>
    /// </remarks>
    public void SetRelativeSizeFromScaledAbsolute(float? x = null, float? y = null)
    {
        Point reference = this.Component.Parent is { } parent
            ? parent.Transform.ScaledSize
            : ScreenController.CurrentSize;

        this.RelativeSize = new Vector2(
            x / reference.X ?? this.RelativeSize.X,
            y / reference.Y ?? this.RelativeSize.Y);
    }

    /// <summary>
    /// Sets the <see cref="RelativeSize"/> property using an unscaled absolute size.
    /// </summary>
    /// <param name="x">The unscaled absolute X size. Default is null.</param>
    /// <param name="y">The unscaled absolute Y size. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative size of the component based
    /// on an unscaled absolute size and a reference size. If the component
    /// has a parent, the reference size is the unscaled size of the parent
    /// component. Otherwise, it's the default screen size
    /// (<see cref="ScreenController.DefaultSize"/>).
    /// </para>
    /// <para>
    /// The relative size is calculated by dividing
    /// the unscaled absolute size by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative size is not changed.</para>
    /// </remarks>
    public void SetRelativeSizeFromUnscaledAbsolute(float? x = null, float? y = null)
    {
        Point reference = this.Component.Parent is { } parent
            ? parent.Transform.UnscaledSize
            : ScreenController.DefaultSize;

        this.RelativeSize = new Vector2(
            x / reference.X ?? this.RelativeSize.X,
            y / reference.Y ?? this.RelativeSize.Y);
    }

    /// <summary>
    /// Recalculates the component's transformation if needed.
    /// </summary>
    /// <param name="withChildren">
    /// Indicates whether to include children in the recalculation.
    /// </param>
    public void RecalculateIfNeeded(bool withChildren = true)
    {
        if (!this.IsRecalculationNeeded)
        {
            return;
        }

        if (withChildren)
        {
            this.RecalculateWithChildren();
        }
        else
        {
            this.Recalculate();
        }
    }

    private void Recalculate()
    {
        Point unscaledLocationBefore = this.unscaledLocation;
        Point unscaledSizeBefore = this.unscaledSize;

        switch (this.transformType)
        {
            case TransformType.Relative:
                this.RecalculateRelative();
                break;
            case TransformType.Absolute:
                this.RecalculateAbsolute();
                break;
        }

        this.scaledLocation = this.unscaledLocation
            .Scale(ScreenController.Scale);

        this.scaledSize = this.unscaledSize
            .Scale(ScreenController.Scale)
            .Clamp(this.MinSize, this.MaxSize);

        this.IsRecalculationNeeded = false;
        this.Recalculated?.Invoke(this, EventArgs.Empty);

        if (this.unscaledLocation != unscaledLocationBefore)
        {
            this.LocationChanged?.Invoke(
                this,
                new TransformElementChangedEventArgs<Point>(
                    unscaledLocationBefore,
                    this.unscaledLocation));
        }

        if (this.unscaledSize != unscaledSizeBefore)
        {
            this.SizeChanged?.Invoke(
                this,
                new TransformElementChangedEventArgs<Point>(
                    unscaledSizeBefore,
                    this.unscaledSize));
        }
    }

    private void RecalculateWithChildren()
    {
        this.Recalculate();
        foreach (UIComponent child in this.Component.Children.ToList())
        {
            child.Transform.RecalculateWithChildren();
        }
    }

    private void RecalculateRelative()
    {
        UITransform reference = this.Component.Parent!.Transform;

        if (reference.IsRecalculationNeeded)
        {
            reference.Recalculate();
        }

        this.unscaledLocation = reference.unscaledLocation;
        this.unscaledSize = reference.unscaledSize.Scale(this.relativeSize);

        this.RecalculateRatio();

        Rectangle sourceRect = reference.UnscaledRectangle;
        var currentRect = new Rectangle(this.unscaledLocation, this.unscaledSize);

        this.unscaledLocation = RecalculationUtils.AlignRectangle(
            sourceRect, currentRect, this.alignment).Location;

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

    private void ScreenController_ScreenChanged()
    {
        this.RecalculateWithChildren();
    }

    private void Component_ParentChanged(object? sender, ParentChangedEventArgs e)
    {
        if (e.OldParent is null)
        {
            ScreenController.ScreenChanged -= this.ScreenController_ScreenChanged;
        }

        if (e.NewParent is null)
        {
            ScreenController.ScreenChanged += this.ScreenController_ScreenChanged;
        }

        this.RecalculateWithChildren();
    }
}
