using System;
using System.Collections.Generic;

namespace MonoRivUI;

/// <summary>
/// Represents an overlay scene in the game.
/// </summary>
public interface IOverlayScene : IOverlay, IScene
{
    /// <summary>
    /// Gets the overlay components of the scene.
    /// </summary>
    IEnumerable<IComponent> OverlayComponents { get; }
}
