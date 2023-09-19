using System;
using System.Collections.Generic;

namespace AnyPoly.DebugConsoleElements;

/// <summary>
/// Represents a command.
/// </summary>
internal class Command : BaseCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    /// <param name="action">The action that is executed when the command is invoked.</param>
    /// <param name="arguments">A collection of required arguments.</param>
    public Command(string name, string description, Action<string[]> action, params Argument[] arguments)
        : base(name, description)
    {
        this.Action = action;
        this.Arguments = arguments;
    }

    /// <summary>
    /// Gets a collection of arguments required by the command.
    /// </summary>
    public IEnumerable<Argument> Arguments { get; } = Array.Empty<Argument>();

    /// <summary>
    /// Gets the action that is executed when the command is invoked.
    /// </summary>
    public Action<string[]> Action { get; }
}
