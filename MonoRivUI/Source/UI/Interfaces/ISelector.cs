using System;

namespace MonoRivUI;

/// <summary>
/// Represents a selector component.
/// </summary>
public interface ISelector : IReadOnlyComponent
{
    /// <summary>
    /// Occurs when the selector is opening.
    /// </summary>
    event EventHandler? Opening;

    /// <summary>
    /// Occurs when the selector is opened.
    /// </summary>
    event EventHandler? Opened;

    /// <summary>
    /// Occurs when the selector is closing.
    /// </summary>
    event EventHandler? Closing;

    /// <summary>
    /// Occurs when the selector is closed.
    /// </summary>
    event EventHandler? Closed;

    /// <summary>
    /// Gets the active container.
    /// </summary>
    /// <remarks>
    /// Active container is displayed if the selector is opened.
    /// </remarks>
    Container ActiveContainer { get; }

    /// <summary>
    /// Gets the inactive container.
    /// </summary>
    /// <remarks>
    /// Inactive container is displayed if the selector is closed.
    /// </remarks>
    Container InactiveContainer { get; }

    /// <summary>
    /// Gets the list box.
    /// </summary>
    ListBox ListBox { get; }

    /// <summary>
    /// Gets a value indicating whether the selector is opened.
    /// </summary>
    bool IsOpened { get; }

    /// <summary>
    /// Gets or sets the relative height of the active container.
    /// </summary>
    /// <remarks>
    /// The height is relative to the selector's height.
    /// </remarks>
    float RelativeHeight { get; set; }

    /// <summary>
    /// Gets or sets the fixed height of the elements in the list box.
    /// </summary>
    /// <remarks>
    /// If it is set, the height of each element in the list box is fixed.
    /// </remarks>
    int? ElementFixedHeight { get; set; }

    /// <summary>
    /// Gets or sets the alignment of the active container.
    /// </summary>
    Alignment ActiveContainerAlignment { get; set; }

    /// <summary>
    /// Gets or sets the background of the active container.
    /// </summary>
    Component? ActiveBackground { get; set; }

    /// <summary>
    /// Gets or sets the background of the closed selector.
    /// </summary>
    Component? InactiveBackground { get; set; }

    /// <summary>
    /// Opens the selector.
    /// </summary>
    void Open();

    /// <summary>
    /// Closes the selector.
    /// </summary>
    void Close();

    /// <summary>
    /// Selects the current item based on the <see cref="Selector{T}.CurrentItemPredicate"/>.
    /// </summary>
    void SelectCurrentItem();
}