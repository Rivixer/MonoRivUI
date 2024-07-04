using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// A static class responsible for managing keyboard input.
/// </summary>
public static class KeyboardController
{
    private static KeyboardState previousState;
    private static KeyboardState currentState;

    /// <summary>
    /// Updates the keyboard state.
    /// </summary>
    /// <remarks>
    /// This method should be called once per frame
    /// to keep the keyboard state up to date.
    /// </remarks>
    public static void Update()
    {
        previousState = currentState;
        currentState = Keyboard.GetState();
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
        return currentState.IsKeyDown(key);
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
        return currentState.IsKeyUp(key);
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
        return previousState.IsKeyUp(key)
            && currentState.IsKeyDown(key);
    }

    /// <summary>
    /// Returns an enumerable collection of all keys that are currently down.
    /// </summary>
    /// <returns>An enumerable collection of all keys that are currently down.</returns>
    public static IEnumerable<Keys> GetPressedKeys()
    {
        return currentState.GetPressedKeys();
    }
}
