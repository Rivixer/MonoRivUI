using AnyPoly.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#nullable disable

namespace AnyPoly;

/// <summary>
/// A static class responsible for managing screen settings and changes.
/// </summary>
internal static class ScreenController
{
    private static GraphicsDeviceManager graphicsDeviceManager;

    /// <summary>
    /// Represents the method that will handle screen change events.
    /// </summary>
    public delegate void OnScreenChangeEventHandler();

    /// <summary>
    /// An event raised when the screen settings change.
    /// </summary>
    public static event OnScreenChangeEventHandler OnScreenChange;

    /// <summary>
    /// Gets the default size the UI is designed for.
    /// </summary>
    public static Point DefaultSize { get; } = new Point(1920, 1080);

    /// <summary>
    /// Gets the GameWindow class provided by MonoGame.
    /// </summary>
    public static GameWindow GameWindow { get; private set; }

    /// <summary>
    /// Gets the GraphicsDevice class provided by MonoGame.
    /// </summary>
    public static GraphicsDevice GraphicsDevice => graphicsDeviceManager.GraphicsDevice;

    /// <summary>
    /// Gets the SpriteBatch class provided by MonoGame.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the scale factor of the current screen compared to <see cref="DefaultSize"/>.
    /// </summary>
    public static Vector2 Scale => new Vector2(
            graphicsDeviceManager.PreferredBackBufferWidth / (float)DefaultSize.X,
            graphicsDeviceManager.PreferredBackBufferHeight / (float)DefaultSize.Y);

    /// <summary>
    /// Initializes the ScreenController class.
    /// </summary>
    /// <param name="graphics">The GraphicsDeviceManager class provided by MonoGame.</param>
    /// <param name="spriteBatch">The SpriteBatch class provided by MonoGame.</param>
    /// <param name="gameWindow">The GameWindow class provided by MonoGame.</param>
    public static void Initialize(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, GameWindow gameWindow)
    {
        graphicsDeviceManager = graphics;
        SpriteBatch = spriteBatch;
        GameWindow = gameWindow;
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
            GameWindow.IsBorderless = screenType is ScreenType.Borderless;
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
    /// This method will also invoke the <see cref="OnScreenChange"/> event.
    /// </para>
    /// </remarks>
    public static void ApplyChanges()
    {
        graphicsDeviceManager.ApplyChanges();
        OnScreenChange?.Invoke();
    }
}
