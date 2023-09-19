namespace AnyPoly.DebugConsoleElements;

/// <summary>
/// Represents a base class for commands.
/// </summary>
internal abstract class BaseCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseCommand"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    protected BaseCommand(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets or sets the command group that the command belongs to.
    /// </summary>
    /// <remarks>
    /// NOTE: This property does NOT add/remove the command to/from the group.<br/>
    /// Use <see cref="CommandGroup.AddSubcommand(BaseCommand)"/> or
    /// <see cref="CommandGroup.RemoveSubcommand(BaseCommand)"/> instead.
    /// </remarks>
    public CommandGroup? Group { get; set; }

    /// <summary>
    /// Gets the full name of the command.
    /// </summary>
    /// <remarks>
    /// The full name of the command is the name of the command group
    /// followed by the name of the command separated by a space.
    /// </remarks>
    public string FullName => this.Group is null ? this.Name : $"{this.Group.FullName} {this.Name}";

    /// <summary>
    /// Gets the depth of the command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The depth of the command is the number of command groups
    /// that the command belongs to.
    /// </para>
    /// <para>
    /// Zero means that the command does not belong to any command group.
    /// </para>
    /// </remarks>
    public int Depth => this.Group is null ? 0 : this.Group.Depth + 1;
}
