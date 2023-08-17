using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component that draws a solid color.
/// </summary>
internal class UISolidColor : UIComponent
{
    private Texture2D texture;
    private Color color;

    /// <summary>
    /// Initializes a new instance of the <see cref="UISolidColor"/> class.
    /// </summary>
    /// <param name="color">The color to be drawn.</param>
    public UISolidColor(Color color)
    {
        this.color = color;
        this.texture = this.LoadTexture();
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
            this.texture = this.LoadTexture();
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatchController.SpriteBatch.Draw(
            texture: this.texture,
            destinationRectangle: this.Transform.ScaledRectangle,
            color: Color.White);

        base.Draw(gameTime);
    }

    private Texture2D LoadTexture()
    {
        this.texture?.Dispose();
        var texture = new Texture2D(AnyPoly.Instance.GraphicsDevice, 1, 1);
        texture.SetData(new Color[] { this.color });
        return texture;
    }
}
