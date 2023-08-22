using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnyPoly;

/// <summary>
/// A static class that provides access to the sprite batch.
/// </summary>
internal static class SpriteBatchController
{
    private static bool isInitialized;

    /// <summary>
    /// Gets the SpriteBatch instance provided by Monogame to draw sprites.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; } = default!;

    /// <summary>
    /// Gets a white pixel texture.
    /// </summary>
    public static Texture2D WhitePixel { get; private set; } = default!;

    /// <summary>
    /// Initializes the <see cref="SpriteBatchController"/> class.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch class provided by Monogame.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the class is already initialized.
    /// </exception>
    public static void Initialize(SpriteBatch spriteBatch)
    {
        if (isInitialized)
        {
            throw new InvalidOperationException(
                "SpriteBatchController has already been initialized.");
        }

        SpriteBatch = spriteBatch;

        WhitePixel = new Texture2D(AnyPolyGame.Instance.GraphicsDevice, 1, 1);
        WhitePixel.SetData(new[] { Color.White });

        isInitialized = true;
    }
}
