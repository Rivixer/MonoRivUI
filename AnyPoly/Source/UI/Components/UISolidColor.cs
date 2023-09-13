using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component that draws a solid color.
/// </summary>
internal class UISolidColor : UITextureComponent, IUIButtonContent<UISolidColor>
{
    private Color color;

    /// <summary>
    /// Initializes a new instance of the <see cref="UISolidColor"/> class.
    /// </summary>
    /// <param name="color">The color to be drawn.</param>
    public UISolidColor(Color color)
    {
        this.color = color;
        this.LoadTexture();
    }

    /// <summary>
    /// Gets or sets the color to be drawn.
    /// </summary>
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
    bool IUIButtonContent<UISolidColor>.IsButtonContentHovered(Point mousePosition)
    {
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the texture is already loaded, it will
    /// be disposed before loading a new one.
    /// </remarks>
    protected override void LoadTexture()
    {
        this.Texture?.Dispose();
        var texture = new Texture2D(AnyPolyGame.Instance.GraphicsDevice, 1, 1);
        texture.SetData(new Color[] { this.color });
        this.Texture = texture;
    }
}
