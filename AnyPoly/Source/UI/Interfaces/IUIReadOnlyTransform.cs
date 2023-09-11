using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a read-only transformation of a UI component.
/// </summary>
internal interface IUIReadOnlyTransform
{
    /// <summary>
    /// Gets the unscaled location of the component.
    /// </summary>
    public Point UnscaledLocation { get; }

    /// <summary>
    /// Gets the unscaled size of the component.
    /// </summary>
    public Point UnscaledSize { get; }

    /// <summary>
    /// Gets the scaled location of the component.
    /// </summary>
    public Point ScaledLocation { get; }

    /// <summary>
    /// Gets the scaled size of the component.
    /// </summary>
    public Point ScaledSize { get; }

    /// <summary>
    /// Gets the type of the transformation.
    /// </summary>
    public TransformType Type { get; }

    /// <summary>
    /// Gets the minimum size of the component.
    /// </summary>
    public Point MinSize { get; }

    /// <summary>
    /// Gets the maximum size of the component.
    /// </summary>
    public Point MaxSize { get; }

    /// <summary>
    /// Gets the ratio of the component.
    /// </summary>
    public Ratio Ratio { get; }

    /// <summary>
    /// Gets the relative offset of the component.
    /// </summary>
    public Vector2 RelativeOffset { get; }

    /// <summary>
    /// Gets the relative size of the component.
    /// </summary>
    public Vector2 RelativeSize { get; }

    /// <summary>
    /// Gets the alignment of the component.
    /// </summary>
    public Alignment Alignment { get; }

    /// <summary>
    /// Gets the padding of the component.
    /// </summary>
    public Vector4 RelativePadding { get; }

    /// <summary>
    /// Gets a value indicating whether
    /// the parent padding should be ignored.
    /// </summary>
    public bool IgnoreParentPadding { get; }

    /// <summary>
    /// Gets the unscaled rectangle of the component.
    /// </summary>
    public Rectangle UnscaledRectangle { get; }

    /// <summary>
    /// Gets the scaled rectangle of the component.
    /// </summary>
    public Rectangle ScaledRectangle { get; }
}
