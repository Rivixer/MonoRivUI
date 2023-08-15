using System;
using Microsoft.Xna.Framework.Content;

namespace AnyPoly;

/// <summary>
/// A static class that provides access to the content manager.
/// </summary>
internal static class ContentController
{
    private static bool isInitialized;

    /// <summary>
    /// Gets the ContentManager instance provided by Monogame to load assets.
    /// </summary>
    public static ContentManager Content { get; private set; } = default!;

    /// <summary>
    /// Initializes the <see cref="ContentController"/> class.
    /// </summary>
    /// <param name="content">The content manager.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if class is already initialized.
    /// </exception>
    public static void Initialize(ContentManager content)
    {
        if (isInitialized)
        {
            throw new InvalidOperationException(
                "The ContentController class has already been initialized.");
        }

        Content = content;
        isInitialized = true;
    }
}
