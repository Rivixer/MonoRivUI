using Microsoft.Xna.Framework.Input;

namespace AnyPoly;

/// <summary>
/// A static class that provides method to interact with the mouse input.
/// </summary>
internal class MouseController
{
    private static MouseState previousMouseState;
    private static MouseState currentMouseState;

    /// <summary>
    /// Updates the mouse state.
    /// </summary>
    /// <remarks>
    /// This method should be called once per frame
    /// to keep the mouse state up to date.
    /// </remarks>
    public static void Update()
    {
        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();
    }

    /// <summary>
    /// Checks if the left mouse button is currently pressed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button is pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonPressed()
    {
        return currentMouseState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the left mouse button is currently released.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button is released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonReleased()
    {
        return currentMouseState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the left mouse button has been clicked.
    /// </summary>
    /// <remarks>
    /// The button has been clicked if pressed and
    /// then released since the last frame.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if the left mouse button has been clicked;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonClicked()
    {
        return previousMouseState.LeftButton == ButtonState.Pressed
            && currentMouseState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the right mouse button is currently pressed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button is pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRightButtonPressed()
    {
        return currentMouseState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the right mouse button is currently released.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button is released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRightButtonReleased()
    {
        return currentMouseState.RightButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the right mouse button has been clicked.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button has been clicked;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The button has been clicked if it was pressed
    /// then released from the last frame.
    /// </remarks>
    public static bool IsRightButtonClicked()
    {
        return previousMouseState.RightButton == ButtonState.Pressed
            && currentMouseState.RightButton == ButtonState.Released;
    }
}
