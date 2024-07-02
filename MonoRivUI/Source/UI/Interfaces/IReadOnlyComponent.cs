using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Gets a value indicating whether the component is enabled.
    /// </summary>
    /// <remarks>
    /// If component is not enabled, it will not be updated or drawn.
    /// </remarks>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the component is priority.
    /// </summary>
    /// <remarks>
    /// Priority components are drawn after other components.
    /// </remarks>
    bool IsPriority { get; }

    /// <summary>
    /// Returns the first child of a component
    /// that is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the child to get.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the child components.
    /// </param>
    /// <returns>
    /// The first child of a component that is of type <typeparamref name="T"/>,
    /// or <see langword="null"/> if no child is found.
    /// </returns>
    public T? GetChild<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent;

    /// <summary>
    /// Returns the first descendant of a component
    /// that is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the descendant to get.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the descendant components.</param>
    /// <returns>
    /// The first descendant of a component that is of type <typeparamref name="T"/>,
    /// or <see langword="null"/> if no descendant is found.
    /// </returns>
    public T? GetDescendant<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent;

    /// <summary>
    /// Retrieves all children of a component
    /// that are of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the children to retrieve.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the children components.
    /// </param>
    /// <returns>
    /// An enumerable containing all children of a component
    /// that are of type <typeparamref name="T"/>.
    /// </returns>
    public IEnumerable<T> GetAllChildren<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent;

    /// <summary>
    /// Retrieves all descendants of a component
    /// that are of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the descendants to retrive.</typeparam>
    /// <param name="predicate">
    /// An optional predicate to use to filter the descendants components.
    /// </param>
    /// <returns>
    /// An enumerable containing all descendants of a component
    /// that are of type <typeparamref name="T"/>.
    /// </returns>
    public IEnumerable<T> GetAllDescendants<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent;
}
