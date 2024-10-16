using System;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for displaying scene event arguments.
/// </summary>
public class SceneDisplayEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneDisplayEventArgs"/> class.
    /// </summary>
    /// <param name="overlay">A value indicating whether the scene is an overlay.</param>
    public SceneDisplayEventArgs(bool overlay)
    {
        this.Overlay = overlay;
    }

    /// <summary>
    /// Gets a value indicating whether the scene is an overlay.
    /// </summary>
    public bool Overlay { get; }
}
