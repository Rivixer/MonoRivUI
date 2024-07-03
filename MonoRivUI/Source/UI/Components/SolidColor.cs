using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static MonoRivUI.Style;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component that draws a solid color.
/// </summary>
public class SolidColor : TextureComponent, IButtonContent<SolidColor>, IStyleable<SolidColor>
{
    private Color color;

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
        this.color = color;
        this.LoadTexture();
    }

    /// <summary>
    /// Gets or sets the color to be drawn.
    /// </summary>
    [Stylable]
    public Color Color
    {
        get => this.color;
        set
        {
            if (this.color == value)
            {
                return;
            }

            this.color = value;
            this.LoadTexture();
        }
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
    public SolidColor ApplyStyle(Style<SolidColor> style)
    {
        style.Apply(this);
        return this;
    }

    /// <inheritdoc/>
    public Style<SolidColor> GetStyle()
    {
        return new Style<SolidColor>()
        {
            CustomProperties = new List<Property>()
            {
                new("Color", this.Color),
            },
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the texture is already loaded, it will
    /// be disposed before loading a new one.
    /// </remarks>
    protected override void LoadTexture()
    {
        this.Texture?.Dispose();
        var texture = new Texture2D(ScreenController.GraphicsDevice, 1, 1);
        texture.SetData(new Color[] { this.color });
        this.Texture = texture;
    }
}
