using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a transformation for a UI component.
/// </summary>
/// <remarks>
/// It is responsible for positioning and sizing the component.
/// </remarks>
public class Transform
{
    private Point location;
    private Point size;

    private TransformType transformType;

    private Point minSize = new(1);
    private Point maxSize = new(int.MaxValue);
    private Ratio ratio = Ratio.Unspecified;

    private Vector2 relativeOffset = Vector2.Zero;
    private Vector2 relativeSize = Vector2.One;
    private Vector4 padding = Vector4.Zero;
    private Alignment alignment = Alignment.TopLeft;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transform"/> class.
    /// </summary>
    /// <param name="component">The component associated with this transformation.</param>
    public Transform(Component component)
    {
        this.Component = component;
        component.ParentChanged += this.Component_ParentChanged;

        if (component.Parent is null)
        {
            ScreenController.ScreenChanged += this.ScreenController_ScreenChanged;
        }
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
    public Component Component { get; private set; }

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
            this.isRecalculationNeeded = true;
        }
    }

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

            if (value.X <= 0 || value.Y <= 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.RelativeSize)} cannot have non-positive components.");
            }

            this.relativeSize = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative offset of the component.
    /// </summary>
    /// <remarks>
    /// It is effective only when <see cref="Type"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </remarks>
    /// <seealso cref="SetRelativeOffsetFromAbsolute(float?, float?)"/>
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
            this.isRecalculationNeeded = true;
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

            if (value.X <= 0 || value.Y <= 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.MinSize)} cannot have non-positive components.");
            }

            this.minSize = value;
            this.isRecalculationNeeded = true;
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
            this.isRecalculationNeeded = true;
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
            this.isRecalculationNeeded = true;
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
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the relative padding of the component.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is effective only when the child component's <see cref="Type"/>
    /// is set to <see cref="TransformType.Relative"/>.
    /// </para>
    /// <para>
    /// The padding values are relative to the size of this component.
    /// </para>
    /// <para>
    /// The <see cref="Vector4"/> components are interpreted as follows:
    /// <list type="bullet">
    /// <item>X: left padding</item>
    /// <item>Y: top padding</item>
    /// <item>Z: right padding</item>
    /// <item>W: bottom padding</item>
    /// </list>
    /// </para>
    /// </remarks>
    public Vector4 RelativePadding
    {
        get => this.padding;
        set
        {
            if (this.padding == value)
            {
                return;
            }

            this.padding = value;

            foreach (Component component in this.Component.Children)
            {
                component.Transform.isRecalculationNeeded = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the parent padding should be ignored.
    /// </summary>
    public bool IgnoreParentPadding { get; set; }

    /// <summary>
    /// Gets or sets the location of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point Location
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.location;
        }

        set
        {
            if (this.location == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.Location)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            this.location = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the size of the component.
    /// </summary>
    /// <remarks>
    /// Setting this property is effective only when
    /// <see cref="Type"/> is set to <see cref="TransformType.Absolute"/>.
    /// Otherwise it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public Point Size
    {
        get
        {
            this.RecalculateIfNeeded();
            return this.size;
        }

        set
        {
            if (this.size == value)
            {
                return;
            }

            if (this.Type is not TransformType.Absolute)
            {
                throw new InvalidOperationException(
                    $"Cannot set {nameof(this.Size)} " +
                    $"when {nameof(this.Type)} " +
                    $"is not {TransformType.Absolute}.");
            }

            if (value.X <= 0 || value.Y <= 0)
            {
                throw new ArgumentException(
                    $"{nameof(this.Size)} cannot have non-positive components.");
            }

            this.size = value.Clamp(this.minSize, this.maxSize);
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets the destination rectangle of the component.
    /// </summary>
    public Rectangle DestRectangle
    {
        get
        {
            this.RecalculateIfNeeded();
            return new Rectangle(this.location, this.size);
        }
    }

    /// <summary>
    /// Creates a new <see cref="Transform"/> with default values.
    /// </summary>
    /// <param name="component">
    /// The component associated with the transformation.
    /// </param>
    /// <returns>The new <see cref="Transform"/> with default values.</returns>
    public static Transform Default(Component component)
    {
        return new Transform(component)
        {
            Type = TransformType.Absolute,
            location = new Point(0, 0),
            size = ScreenController.DefaultSize,
        };
    }

    /// <summary>
    /// Sets the <see cref="RelativeOffset"/> property using an absolute offset.
    /// </summary>
    /// <param name="x">The absolute X offset. Default is null.</param>
    /// <param name="y">The absolute Y offset. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative offset of the component based
    /// on an absolute offset and a reference as parent size.
    /// </para>
    /// <para>
    /// The relative offset is calculated by dividing
    /// the absolute offset by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative offset is not changed.</para>
    /// </remarks>
    public void SetRelativeOffsetFromAbsolute(float? x = null, float? y = null)
    {
        this.RecalculateFromRootIfNeeded();

        // While recalucating, the parent may have changed.
        if (this.Component.Parent is null)
        {
            return;
        }

        Point reference = this.Component.Parent!.Transform.Size;
        this.RelativeOffset = new Vector2(
            x / reference.X ?? this.relativeOffset.X,
            y / reference.Y ?? this.relativeOffset.Y);
    }

    /// <inheritdoc cref="SetRelativeOffsetFromAbsolute(float?, float?)"/>
    /// <param name="offset">The absolute offset.</param>
    public void SetRelativeOffsetFromAbsolute(Point offset)
    {
        this.SetRelativeOffsetFromAbsolute(offset.X, offset.Y);
    }

    /// <summary>
    /// Sets the <see cref="RelativeSize"/> property using an absolute size.
    /// </summary>
    /// <param name="x">The absolute X size. Default is null.</param>
    /// <param name="y">The absolute Y size. Default is null.</param>
    /// <remarks>
    /// <para>
    /// This method calculates the relative size of the component based
    /// on an absolute size and a reference as parent size.
    /// </para>
    /// <para>
    /// The relative size is calculated by dividing
    /// the absolute size by the reference size.
    /// </para>
    /// <para>If the parameter is null, the relative size is not changed.</para>
    /// </remarks>
    public void SetRelativeSizeFromAbsolute(float? x = null, float? y = null)
    {
        this.RecalculateFromRootIfNeeded();

        // While recalucating, the parent may have changed.
        if (this.Component.Parent is null)
        {
            return;
        }

        Point reference = this.Component.Parent!.Transform.Size;
        this.RelativeSize = new Vector2(
            x / reference.X ?? this.RelativeSize.X,
            y / reference.Y ?? this.RelativeSize.Y);

    }

    /// <inheritdoc cref="SetRelativeSizeFromAbsolute(float?, float?)"/>
    /// <param name="size">The absolute size.</param>
    public void SetRelativeSizeFromAbsolute(Point size)
    {
        this.SetRelativeSizeFromAbsolute(size.X, size.Y);
    }

    /// <summary>
    /// Forces the recalculation of the component's transformation.
    /// </summary>
    /// <param name="withChildren">Indicates whether to include children in the recalculation.</param>
    internal void ForceRecalulcation(bool withChildren = true)
    {
        if (withChildren)
        {
            this.RecalculateWithChildren();
        }
        else
        {
            this.Recalculate();
        }
    }

    /// <summary>
    /// Recalculates the component's transformation if needed.
    /// </summary>
    /// <param name="withChildren">Indicates whether to include children in the recalculation.</param>
    internal void RecalculateIfNeeded(bool withChildren = true)
    {
        if (!this.isRecalculationNeeded)
        {
            return;
        }

        this.ForceRecalulcation(withChildren);
    }

    private void Recalculate()
    {
        Point locationBefore = this.location;
        Point sizeBefore = this.size;

        switch (this.transformType)
        {
            case TransformType.Relative:
                this.RecalculateRelative();
                break;
            case TransformType.Absolute:
                this.RecalculateAbsolute();
                break;
        }

        this.isRecalculationNeeded = false;
        this.Recalculated?.Invoke(this, EventArgs.Empty);

        if (this.location != locationBefore)
        {
            this.LocationChanged?.Invoke(
                this,
                new TransformElementChangedEventArgs<Point>(
                    locationBefore,
                    this.location));
        }

        if (this.size != sizeBefore)
        {
            this.SizeChanged?.Invoke(
                this,
                new TransformElementChangedEventArgs<Point>(
                    sizeBefore,
                    this.size));
        }
    }

    private void RecalculateWithChildren()
    {
        this.Recalculate();
        foreach (Component child in this.Component.Children.ToList())
        {
            child.Transform.RecalculateWithChildren();
        }
    }

    private void RecalculateFromRootIfNeeded()
    {
        Stack<Transform> transforms = new();

        Transform current = this;
        while (current.Component.Parent is not null)
        {
            transforms.Push(current);
            current = (Transform)current.Component.Parent!.Transform;
        }

        while (transforms.Count > 0)
        {
            var transform = transforms.Pop();
            if (transform.isRecalculationNeeded)
            {
                // RecalculateWithChildren will also recalculate the children,
                // so we can clear the stack (to remove references for GC)
                // and break the loop.
                transforms.Clear();
                transform.RecalculateWithChildren();
                break;
            }
        }
    }

    private void RecalculateRelative()
    {
        var reference = (Transform)this.Component.Parent!.Transform;

        if (reference.isRecalculationNeeded)
        {
            reference.Recalculate();
        }

        this.location = reference.location;
        this.size = reference.size.Scale(this.relativeSize);

        this.RecalculateRatio();

        this.size = this.size.Clamp(this.minSize, this.maxSize);

        Rectangle sourceRect = reference.DestRectangle;
        if (!this.IgnoreParentPadding && reference.padding != Vector4.Zero)
        {
            Point referenceSize = reference.size;

            int paddingLeft = (int)(reference.RelativePadding.X * referenceSize.X);
            int paddingTop = (int)(reference.RelativePadding.Y * referenceSize.Y);
            int paddingRight = (int)(reference.RelativePadding.Z * referenceSize.X);
            int paddingBottom = (int)(reference.RelativePadding.W * referenceSize.Y);

            sourceRect.X += paddingLeft;
            sourceRect.Y += paddingTop;
            sourceRect.Width -= paddingLeft + paddingRight;
            sourceRect.Height -= paddingTop + paddingBottom;
            this.size.X -= paddingLeft + paddingRight;
            this.size.Y -= paddingTop + paddingBottom;
        }

        sourceRect.Size = sourceRect.Size;

        if (this.Component.Parent is null)
        {
            sourceRect.Size = sourceRect.Size.Scale(ScreenController.Scale);
        }

        var currentRect = new Rectangle(this.location, this.size);

        this.location = RecalculationUtils.AlignRectangle(
            sourceRect, currentRect, this.alignment).Location;

        this.location += reference.size.Scale(this.relativeOffset);
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

        var currentRatio = this.size.ToRatio();
        if (currentRatio == this.ratio)
        {
            return;
        }

        Point size = this.size;
        bool heightIsOversized = currentRatio.ToFloat() < this.ratio.ToFloat();
        if (heightIsOversized)
        {
            size.Y = (int)(size.X / this.ratio.ToFloat());
        }
        else
        {
            size.X = (int)(size.Y * this.ratio.ToFloat());
        }

        this.size = size;
    }

    private void ScreenController_ScreenChanged(object? sender, EventArgs args)
    {
        this.RecalculateWithChildren();
    }

    private void Component_ParentChanged(object? sender, ParentChangedEventArgs e)
    {
        if (e.Previous is null)
        {
            ScreenController.ScreenChanged -= this.ScreenController_ScreenChanged;
        }

        if (e.Current is not null)
        {
            this.isRecalculationNeeded = true;
        }
    }
}
