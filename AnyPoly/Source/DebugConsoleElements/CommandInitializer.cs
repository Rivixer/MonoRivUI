using AnyPoly.DebugConsoleElements;
using AnyPoly.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AnyPoly;

/// <summary>
/// Represents a debug console.
/// </summary>
internal static partial class DebugConsole
{
    private static readonly List<BaseCommand> Commands = new();
    private static bool areCommandsRegistered;

    private static void RegisterCommands()
    {
        if (areCommandsRegistered)
        {
            throw new InvalidOperationException("Commands are already registered.");
        }

        Commands.Add(new Command("exit", "Exit the game.", args => AnyPolyGame.Instance.Exit()));

        Commands.Add(new Command(
            "help",
            "Display the list of available commands.",
            (args) =>
            {
                var sb = new StringBuilder();
                _ = sb.Append("List of available commands:");

                foreach (var command in Commands)
                {
                    _ = sb.Append($"\n  ")
                        .Append(command.Name)
                        .Append(" -> ")
                        .Append(command.Description);
                }

                SendMessage(sb.ToString());
            }));

        // Screen commands group
        {
            var screenCommandGroup = new CommandGroup("screen", "Change screen settings.");
            Commands.Add(screenCommandGroup);

            // Screen resolution commands group
            {
                var screenResolutionCommandGroup = new CommandGroup("resolution", "Interfere with the screen settings.");
                screenCommandGroup.AddSubcommand(screenResolutionCommandGroup);

                screenResolutionCommandGroup.AddSubcommand(new Command(
                    "get",
                    "Get the current resolution of the screen.",
                    args => SendMessage($"Current resolution: {ScreenController.Width}x{ScreenController.Height}")));

                screenResolutionCommandGroup.AddSubcommand(new Command(
                    "set",
                    "Set the resolution of the screen.",
                    args =>
                    {
                        int width = int.Parse(args[0], CultureInfo.InvariantCulture);
                        int height = int.Parse(args[1], CultureInfo.InvariantCulture);
                        ScreenController.Change(width, height);
                        ScreenController.ApplyChanges();
                    },
                    new Argument("width", "A new width of the screen (200-3840).", (x) => int.TryParse(x, out var i) && i is >= 200 and <= 3840),
                    new Argument("height", "A new height of the screen (150-2160).", (x) => int.TryParse(x, out var i) && i is >= 150 and <= 2160)));
            }

            // Screen type commands group
            {
                var screenTypeCommandGroup = new CommandGroup("type", "Interfere with the type of the screen.");
                screenCommandGroup.AddSubcommand(screenTypeCommandGroup);

                screenTypeCommandGroup.AddSubcommand(new Command(
                    "get",
                    "Get the current type of the screen.",
                    args => SendMessage($"Current type: {ScreenController.ScreenType}")));

                screenTypeCommandGroup.AddSubcommand(new Command(
                    "set",
                    $"Set the type of the screen.",
                    args =>
                    {
                        ScreenType screenType = Enum.Parse<ScreenType>(args[0], ignoreCase: true);
                        ScreenController.Change(screenType: screenType);
                        ScreenController.ApplyChanges();
                        SendMessage($"The type of the screen has been changed to {screenType}.");
                    },
                    new Argument(
                        "type",
                        $"A new type of the screen. ({string.Join(", ", Enum.GetNames(typeof(ScreenType)))})",
                        (x) => Enum.TryParse(typeof(ScreenType), x, ignoreCase: true, out var _))));
            }
        }

        // Console commands group
        {
            var consoleCommandGroup = new CommandGroup("console", "Change debug console settings.");
            Commands.Add(consoleCommandGroup);

            // Console window commands group
            {
                var consoleWindowCommandGroup = new CommandGroup("window", "Interfere with the debug console window.");
                consoleCommandGroup.AddSubcommand(consoleWindowCommandGroup);

                // Console window size commands group
                {
                    var consoleWindowSizeCommandGroup = new CommandGroup("size", "Interfere with the size of the debug console window.");
                    consoleWindowCommandGroup.AddSubcommand(consoleWindowSizeCommandGroup);

                    consoleWindowSizeCommandGroup.AddSubcommand(new Command(
                        "get",
                        "Get the current size of the debug console window (unscaled).",
                        args =>
                        {
                            Point size = BaseFrame.Transform.UnscaledSize;
                            SendMessage($"Current debug console size (unscaled): {size.X}x{size.Y}");
                        }));

                    static bool IsSizeValid(string input, int minSize, int currentSize, int currentLocation, int defaultSize)
                    {
                        return int.TryParse(input, out var i) && i >= minSize && i + currentLocation <= defaultSize;
                    }

                    consoleWindowSizeCommandGroup.AddSubcommand(new Command(
                        "set",
                        "Set the size of the debug console window (unscaled).",
                        args =>
                        {
                            int width = int.Parse(args[0], CultureInfo.InvariantCulture);
                            int height = int.Parse(args[1], CultureInfo.InvariantCulture);
                            baseFrame.Transform.UnscaledSize = new Point(width, height);
                            SendMessage("The size of the debug console window has been changed.");
                        },
                        new Argument("width", "A new width of the window (>=200).", (x) =>
                        {
                            int currentSizeX = BaseFrame.Transform.UnscaledSize.X;
                            int currentLocationX = BaseFrame.Transform.UnscaledLocation.X;
                            int defaultSizeX = ScreenController.DefaultSize.X;
                            return IsSizeValid(x, 200, currentSizeX, currentLocationX, defaultSizeX);
                        }),
                        new Argument("height", "A new height of the window (>=130).", (x) =>
                        {
                            int currentSizeY = BaseFrame.Transform.UnscaledSize.Y;
                            int currentLocationY = BaseFrame.Transform.UnscaledLocation.Y;
                            int defaultSizeY = ScreenController.DefaultSize.Y;
                            return IsSizeValid(x, 200, currentSizeY, currentLocationY, defaultSizeY);
                        })));
                }

                // Console window location commands group
                {
                    var consoleWindowLocationCommandGroup = new CommandGroup("location", "Interfere with the location of the debug console window.");
                    consoleWindowCommandGroup.AddSubcommand(consoleWindowLocationCommandGroup);

                    consoleWindowLocationCommandGroup.AddSubcommand(new Command(
                        "get",
                        "Get the current location of the debug console window (unscaled).",
                        args =>
                        {
                            Point location = BaseFrame.Transform.UnscaledLocation;
                            SendMessage($"Current debug console location (unscaled): {location.X}x{location.Y}");
                        }));

                    static bool IsLocationValid(string input, int currentSize, int defaultSize)
                    {
                        return int.TryParse(input, out var i) && i >= 0 && i + currentSize <= defaultSize;
                    }

                    consoleWindowLocationCommandGroup.AddSubcommand(new Command(
                        "set",
                        "Set the location of the debug console window (unscaled).",
                        args =>
                        {
                            int x = int.Parse(args[0], CultureInfo.InvariantCulture);
                            int y = int.Parse(args[1], CultureInfo.InvariantCulture);
                            baseFrame.Transform.UnscaledLocation = new Point(x, y);
                            SendMessage("The location of the debug console window has been changed.");
                        },
                        new Argument("x", "The new x coordinate of the location.", (x) =>
                        {
                            int currentSizeX = BaseFrame.Transform.UnscaledSize.X;
                            int defaultSizeX = ScreenController.DefaultSize.X;
                            return IsLocationValid(x, currentSizeX, defaultSizeX);
                        }),
                        new Argument("y", "The new y coordinate of the location.", (x) =>
                        {
                            int currentSizeY = BaseFrame.Transform.UnscaledSize.Y;
                            int defaultSizeY = ScreenController.DefaultSize.Y;
                            return IsLocationValid(x, currentSizeY, defaultSizeY);
                        })));
                }
            }

            // Console messages commands group
            {
                var consoleMessagesCommandGroup = new CommandGroup("messages", "Interfere with the debug console messages.");
                consoleCommandGroup.AddSubcommand(consoleMessagesCommandGroup);

                // Console messages scale commands group
                {
                    var consoleMessagesScaleCommandGroup = new CommandGroup("scale", "Interfere with the scale of the debug console messages.");
                    consoleMessagesCommandGroup.AddSubcommand(consoleMessagesScaleCommandGroup);

                    consoleMessagesScaleCommandGroup.AddSubcommand(new Command(
                        "get",
                        "Get the current scale of the debug console messages.",
                        args => SendMessage($"Current debug console messages scale: {messagesScale}")));

                    consoleMessagesScaleCommandGroup.AddSubcommand(new Command(
                        "set",
                        "Set the scale of the debug console messages.",
                        args =>
                        {
                            float scale = float.Parse(args[0], CultureInfo.InvariantCulture);
                            messagesScale = scale;

                            foreach (var message in messages.Components.Cast<UIWrappedText>())
                            {
                                message.Scale = scale;
                            }

                            foreach (var message in messages.QueuedComponents.Cast<UIWrappedText>())
                            {
                                message.Scale = scale;
                            }

                            SendMessage("The scale of the debug console messages has been changed.");
                        },
                        new Argument("scale", "A new scale of the debug console messages (>0.0).", (x) =>
                        {
                            return float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) && f > 0.0f;
                        })));
                }
            }
        }

        areCommandsRegistered = true;
    }
}
