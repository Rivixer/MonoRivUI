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
    /// Gets the root of the component hierarchy.
    /// </summary>
    IComponentHierarchy Root { get; }

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
}
