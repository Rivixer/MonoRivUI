using System.Collections.Generic;

namespace AnyPoly.UI;

/// <summary>
/// Represents a read-only UI component.
/// </summary>
internal interface IUIReadOnlyComponent
{
    /// <summary>
    /// Gets the read-only transform of the component.
    /// </summary>
    public IUIReadOnlyTransform Transform { get; }

    /// <summary>
    /// Gets the read-only parent of the component.
    /// </summary>
    public IUIReadOnlyComponent? Parent { get; }

    /// <summary>
    /// Gets the read-only collection of children of the component.
    /// </summary>
    public IEnumerable<IUIReadOnlyComponent> Children { get; }

    /// <summary>
    /// Gets a value indicating whether the component
    /// automatically updates itself and its child
    /// components during the game loop.
    /// </summary>
    public bool AutoUpdate { get; }

    /// <summary>
    /// Gets a value indicating whether the component
    /// automatically draws itself and its child
    /// components during the game loop.
    /// </summary>
    public bool AutoDraw { get; }

    /// <summary>
    /// Gets the unique identifier of the component.
    /// </summary>
    public uint Id { get; }
}
