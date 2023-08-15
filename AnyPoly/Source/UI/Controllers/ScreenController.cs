using System;
using AnyPoly.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AnyPoly;

/// <summary>
/// A static class responsible for managing screen settings and changes.
/// </summary>
internal static class ScreenController
{
    private static bool isInitialized;
    private static GraphicsDeviceManager graphicsDeviceManager = default!;

    /// <summary>
    /// An event raised when the screen settings have been changed.
    /// </summary>
    public static event Action? OnScreenChanged;

    /// <summary>
    /// Gets the default size the UI is designed for.
    /// </summary>
    public static Point DefaultSize { get; } = new Point(1920, 1080);

    /// <summary>
    /// Gets the scale factor of the current screen compared to <see cref="DefaultSize"/>.
    /// </summary>
    public static Vector2 Scale => new Vector2(
            graphicsDeviceManager.PreferredBackBufferWidth / (float)DefaultSize.X,
            graphicsDeviceManager.PreferredBackBufferHeight / (float)DefaultSize.Y);

    /// <summary>
    /// Initializes the <see cref="ScreenController"/> class.
    /// </summary>
    /// <param name="graphics">The GraphicsDeviceManager class provided by MonoGame.</param>
    public static void Initialize(GraphicsDeviceManager graphics)
    {
        if (isInitialized)
        {
            throw new InvalidOperationException(
                "The ScreenController class has already been initialized.");
        }

        graphicsDeviceManager = graphics;
        isInitialized = true;
    }

    /// <summary>
    /// Updates the ScreenController class.
    /// </summary>
    public static void Update()
    {
        if (KeyboardController.IsKeyHit(Keys.F11))
        {
            if (graphicsDeviceManager.IsFullScreen)
            {
                Change(1366, 768, ScreenType.Windowed);
            }
            else
            {
                Change(1920, 1080, ScreenType.FullScreen);
            }

            ApplyChanges();
        }
    }

    /// <summary>
    /// Changes the screen settings.
    /// </summary>
    /// <param name="width">The new width of the screen.</param>
    /// <param name="height">The new height of the screen.</param>
    /// <param name="screenType">The new sceen type.</param>
    /// <remarks>If a parameter is not specified, the corresponding setting will not be changed.</remarks>
    public static void Change(int? width = null, int? height = null, ScreenType? screenType = null)
    {
        if (width.HasValue)
        {
            graphicsDeviceManager.PreferredBackBufferWidth = width.Value;
        }

        if (height.HasValue)
        {
            graphicsDeviceManager.PreferredBackBufferHeight = height.Value;
        }

        if (screenType.HasValue)
        {
            graphicsDeviceManager.IsFullScreen = screenType is ScreenType.FullScreen or ScreenType.Borderless;
            AnyPoly.Instance.Window.IsBorderless = screenType is ScreenType.Borderless;
        }
    }

    /// <summary>
    /// Applies the changes made to the screen settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called after the
    /// <see cref="Change(int?, int?, ScreenType?)"/> method.
    /// </para>
    /// <para>
    /// This method will also invoke the <see cref="OnScreenChanged"/> event.
    /// </para>
    /// </remarks>
    public static void ApplyChanges()
    {
        graphicsDeviceManager.ApplyChanges();
        OnScreenChanged?.Invoke();
    }
}
