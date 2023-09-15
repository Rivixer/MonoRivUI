using System.Collections.Generic;
using System.Linq;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component hierarchy.
/// </summary>
internal interface IUIComponentHierarchy
{
    /// <summary>
    /// Gets the parent of the component.
    /// </summary>
    IUIComponentHierarchy? Parent { get; }

    /// <summary>
    /// Gets the collection of children of the component.
    /// </summary>
    IEnumerable<IUIComponentHierarchy> Children { get; }

    /// <summary>
    /// Checks whether the component is a parent of the specified component.
    /// </summary>
    /// <param name="component">The component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the component is a parent of the specified component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsParent(IUIComponentHierarchy component)
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
    bool IsAncestor(IUIComponentHierarchy component)
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
    bool IsChild(IUIComponentHierarchy component)
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
    bool IsDescendant(IUIComponentHierarchy component)
    {
        return this.IsChild(component)
            || (this.Parent?.IsDescendant(component) ?? false);
    }
}
