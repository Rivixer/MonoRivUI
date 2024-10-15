using Microsoft.Xna.Framework;
using static MonoRivUI.Style;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component that draws a solid color.
/// </summary>
public class SolidColor : TextureComponent, IButtonContent<SolidColor>, IStyleable<SolidColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColor"/> class.
    /// </summary>
    /// <remarks>
    /// The solid color is white.
    /// </remarks>
    public SolidColor()
        : this(Color.White)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColor"/> class.
    /// </summary>
    /// <param name="color">The color to be drawn.</param>
    public SolidColor(Color color)
    {
        this.Color = color;
    }

    /// <summary>
    /// Gets or sets the color to be drawn.
    /// </summary>
    [Stylable]
    public new Color Color
    {
        get => base.Color;
        set => base.Color = value;
    }

    /// <remarks>
    /// <para>The solid color is always hovered.</para>
    /// <inheritdoc/>
    /// </remarks>
    /// <inheritdoc/>
    bool IButtonContent<SolidColor>.IsButtonContentHovered(Point mousePosition)
    {
        return true;
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<SolidColor> style)
    {
        style.Apply(this);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the texture is already loaded, it will
    /// be disposed before loading a new one.
    /// </remarks>
    public override void Load()
    {
        this.Texture = SpriteBatchController.WhitePixel;
        this.IsLoaded = true;
    }
}
