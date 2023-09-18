using AnyPoly.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using System.Text;

namespace AnyPoly;

/// <summary>
/// A static class representing a debug console.
/// </summary>
internal static partial class DebugConsole
{
    private static readonly Color DefaultTextColor = Color.White;

    private static UIFrame baseFrame = default!;
    private static UIListBox messages = default!;
    private static UITextInput textInput = default!;

    private static bool openOnError;

    private static bool isInitialized;

    /// <summary>
    /// Delegate representing the event handler for the console open or close event.
    /// </summary>
    public delegate void OpenCloseEventHandler();

    /// <summary>
    /// An event raised when the console has been opened.
    /// </summary>
    public static event OpenCloseEventHandler? Opened;

    /// <summary>
    /// An event raised when the console has been closed.
    /// </summary>
    public static event OpenCloseEventHandler? Closed;

    /// <summary>
    /// Gets the base frame component of the console.
    /// </summary>
    public static IUIReadOnlyComponent BaseFrame => baseFrame;

    /// <summary>
    /// Gets a value indicating whether the console is open.
    /// </summary>
    public static bool IsOpen { get; private set; }

    /// <summary>
    /// Initializes the debug console.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The debug console is already initialized.
    /// </exception>
    public static void Initialize()
    {
        if (isInitialized)
        {
            throw new InvalidOperationException("Debug console is already initialized.");
        }

        baseFrame = new UIFrame(Color.Black, thickness: 2)
        {
            Transform =
            {
                UnscaledLocation = new Point(60, 60),
                UnscaledSize = new Point(1500, 700),
            },
        };

        // Background
        _ = new UISolidColor(Color.Black * 0.9f) { Parent = baseFrame.InnerContainer };

        // Info text
        {
            var text = new UIText(Color.CornflowerBlue)
            {
                Parent = baseFrame.InnerContainer,
                Text = "DEBUG CONSOLE \t (PRESS [CTRL+`] TO TOGGLE VISIBILITY)",
                TextFit = TextFit.Both,
                Transform =
                {
                    Alignment = Alignment.TopLeft,
                    RelativeOffset = new Vector2(0.005f),
                    RelativeSize = new Vector2(0.95f, 0.031f),
                },
            };
        }

        // Close button
        {
            var closeButton = new UIButton<UISolidColor>(new UISolidColor(Color.DarkRed))
            {
                Parent = baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.TopRight,
                    RelativeSize = new Vector2(0.04f),
                    RelativeOffset = new Vector2(-0.003f, 0.005f),
                    Ratio = new Ratio(1, 1),
                },
            };

            var text = new UIText(Color.White)
            {
                Parent = closeButton,
                Text = "X",
                TextFit = TextFit.Both,
                TextAlignment = Alignment.Center,
                Scale = 0.9f,
                Transform =
                {
                    RelativeOffset = new Vector2(-0.05f, -0.06f),
                },
            };

            closeButton.Clicked += (s, e) => Close();
            closeButton.HoverEntered += (s, e) => e.Component.Color = Color.Red;
            closeButton.HoverExited += (s, e) => e.Component.Color = Color.DarkRed;
        }

        // Messages
        {
            var frame = new UIFrame(new Color(60, 60, 60, 255), thickness: 2)
            {
                Parent = baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.Top,
                    RelativeSize = new Vector2(0.995f, 0.89f),
                    RelativeOffset = new Vector2(0.0f, 0.05f),
                },
            };

            var background = new UISolidColor(Color.Gray * 0.15f)
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            messages = new UIListBox()
            {
                Parent = frame.InnerContainer,
                Orientation = Orientation.Vertical,
                Spacing = 6,
                IsScrollable = true,
                ScrollBarFrameColor = Color.Gray,
                ScrollBarThumbColor = Color.DarkGray,
                ScrollBarRelativeSize = 0.015f,
                Transform = { RelativePadding = new Vector4(0.005f) },
            };
        }

        // Text input
        {
            var frame = new UIFrame(new Color(60, 60, 60, 255), thickness: 2)
            {
                Parent = baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.Bottom,
                    RelativeSize = new Vector2(0.995f, 0.045f),
                    RelativeOffset = new Vector2(0.0f, -0.01f),
                    RelativePadding = new Vector4(0.005f, 0.0f, 0.005f, 0.0f),
                },
            };

            var background = new UISolidColor(Color.Gray * 0.5f)
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            textInput = new UITextInput(Color.White, caretColor: Color.Black)
            {
                Parent = frame.InnerContainer,
                Transform = { Alignment = Alignment.Left },
                TextAlignment = Alignment.Left,
                Placeholder = "Enter command...",
                PlaceholderOpacity = 0.3f,
                Scale = 0.8f,
                ClearAfterSend = true,
                DeselectAfterSend = false,
            };

            Opened += () => textInput.Select();
            Closed += () => textInput.Deselect();

            textInput.TextInputSent += TextInput_TextInputSent;
        }

#if DEBUG
        openOnError = true;
#else
        openOnError = false;
#endif

        isInitialized = true;
    }

    /// <summary>
    /// Opens the debug console.
    /// </summary>
    public static void Open()
    {
        IsOpen = true;
        Opened?.Invoke();
    }

    /// <summary>
    /// Closes the debug console.
    /// </summary>
    public static void Close()
    {
        IsOpen = false;
        Closed?.Invoke();
    }

    /// <summary>
    /// Updates the debug console.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void Update(GameTime gameTime)
    {
        if (!isInitialized)
        {
            return;
        }

        baseFrame.Update(gameTime);

        if (KeyboardController.IsKeyHit(Keys.OemTilde)
            && KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    /// <summary>
    /// Draws the debug console if opened.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void Draw(GameTime gameTime)
    {
        if (!isInitialized)
        {
            return;
        }

        if (IsOpen)
        {
            baseFrame.Draw(gameTime);
        }
    }

    /// <summary>
    /// Sends a message to the debug console.
    /// </summary>
    /// <param name="text">The text to be sent.</param>
    /// <param name="color">The color of the message text.</param>
    public static void SendMessage(string text, Color? color = null)
    {
        string currentTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        _ = new UIWrappedText(color ?? DefaultTextColor)
        {
            Parent = messages.ContentContainer,
            Text = $"[{currentTime}] {text}",
            Scale = 0.26f,
            AdjustSizeToText = true,
        };
    }

    /// <summary>
    /// Sends a warning message to the debug console.
    /// </summary>
    /// <param name="message">The warning message to be sent.</param>
    /// <remarks>The message will be colored yellow.</remarks>
    public static void SendWarning(string message)
    {
        SendMessage(message, Color.Yellow);
    }

    /// <summary>
    /// Throws an error message to the debug console.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="message">An optional additional message.</param>
    /// <remarks>The message will be colored red.</remarks>
    public static void ThrowError(Exception ex, string? message = null)
    {
        var stringBuilder = new StringBuilder();

        if (message is not null)
        {
            _ = stringBuilder.AppendLine(message);
        }

        _ = stringBuilder
            .AppendLine(ex.Message)
            .Append(ex.StackTrace);

        SendMessage(stringBuilder.ToString(), Color.Red);

        if (openOnError)
        {
            Open();
        }
    }

    private static void TextInput_TextInputSent(object? sender, UI.TextInputEventArgs e)
    {
        SendMessage(">" + e.Text, Color.MediumPurple);
        SendMessage("Unknown command.", Color.IndianRed);
    }
}
