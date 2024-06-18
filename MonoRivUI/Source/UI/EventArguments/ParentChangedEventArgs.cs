using System;

namespace MonoRivUI;

/// <summary>
/// Represents event data for a parent change event.
/// </summary>
public class ParentChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParentChangedEventArgs"/> class.
    /// </summary>
    /// <param name="current">The current parent of the component.</param>
    /// <param name="previous">The previuos parent of the component.</param>
    public ParentChangedEventArgs(Component? current, Component? previous)
    {
        this.Current = current;
        this.Previous = previous;
    }

    /// <summary>
    /// Gets the current parent of the component.
    /// </summary>
    public Component? Current { get; }

    /// <summary>
    /// Gets the previous parent of the component.
    /// </summary>
    public Component? Previous { get; }
}
