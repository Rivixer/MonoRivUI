using System;
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
    /// Gets or sets the source rectangle of the image.
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the destination
    /// rectangle should be adjusted to the source rectangle.
    /// </summary>
    public bool MatchDestinationToSource { get; set; }

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

    /// <summary>
    /// Gets the ratio of the texture.
    /// </summary>
    public Ratio TextureRatio => new(this.Texture.Width, this.Texture.Height);

    /// <summary>
    /// Gets or sets a value indicating whether the texture is loaded.
    /// </summary>
    public bool IsLoaded { get; protected set; }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (!this.IsLoaded)
        {
            throw new InvalidOperationException("The texture is not loaded.");
        }

        SpriteBatchController.SpriteBatch.Draw(
            texture: this.Texture,
            destinationRectangle: this.CalculateDestinationRectangle(),
            sourceRectangle: this.SourceRect,
            color: this.Color * this.Opacity,
            rotation: this.Rotation,
            origin: this.Transform.Size.ToVector2() * this.RelativeOrigin,
            effects: this.SpriteEffects,
            layerDepth: this.LayerDepth);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Loads the texture.
    /// </summary>
    public abstract void Load();

    private Rectangle CalculateDestinationRectangle()
    {
        Rectangle rectangle = this.Transform.DestRectangle;

        if (this.CenterOrigin)
        {
            var originOffset = this.Transform.Size.ToVector2() * this.RelativeOrigin;
            rectangle.X += (int)originOffset.X;
            rectangle.Y += (int)originOffset.Y;
        }

        if (this.MatchDestinationToSource && this.SourceRect is not null)
        {
            rectangle.X += this.SourceRect.Value.X;
            rectangle.Y += this.SourceRect.Value.Y;
            rectangle.Width = this.SourceRect!.Value.Width;
            rectangle.Height = this.SourceRect.Value.Height;
        }

        return rectangle;
    }
}
