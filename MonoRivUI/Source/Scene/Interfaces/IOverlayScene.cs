using System;
using System.Collections.Generic;

namespace MonoRivUI;

/// <summary>
/// Represents an overlay scene in the game.
/// </summary>
public interface IOverlayScene : IScene
{
    /// <summary>
    /// Occurs when the priority of the overlay scene changes.
    /// </summary>
    event EventHandler<(int Before, int After)> PriorityChanged;

    /// <summary>
    /// Gets the priority of the overlay scene.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets the overlay components of the scene.
    /// </summary>
    IEnumerable<IComponent> OverlayComponents { get; }
}
