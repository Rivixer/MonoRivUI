using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component hierarchy.
/// </summary>
public interface IComponentHierarchy
{
    /// <summary>
    /// Gets the parent of the component.
    /// </summary>
    IComponentHierarchy? Parent { get; }

    /// <summary>
    /// Gets the collection of children of the component.
    /// </summary>
    IEnumerable<IComponentHierarchy> Children { get; }

    /// <summary>
    /// Checks whether the component is a parent of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a parent of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsParent(IComponentHierarchy component)
    {
        return this == component.Parent;
    }

    /// <summary>
    /// Checks whether the component is an ancestor of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is an ancestor of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsAncestor(IComponentHierarchy component)
    {
        return this.IsParent(component)
            || (component.Parent is not null && this.IsAncestor(component.Parent));
    }

    /// <summary>
    /// Checks whether the component is a child of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a child of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsChild(IComponentHierarchy component)
    {
        return component.Children.Contains(this);
    }

    /// <summary>
    /// Checks whether the component is a descendant of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a descendant of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDescendant(IComponentHierarchy component)
    {
        return this.IsChild(component)
            || (this.Parent?.IsDescendant(component) ?? false);
    }

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
        where T : class, IComponentHierarchy;

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
        where T : class, IComponentHierarchy;

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
        where T : class, IComponentHierarchy;

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
        where T : class, IComponentHierarchy;
}
