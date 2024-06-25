using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a read-only transformation of a UI component.
/// </summary>
public interface IUIReadOnlyTransform
{
    /// <summary>
    /// Gets the location of the component.
    /// </summary>
    public Point Location { get; }

    /// <summary>
    /// Gets the size of the component.
    /// </summary>
    public Point Size { get; }

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
    /// Gets the rectangle of the component.
    /// </summary>
    public Rectangle DestRectangle { get; }
}
