using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components that draw a texture.
/// </summary>
public abstract class TextureComponent : Component
{
    private Texture2D? texture;

    /// <summary>
    /// Gets or sets the texture of the image.
    /// </summary>
    public Texture2D Texture
    {
        get => this.texture!;
        protected set => this.texture = value;
    }

    /// <summary>
    /// Gets or sets the color of the image.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the opacity of the image.
    /// </summary>
    /// <remarks>
    /// Should be a value between 0.0f and 1.0f.
    /// </remarks>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the rotation of the image.
    /// </summary>
    /// <remarks>
    /// The rotation is applied to the image in a clockwise
    /// direction around its origin (see <see cref="RelativeOrigin"/>).
    /// The angle is measured in radians, where 0 represents
    /// no rotation and values increase clockwise.
    /// </remarks>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the relative origin of the image.
    /// </summary>
    public Vector2 RelativeOrigin { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets a value indicating whether the origin of the image is centered.
    /// </summary>
    public bool CenterOrigin { get; set; }

    /// <summary>
    /// Gets or sets the sprite effects applied to the image.
    /// </summary>
    public SpriteEffects SpriteEffects { get; set; } = SpriteEffects.None;

    /// <summary>
    /// Gets or sets the depth layer at which the image is drawn.
    /// </summary>
    public float LayerDepth { get; set; }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        SpriteBatchController.SpriteBatch.Draw(
            texture: this.Texture,
            destinationRectangle: this.GetDestRectWithOriginOffset(),
            sourceRectangle: null,
            color: this.Color * this.Opacity,
            rotation: this.Rotation,
            origin: this.Transform.Size.ToVector2() * this.RelativeOrigin,
            effects: this.SpriteEffects,
            layerDepth: this.LayerDepth);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Loads the texture of the image.
    /// </summary>
    protected abstract void LoadTexture();

    private Rectangle GetDestRectWithOriginOffset()
    {
        if (!this.CenterOrigin)
        {
            return this.Transform.DestRectangle;
        }

        var destRect = this.Transform.DestRectangle;
        var originOffset = this.Transform.Size.ToVector2() * this.RelativeOrigin;

        return new Rectangle(
            destRect.X + (int)originOffset.X,
            destRect.Y + (int)originOffset.Y,
            destRect.Width,
            destRect.Height);
    }
}
