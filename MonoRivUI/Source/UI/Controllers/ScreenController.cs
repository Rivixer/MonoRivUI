using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// A static class responsible for managing screen settings and changes.
/// </summary>
public static class ScreenController
{
    private static readonly List<OverlayData<IOverlay>> DisplayedOverlaysData = new();

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
    /// Gets the currently displayed overlays.
    /// </summary>
    public static IEnumerable<OverlayData<IOverlay>> DisplayedOverlays => DisplayedOverlaysData;

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

    /// <summary>
    /// Updates the overlay scenes.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void UpdateOverlays(GameTime gameTime)
    {
        SortOverlayPriorities();

        var stack = new Stack<OverlayData<IOverlay>>();
        foreach (OverlayData<IOverlay> data in DisplayedOverlaysData.AsEnumerable().Reverse())
        {
            stack.Push(data);
            if (data.Options.BlockUpdateOnUnderlyingScenes)
            {
                break;
            }
        }

        while (stack.Count > 0)
        {
            IOverlay overlay = stack.Pop().Value;
            overlay.Update(gameTime);
        }
    }

    /// <summary>
    /// Shows an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to show.</param>
    /// <param name="options">The options for showing the overlay.</param>
    /// <remarks>
    /// If the overlay is already displayed, it will not be shown again.
    /// </remarks>
    public static void ShowOverlay(IOverlay overlay, OverlayShowOptions options = default)
    {
        if (!IsOverlayDisplayed(overlay))
        {
            (overlay as IOverlayComponent)?.OnShow();
            DisplayedOverlaysData.Add(new OverlayData<IOverlay>(overlay, options));
        }
    }

    /// <summary>
    /// Hides an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to hide.</param>
    /// <remarks>
    /// If the overlay is not displayed, nothing will happen.
    /// </remarks>
    public static void HideOverlay(IOverlay overlay)
    {
        int index = DisplayedOverlaysData.FindIndex(x => x.Value == overlay);
        if (index != -1)
        {
            DisplayedOverlaysData.RemoveAt(index);
            (overlay as IOverlayComponent)?.OnHide();
        }
    }

    /// <summary>
    /// Returns whether an overlay is currently displayed.
    /// </summary>
    /// <param name="overlay">The overlay to check if it is displayed.</param>
    /// <returns>
    /// <see langword="true"/> if the overlay is displayed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsOverlayDisplayed(IOverlay overlay)
    {
        return DisplayedOverlaysData.Any(x => x.Value == overlay);
    }

    /// <summary>
    /// Returns whether the focus is blocked by an overlay.
    /// </summary>
    /// <param name="currentOverlay">
    /// The current overlay to check if only the overlays
    /// with higher priority block the focus.
    /// If not specified, all overlays will be checked.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the focus is blocked by an overlay;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsFocusBlockedByOverlay(IOverlay? currentOverlay = null)
    {
        if (currentOverlay is null)
        {
            return DisplayedOverlaysData.Any(x => x.Options.BlockFocusOnUnderlyingScenes);
        }

        var data = DisplayedOverlaysData;
        data.Reverse();

        foreach (var overlayData in data)
        {
            if (overlayData.Value == currentOverlay)
            {
                break;
            }

            if (overlayData.Options.BlockFocusOnUnderlyingScenes)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Draws the overlays.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void DrawOverlays(GameTime gameTime)
    {
        SortOverlayPriorities();

        if (DisplayedOverlaysData.Count == 0)
        {
            return;
        }

        var stack = new Stack<OverlayData<IOverlay>>();
        foreach (OverlayData<IOverlay> data in DisplayedOverlaysData.AsEnumerable().Reverse())
        {
            stack.Push(data);
            if (data.Options.BlockDrawOnUnderlyingScenes)
            {
                break;
            }
        }

        while (stack.Count > 0)
        {
            IOverlay overlay = stack.Pop().Value;
            overlay.Draw(gameTime);

            if (overlay is IOverlayScene)
            {
                Component.DrawPriorityComponents(gameTime);
            }
        }
    }

    private static void SortOverlayPriorities()
    {
        DisplayedOverlaysData.Sort((a, b) => a.Value.Priority.CompareTo(b.Value.Priority));
    }
}
