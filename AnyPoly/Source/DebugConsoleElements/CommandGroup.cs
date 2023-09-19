using System.Collections.Generic;
using System.Text;

namespace AnyPoly.DebugConsoleElements;

/// <summary>
/// Represents a command group.
/// </summary>
internal class CommandGroup : BaseCommand
{
    private readonly HashSet<BaseCommand> subcommand = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroup"/> class.
    /// </summary>
    /// <param name="name">The name of the command group.</param>
    /// <param name="description">The description of the command group.</param>
    public CommandGroup(string name, string description)
        : base(name, description)
    {
    }

    /// <summary>
    /// Gets a collection of the subcommands.
    /// </summary>
    public IEnumerable<BaseCommand> Subcommands => this.subcommand;

    /// <summary>
    /// Generates a help message for the command group.
    /// </summary>
    /// <returns>The generated help message.</returns>
    public string GenerateHelpMessage()
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder
            .Append("List of available '")
            .Append(this.FullName)
            .Append("' subcommands:");

        foreach (var command in this.subcommand)
        {
            _ = stringBuilder
                .Append("\n\t")
                .Append(command.FullName)
                .Append(" -> ")
                .Append(command.Description);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Adds a subcommand to the command group.
    /// </summary>
    /// <param name="command">The subcommand to be added to the command group.</param>
    /// <remarks>
    /// This method sets the <see cref="BaseCommand.Group"/> property of the subcommand.
    /// </remarks>
    public void AddSubcommand(BaseCommand command)
    {
        _ = this.subcommand.Add(command);
        command.Group = this;
    }

    /// <summary>
    /// Removes a subcommand from the command group.
    /// </summary>
    /// <param name="command">
    /// The subcommand to be removed from the command group.
    /// </param>
    /// <remarks>
    /// This method sets the <see cref="BaseCommand.Group"/>
    /// property of the subcommand to <c>null</c>.
    /// </remarks>
    public void RemoveSubcommand(BaseCommand command)
    {
        _ = this.subcommand.Remove(command);
        command.Group = null;
    }
}
