using AnyPoly.DebugConsoleElements;
using AnyPoly.UI;
using Fastenshtein;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyPoly;

/// <summary>
/// A static class representing a debug console.
/// </summary>
internal static partial class DebugConsole
{
    private static readonly Color DefaultTextColor = Color.White;

    private static UIFrame baseFrame = default!;
    private static UIFrame messagesFrame = default!;
    private static UIListBox messages = default!;
    private static UITextInput textInput = default!;
    private static UIFrame commandInfoFrame = default!;
    private static UIText commandInfo = default!;

    private static bool openOnError;
    private static float messagesScale = 0.26f;

    private static BaseCommand? foundCommand;
    private static bool isFullInputValid;
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

        // Messages
        {
            messagesFrame = new UIFrame(new Color(60, 60, 60, 255), thickness: 2)
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
                Parent = messagesFrame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            messages = new UIListBox()
            {
                Parent = messagesFrame.InnerContainer,
                Orientation = Orientation.Vertical,
                Spacing = 6,
                IsScrollable = true,
                ScrollBarFrameColor = Color.Gray,
                ScrollBarThumbColor = Color.DarkGray,
                ScrollBarRelativeSize = 0.015f,
                Transform = { RelativePadding = new Vector4(0.005f) },
            };

            // After adding a new message, scroll to the bottom
            // if the scroll bar is at the bottom or has just appeared
            float? scrollPositionBeforeDequeue = null;
            messages.ComponentsDequeuing += (s, e) =>
            {
                scrollPositionBeforeDequeue = messages.ScrollPosition;
            };
            messages.ComponentsDequeued += (s, e) =>
            {
                if (scrollPositionBeforeDequeue is null or 1.0f)
                {
                    messages.ScrollTo(1.0f);
                }
            };

            messages.Transform.SizeChanged += (s, e) => messages.ScrollTo(1.0f);
        }

        // Command info
        {
            commandInfoFrame = new UIFrame(Color.White * 0.1f, thickness: 2)
            {
                Parent = textInput.Parent.Parent,
                IsEnabled = false,
                Transform =
                {
                    RelativeOffset = new Vector2(0.0f, -1.0f),
                    RelativePadding = new Vector4(0.005f, 0.0f, 0.005f, 0.0f),
                    IgnoreParentPadding = true,
                },
            };

            var background = new UISolidColor(Color.White * 0.2f)
            {
                Parent = commandInfoFrame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            commandInfo = new UIText(Color.White * 0.5f)
            {
                Parent = commandInfoFrame.InnerContainer,
                IsEnabled = false,
                Transform = { Alignment = Alignment.Left },
                TextAlignment = Alignment.Left,
                TextFit = TextFit.Both,
                Scale = 0.8f,
            };

            textInput.TextChanged += (s, e) => UpdateCommandInfo(e.Text);
        }

#if DEBUG
        openOnError = true;
#else
        openOnError = false;
#endif

        RegisterCommands();

        isInitialized = true;
    }

    /// <summary>
    /// Opens the debug console.
    /// </summary>
    public static void Open()
    {
        IsOpen = true;
        messages.ScrollTo(1.0f);
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

        if (IsOpen && foundCommand is not null
            && KeyboardController.IsKeyHit(Keys.Tab))
        {
            textInput.SetText(foundCommand.FullName);
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
            Scale = messagesScale,
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
    /// <param name="message">The error message to be sent.</param>
    public static void ThrowError(string message)
    {
        SendMessage(message, Color.Red);

        if (openOnError)
        {
            Open();
        }
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

        ThrowError(stringBuilder.ToString());
    }

    /* Code below needs refactoring, but it works for now :) */

    private static void TextInput_TextInputSent(object? sender, UI.TextInputEventArgs e)
    {
        SendMessage(">" + e.Text, Color.MediumPurple);
        int maxThreshold = Regex.Split(e.Text, @"\s+").Length + 1;
        BaseCommand? command = GetCommandFromInput(e.Text.Trim(), out int threshold);

        if (command is null || threshold > maxThreshold)
        {
            SendMessage("Unknown command.", Color.IndianRed);
        }
        else if (threshold > 0)
        {
            SendMessage($"Unknown command. Did you mean '{command.FullName}'?", Color.IndianRed);
        }
        else if (command is CommandGroup group)
        {
            SendMessage(group.GenerateHelpMessage());
        }
        else if (!isFullInputValid)
        {
            SendMessage("Incorrect command usage.", Color.IndianRed);
        }
        else if (command is Command c)
        {
            try
            {
                string[] args = GetArgsFromInput(command, e.Text);
                c.Action.Invoke(args);
            }
            catch (Exception ex) when
                (ex is FormatException or OverflowException
                or ArgumentException or ArgumentNullException
                or InvalidCastException or InvalidOperationException
                or NotSupportedException)
            {
                ThrowError(ex);
            }
        }

        messages.ScrollTo(1.0f);
    }

    private static void UpdateCommandInfo(string input)
    {
        static void Hide()
        {
            commandInfoFrame.IsEnabled = false;
            messagesFrame.Transform.RelativeSize
                = new Vector2(messagesFrame.Transform.RelativeSize.X, 0.89f);
        }

        static void Show()
        {
            commandInfoFrame.IsEnabled = true;
            messagesFrame.Transform.RelativeSize
                = new Vector2(messagesFrame.Transform.RelativeSize.X, 0.85f);
        }

        if (string.IsNullOrEmpty(input))
        {
            Hide();
            return;
        }

        BaseCommand? command = null;
        string[] inputParts = Regex.Split(input, @"\s+");
        bool isComplete = false;
        bool isInputValid = true;

        foreach (BaseCommand c in GetAllCommandCombinations(inputParts.Length))
        {
            if (c.FullName == input.TrimEnd())
            {
                command = c;
                isComplete = true;
                break;
            }

            if (c.FullName.StartsWith(input, StringComparison.InvariantCulture))
            {
                command = c;
                isComplete = false;
                break;
            }

            if (c is Command
                && input.StartsWith(c.FullName, StringComparison.InvariantCulture))
            {
                command = c;
                isComplete = true;
                break;
            }
        }

        if (command is null)
        {
            Hide();
            return;
        }

        Show();

        var stringBuilder = new StringBuilder();
        Color color = Color.White;
        isFullInputValid = false;

        if (command is CommandGroup group)
        {
            _ = stringBuilder
                .Append(group.Name)
                .Append(" [group] -> ")
                .Append(group.Description);

            if (isComplete)
            {
                _ = stringBuilder.Append(" [Send to see available subcommands]");
            }
        }
        else if (command is Command c)
        {
            Argument[] arguments = c.Arguments.ToArray();

            if (!isComplete || input == c.FullName || (arguments.Length == 0 && input.TrimEnd() == c.FullName))
            {
                _ = stringBuilder.Append(command.Name);

                if (arguments.Length > 0)
                {
                    _ = stringBuilder
                        .Append(" (")
                        .Append(string.Join(", ", arguments.Select(x => x.Name)))
                        .Append(')');
                }

                _ = stringBuilder
                    .Append(" -> ")
                    .Append(command.Description);
            }
            else
            {
                int argumentIndex = inputParts.Length - command.Depth - 2;

                if (argumentIndex > arguments.Length
                    || (argumentIndex == arguments.Length && input[^1] != ' '))
                {
                    _ = stringBuilder
                        .Append('(')
                        .Append(c.FullName)
                        .Append(") No more arguments are needed.");
                    isInputValid = false;
                }
                else if (arguments.Length > 0)
                {
                    Argument argument;
                    if (string.IsNullOrEmpty(inputParts[^1]) && argumentIndex == arguments.Length)
                    {
                        argument = arguments[Math.Max(0, argumentIndex - 1)];
                        isInputValid = argument.IsInputValid(inputParts[^2]);
                    }
                    else
                    {
                        argument = arguments[argumentIndex];
                        isInputValid = argument.IsInputValid(inputParts[^1]);
                    }

                    _ = stringBuilder
                        .Append(argument.Name)
                        .Append(" -> ")
                        .Append(argument.Description);
                }
            }

            isFullInputValid = isInputValid
                && inputParts.Length == command.Depth + arguments.Length + (input[^1] == ' ' ? 2 : 1);
        }

        if (!isComplete && isInputValid)
        {
            _ = stringBuilder.Append(" [Press 'TAB' to autocomplete]");
            foundCommand = command;
        }
        else
        {
            foundCommand = null;
        }

        commandInfo.Text = stringBuilder.ToString();
        commandInfo.Color = isFullInputValid && isComplete ? Color.LightGreen : isInputValid ? Color.DarkGray : Color.OrangeRed;
    }

    private static List<BaseCommand> GetAllCommandCombinations(int maxDepth, CommandGroup? group = null)
    {
        var result = new List<BaseCommand>();
        foreach (BaseCommand command in group is null ? Commands : group.Subcommands)
        {
            result.Add(command);
            if (command is CommandGroup commandGroup && command.Depth < maxDepth)
            {
                result.AddRange(GetAllCommandCombinations(maxDepth, commandGroup));
            }
        }

        return result;
    }

    private static BaseCommand? GetCommandFromInput(string input, out int threshold)
    {
        if (string.IsNullOrEmpty(input))
        {
            threshold = int.MaxValue;
            return null;
        }

        threshold = int.MaxValue;
        BaseCommand foundCommand = null!;
        var lev = new Levenshtein(input);
        string[] inputParts = Regex.Split(input, @"\s+");
        int maxDepth = inputParts.Length - 1;
        foreach (BaseCommand command in GetAllCommandCombinations(maxDepth))
        {
            int levenshteinDistance = int.MaxValue;
            if (command is Command c)
            {
                for (int i = 0; i < c.Arguments.Count(); i++)
                {
                    levenshteinDistance = Math.Min(levenshteinDistance, Levenshtein.Distance(
                        string.Join(' ', inputParts[..^(i + 1)]), command.FullName));
                }
            }

            levenshteinDistance = Math.Min(levenshteinDistance, lev.DistanceFrom(command.FullName));

            if (threshold >= levenshteinDistance)
            {
                threshold = levenshteinDistance;
                foundCommand = command;
            }
        }

        return foundCommand;
    }

    private static string[] GetArgsFromInput(BaseCommand command, string input)
    {
        return input[command.FullName.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
