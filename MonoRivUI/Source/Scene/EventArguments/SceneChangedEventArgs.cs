using System;

namespace MonoRivUI;

/// <summary>
/// Represents a scene changed event arguments.
/// </summary>
public class SceneChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previous">The previous scene.</param>
    /// <param name="current">The current scene.</param>
    public SceneChangedEventArgs(Scene? previous, Scene current)
    {
        this.Previous = previous;
        this.Current = current;
    }

    /// <summary>
    /// Gets the previous scene.
    /// </summary>
    public Scene? Previous { get; }

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public Scene Current { get; }
}
