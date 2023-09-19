using System;

namespace AnyPoly.DebugConsoleElements;

/// <summary>
/// Represents an argument of a debug console command.
/// </summary>
public class Argument
{
    private readonly Func<string, bool> validMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="Argument"/> class.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="description">The description of the argument.</param>
    /// <param name="validMethod">The method that checks if the argument is valid.</param>
    public Argument(string name, string description, Func<string, bool> validMethod)
    {
        this.Name = name;
        this.Description = description;
        this.validMethod = validMethod;
    }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the argument.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Checks if the input value is valid.
    /// </summary>
    /// <param name="value">The input value to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the input value is valid;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsInputValid(string value)
    {
        return this.validMethod(value);
    }
}
