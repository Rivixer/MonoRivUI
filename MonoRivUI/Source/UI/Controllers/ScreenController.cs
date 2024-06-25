using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// A static class responsible for managing screen settings and changes.
/// </summary>
public static class ScreenController
{
    private static bool isInitialized;
    private static GraphicsDeviceManager graphicsDeviceManager = default!;

    /// <summary>
    /// An event raised when the screen settings have been changed.
    /// </summary>
    public static event EventHandler? ScreenChanged;

    /// <summary>
    /// Gets the GraphicDevice.
    /// </summary>
    public static GraphicsDevice GraphicsDevice => graphicsDeviceManager.GraphicsDevice;

    /// <summary>
    /// Gets the GameWindow.
    /// </summary>
    public static GameWindow GameWindow { get; private set; } = default!;

    /// <summary>
    /// Gets the default size the UI is designed for.
    /// </summary>
    public static Point DefaultSize { get; } = new Point(1920, 1080);

    /// <summary>
    /// Gets the scale factor of the current screen compared to <see cref="DefaultSize"/>.
    /// </summary>
    public static Vector2 Scale
        => new(Width / (float)DefaultSize.X, Height / (float)DefaultSize.Y);

    /// <summary>
    /// Gets the current width of the screen.
    /// </summary>
    public static int Width { get; private set; }

    /// <summary>
    /// Gets the current height of the screen.
    /// </summary>
    public static int Height { get; private set; }

    /// <summary>
    /// Gets the current screen type.
    /// </summary>
    public static ScreenType ScreenType { get; private set; }

    /// <summary>
    /// Gets the current size of the screen.
    /// </summary>
    public static Point CurrentSize => new(Width, Height);

    /// <summary>
    /// Initializes the <see cref="ScreenController"/> class.
    /// </summary>
    /// <param name="graphics">The GraphicsDeviceManager class provided by MonoGame.</param>
    /// <param name="window">The GameWindow class provided by MonoGame.</param>
    public static void Initialize(GraphicsDeviceManager graphics, GameWindow window)
    {
        if (isInitialized)
        {
            throw new InvalidOperationException(
                "The ScreenController class has already been initialized.");
        }

        GameWindow = window;
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
            if (graphicsDeviceManager.PreferredBackBufferWidth != 1366)
            {
                Change(1366, 768, ScreenType.Windowed);
            }
            else
            {
                Change(1920, 1080, ScreenType.Windowed);
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
            Width = width.Value;
        }

        if (height.HasValue)
        {
            Height = height.Value;
        }

        if (screenType.HasValue)
        {
            ScreenType = screenType.Value;
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
    /// This method will also invoke the <see cref="ScreenChanged"/> event.
    /// </para>
    /// </remarks>
    public static void ApplyChanges()
    {
        graphicsDeviceManager.PreferredBackBufferWidth = Width;
        graphicsDeviceManager.PreferredBackBufferHeight = Height;
        graphicsDeviceManager.IsFullScreen = ScreenType is ScreenType.FullScreen or ScreenType.Borderless;
        GameWindow.IsBorderless = ScreenType is ScreenType.Borderless;

        graphicsDeviceManager.ApplyChanges();
        ScreenChanged?.Invoke(null, EventArgs.Empty);
    }
}
