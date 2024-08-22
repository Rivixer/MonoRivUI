using System;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for displaying scene event arguments.
/// </summary>
public abstract class SceneDisplayEventArgs : EventArgs
{
    /// <summary>
    /// Gets an empty instance of the <see cref="SceneDisplayEventArgs"/> class.
    /// </summary>
    public static new SceneDisplayEventArgs Empty { get; } = new EmptySceneShowingEventArgs();

    private class EmptySceneShowingEventArgs : SceneDisplayEventArgs
    {
    }
}
