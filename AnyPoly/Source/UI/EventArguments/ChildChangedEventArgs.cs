using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents event data for a child change event.
/// </summary>
internal class ChildChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildChangedEventArgs"/> class.
    /// </summary>
    /// <param name="child">The child component that was added or removed.</param>
    public ChildChangedEventArgs(UIComponent child)
    {
        this.Child = child;
    }

    /// <summary>
    /// Gets the child component that was added or removed.
    /// </summary>
    public UIComponent Child { get; }
}
