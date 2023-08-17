﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AnyPoly;

/// <summary>
/// A static class that provides method to interact with the mouse input.
/// </summary>
internal class MouseController
{
    private static MouseState previousState;
    private static MouseState currentState;

    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    public static Point Position => currentState.Position;

    /// <summary>
    /// Updates the mouse state.
    /// </summary>
    /// <remarks>
    /// This method should be called once per frame
    /// to keep the mouse state up to date.
    /// </remarks>
    public static void Update()
    {
        previousState = currentState;
        currentState = Mouse.GetState();
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
        return currentState.LeftButton == ButtonState.Pressed;
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
        return currentState.LeftButton == ButtonState.Released;
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
        return previousState.LeftButton == ButtonState.Pressed
            && currentState.LeftButton == ButtonState.Released;
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
        return currentState.RightButton == ButtonState.Pressed;
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
        return currentState.RightButton == ButtonState.Released;
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
        return previousState.RightButton == ButtonState.Pressed
            && currentState.RightButton == ButtonState.Released;
    }
}