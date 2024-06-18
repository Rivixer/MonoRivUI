using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components that draw a texture.
/// </summary>
internal abstract class TextureComponent : Component
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
    /// direction around its <see cref="Origin"/>. The angle
    /// is measured in radians, where 0 represents
    /// no rotation and values increase clockwise.
    /// </remarks>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the origin of the image.
    /// </summary>
    /// <remarks>
    /// The origin point determines the point around which
    /// the image will rotate. By default, the origin
    /// is set to the top-left corner (0,0) of the image.
    /// </remarks>
    public Vector2 Origin { get; set; } = Vector2.Zero;

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
        SpriteBatchController.SpriteBatch.Draw(
            texture: this.Texture,
            destinationRectangle: this.Transform.ScaledRectangle,
            sourceRectangle: null,
            color: Color.White * this.Opacity,
            rotation: this.Rotation,
            origin: this.Origin,
            effects: this.SpriteEffects,
            layerDepth: this.LayerDepth);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Loads the texture of the image.
    /// </summary>
    protected abstract void LoadTexture();
}
