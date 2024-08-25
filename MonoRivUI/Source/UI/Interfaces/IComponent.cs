using System;
using System.Collections.Generic;

namespace MonoRivUI;

/// <summary>
/// Represents a component.
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Gets the transform of the component.
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// Gets the parent of the component.
    /// </summary>
    IComponent? Parent { get; }

    /// <summary>
    /// Gets the children of the component.
    /// </summary>
    IEnumerable<IComponent> Children { get; }

    /// <summary>
    /// Gets the root component of the component.
    /// </summary>
    IComponent Root { get; }

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
    T? GetChild<T>(Predicate<T>? predicate = null)
        where T : IComponent;

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
    T? GetDescendant<T>(Predicate<T>? predicate = null)
        where T : IComponent;

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
    IEnumerable<T> GetAllChildren<T>(Predicate<T>? predicate = null)
        where T : IComponent;

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
    IEnumerable<T> GetAllDescendants<T>(Predicate<T>? predicate = null)
        where T : IComponent;

    /// <summary>
    /// Checks whether the component is a parent of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a parent of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsParentOf(IComponent component);

    /// <summary>
    /// Checks whether the component is an ancestor of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is an ancestor of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsAncestorOf(IComponent component);

    /// <summary>
    /// Checks whether the component is a child of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a child of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsChildOf(IComponent component);

    /// <summary>
    /// Checks whether the component is a descendant of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a descendant of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDescendantOf(IComponent component);
}
