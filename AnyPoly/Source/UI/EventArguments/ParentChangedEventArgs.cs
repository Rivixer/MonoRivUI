using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents event data for a parent change event.
/// </summary>
internal class ParentChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParentChangedEventArgs"/> class.
    /// </summary>
    /// <param name="newParent">The new parent of the component.</param>
    /// <param name="oldParent">The old parent of the component.</param>
    public ParentChangedEventArgs(UIComponent? newParent, UIComponent? oldParent)
    {
        this.NewParent = newParent;
        this.OldParent = oldParent;
    }

    /// <summary>
    /// Gets the new parent of the component.
    /// </summary>
    public UIComponent? NewParent { get; }

    /// <summary>
    /// Gets the old parent of the component.
    /// </summary>
    public UIComponent? OldParent { get; }
}
