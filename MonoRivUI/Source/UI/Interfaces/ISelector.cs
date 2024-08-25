using System;
using System.Collections.Generic;

namespace MonoRivUI;

/// <summary>
/// Represents a selector component.
/// </summary>
public interface ISelector : IComponent, IStyleable<ISelector>
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
    /// Occurs when an item is selecting.
    /// </summary>
    public event EventHandler<Item?>? ItemSelecting;

    /// <summary>
    /// Occurs when an item is selected.
    /// </summary>
    public event EventHandler<Item?>? ItemSelected;

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
    /// Gets the items.
    /// </summary>
    public IEnumerable<Item> Items { get; }

    /// <summary>
    /// Gets the list box.
    /// </summary>
    ListBox ListBox { get; }

    /// <summary>
    /// Gets the selected item.
    /// </summary>
    Item? SelectedItem { get; }

    /// <summary>
    /// Gets a value indicating whether the selector is opened.
    /// </summary>
    bool IsOpened { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the selector should
    /// close after an item is selected.
    /// </summary>
    bool CloseAfterSelect { get; set; }

    /// <summary>
    /// Gets or sets the relative height of the active container.
    /// </summary>
    /// <remarks>
    /// The height is relative to the selector's height.
    /// </remarks>
    float RelativeHeight { get; set; }

    /// <summary>
    /// Gets or sets the alignment of the active container.
    /// </summary>
    Alignment ActiveContainerAlignment { get; set; }

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

    /// <summary>
    /// Represents an item in the selector.
    /// </summary>
    /// <param name="Button">The button that represents the item.</param>
    /// <param name="Value">The value associated with the item.</param>
    /// <param name="Name">The name of the item.</param>
    public record class Item(IButton Button, object Value, string? Name = null);
}
