using System;
using Microsoft.Xna.Framework.Graphics;

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
        isInitialized = true;
    }
}
