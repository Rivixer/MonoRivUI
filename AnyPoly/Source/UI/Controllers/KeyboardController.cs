using Microsoft.Xna.Framework.Input;

namespace AnyPoly;

/// <summary>
/// A static class responsible for managing keyboard input.
/// </summary>
internal class KeyboardController
{
    private static KeyboardState previousKeyboard;
    private static KeyboardState currentKeyboard;

    /// <summary>
    /// Updates the keyboard state.
    /// </summary>
    /// <remarks>
    /// This method should be called once per frame
    /// to keep the keyboard state up to date.
    /// </remarks>
    public static void Update()
    {
        previousKeyboard = currentKeyboard;
        currentKeyboard = Keyboard.GetState();
    }

    /// <summary>
    /// Checks if the specified key is currently down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="true"/> if the key is down;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsKeyDown(Keys key)
    {
        return currentKeyboard.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if the specified key is currently up.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="true"/> if the key is up;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsKeyUp(Keys key)
    {
        return currentKeyboard.IsKeyUp(key);
    }

    /// <summary>
    /// Checks if the specified key has been hit.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="true"/> if the key has been hit;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The key has been hit if it was up in the previous frame
    /// and is down in the current frame.
    /// </remarks>
    public static bool IsKeyHit(Keys key)
    {
        return previousKeyboard.IsKeyUp(key)
            && currentKeyboard.IsKeyDown(key);
    }
}
