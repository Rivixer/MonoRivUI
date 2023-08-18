using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// A class that provides access to transform properties
/// of a UI component from the <see cref="UIComponent"/> class.
/// </summary>
/// <remarks>
/// Mainly used to faciliate the creation of UI components.
/// For example:
/// <code>
/// <see langword="var"/> component = <see langword="new"/> <see cref="UIComponent"/>()
/// {
///     UnscaledLocation = <see langword="new"/> <see cref="Point"/>(10, 10),
///     UnscaledSize = <see langword="new"/> <see cref="Point"/>(100, 100),
/// };
/// </code>
/// The getters for the transform properties are made private to encourage
/// the use of the <see cref="Transform"/> property
/// for more controlled access.
/// </remarks>
internal abstract partial class UIComponent
{
    /// <inheritdoc cref="UITransform.TransformType"/>
    public TransformType TransformType
    {
        private get => this.Transform.TransformType;
        set => this.Transform.TransformType = value;
    }

    /// <inheritdoc cref="UITransform.RelativeSize"/>
    public Vector2 RelativeSize
    {
        private get => this.Transform.RelativeSize;
        set => this.Transform.RelativeSize = value;
    }

    /// <inheritdoc cref="UITransform.RelativeOffset"/>
    public Vector2 RelativeOffset
    {
        private get => this.Transform.RelativeOffset;
        set => this.Transform.RelativeOffset = value;
    }

    /// <inheritdoc cref="UITransform.MinSize"/>
    public Point MinSize
    {
        private get => this.Transform.MinSize;
        set => this.Transform.MinSize = value;
    }

    /// <inheritdoc cref="UITransform.MaxSize"/>
    public Point MaxSize
    {
        private get => this.Transform.MaxSize;
        set => this.Transform.MaxSize = value;
    }

    /// <inheritdoc cref="UITransform.Ratio"/>
    public Ratio Ratio
    {
        private get => this.Transform.Ratio;
        set => this.Transform.Ratio = value;
    }

    /// <inheritdoc cref="UITransform.Alignment"/>
    public Alignment Alignment
    {
        private get => this.Transform.Alignment;
        set => this.Transform.Alignment = value;
    }

    /// <inheritdoc cref="UITransform.UnscaledLocation"/>
    public Point UnscaledLocation
    {
        private get => this.Transform.UnscaledLocation;
        set => this.Transform.UnscaledLocation = value;
    }

    /// <inheritdoc cref="UITransform.UnscaledSize"/>
    public Point UnscaledSize
    {
        private get => this.Transform.UnscaledSize;
        set => this.Transform.UnscaledSize = value;
    }

    /// <inheritdoc cref="UITransform.ScaledLocation"/>
    public Point ScaledLocation
    {
        private get => this.Transform.ScaledLocation;
        set => this.Transform.ScaledLocation = value;
    }

    /// <inheritdoc cref="UITransform.ScaledSize"/>
    public Point ScaledSize
    {
        private get => this.Transform.ScaledSize;
        set => this.Transform.ScaledSize = value;
    }
}
