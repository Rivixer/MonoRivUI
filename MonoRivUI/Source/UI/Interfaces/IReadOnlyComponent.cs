﻿using System.Collections.Generic;

namespace MonoRivUI;

/// <summary>
/// Represents a read-only UI component.
/// </summary>
public interface IReadOnlyComponent : IComponentHierarchy
{
    /// <summary>
    /// Gets the read-only transform of the component.
    /// </summary>
    IReadOnlyTransform Transform { get; }

    /// <summary>
    /// Gets the read-only parent of the component.
    /// </summary>
    new IReadOnlyComponent? Parent { get; }

    /// <summary>
    /// Gets the read-only collection of children of the component.
    /// </summary>
    new IEnumerable<IReadOnlyComponent> Children { get; }

    /// <summary>
    /// Gets a value indicating whether the component
    /// automatically updates itself and its child
    /// components during the game loop.
    /// </summary>
    bool AutoUpdate { get; }

    /// <summary>
    /// Gets a value indicating whether the component
    /// automatically draws itself and its child
    /// components during the game loop.
    /// </summary>
    bool AutoDraw { get; }

    /// <summary>
    /// Gets the unique identifier of the component.
    /// </summary>
    uint Id { get; }
}
