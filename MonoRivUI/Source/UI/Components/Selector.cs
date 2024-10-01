using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// Represents a selector component that allows the user
/// to select an item from a list of items.
/// </summary>
/// <typeparam name="T">The type of the value associated with each item.</typeparam>
public class Selector<T> : Component, ISelector, IStyleable<Selector<T>>
    where T : notnull
{
    private readonly HashSet<Item> items = new();
    private readonly Button<Container> button;
    private bool scrollToSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="Selector{T}"/> class.
    /// </summary>
    /// <param name="listBox">The list box that contains the items.</param>
    /// <remarks>
    /// Use this constructor to create a selector without a text component
    /// that display the selected item's name.
    /// </remarks>
    public Selector(ListBox listBox)
        : base()
    {
        this.InactiveContainer = new Container() { Parent = this };

        this.button = new Button<Container>(new Container())
        {
            Parent = this,
            IsEnabled = true,
        };

        this.button.Clicked += (s, e) => this.Open();

        this.ActiveContainer = new Container()
        {
            Parent = this,
            IsEnabled = false,
            IsPriority = true,
        };

        this.ActiveContainer.ChildAdded += this.ActiveContainer_ChildAdded;

        this.ListBox = listBox;
        this.ListBox.Parent = this.ActiveContainer;
    }

    /// <inheritdoc/>
    public event EventHandler? Opening;

    /// <inheritdoc/>
    public event EventHandler? Opened;

    /// <inheritdoc/>
    public event EventHandler? Closing;

    /// <inheritdoc/>
    public event EventHandler? Closed;

    /// <summary>
    /// Occurs when an item is selecting.
    /// </summary>
    public event EventHandler<Item?>? ItemSelecting;

    /// <summary>
    /// Occurs when an item is selected.
    /// </summary>
    public event EventHandler<Item?>? ItemSelected;

    /// <inheritdoc/>
    event EventHandler<ISelector.Item?>? ISelector.ItemSelecting
    {
        add => this.ItemSelecting += (s, e) => value?.Invoke(s, e);
        remove => this.ItemSelecting -= (s, e) => value?.Invoke(s, e);
    }

    /// <inheritdoc/>
    event EventHandler<ISelector.Item?>? ISelector.ItemSelected
    {
        add => this.ItemSelected += (s, e) => value?.Invoke(s, e);
        remove => this.ItemSelected -= (s, e) => value?.Invoke(s, e);
    }

    /// <inheritdoc/>
    public Container ActiveContainer { get; }

    /// <inheritdoc/>
    public Container InactiveContainer { get; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IEnumerable<Item> Items => this.items;

    /// <inheritdoc/>
    IEnumerable<ISelector.Item> ISelector.Items => this.Items;

    /// <inheritdoc/>
    public ListBox ListBox { get; private set; }

    /// <inheritdoc/>
    public bool IsOpened { get; private set; }

    /// <inheritdoc/>
    public bool CloseAfterSelect { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the selector
    /// should scroll to the selected item after opening.
    /// </summary>
    /// <remarks>
    /// The list box must be a <see cref="ScrollableListBox"/>.
    /// Otherwise, an <see cref="InvalidOperationException"/> is thrown
    /// when setting the value to <see langword="true"/>.
    /// </remarks>
    public bool ScrollToSelected
    {
        get => this.scrollToSelected;
        set
        {
            if (value && this.ListBox is not ScrollableListBox)
            {
                throw new InvalidOperationException("The list box must be a ScrollableListBox");
            }

            this.scrollToSelected = value;
        }
    }

    /// <inheritdoc/>
    public float RelativeHeight
    {
        get => this.ActiveContainer.Transform.RelativeSize.Y;
        set => this.ActiveContainer.Transform.RelativeSize
            = new Vector2(this.ActiveContainer.Transform.RelativeSize.X, value);
    }

    /// <summary>
    /// Gets or sets the fixed height of the elements.
    /// </summary>
    public int? ElementFixedHeight { get; set; }

    /// <inheritdoc/>
    ISelector.Item? ISelector.SelectedItem => this.SelectedItem;

    /// <summary>
    /// Gets the selected item.
    /// </summary>
    public Item? SelectedItem { get; private set; }

    /// <inheritdoc/>
    public Alignment ActiveContainerAlignment
    {
        get => this.ActiveContainer.Transform.Alignment;
        set => this.ActiveContainer.Transform.Alignment = value;
    }

    /// <summary>
    /// Gets or sets the predicate used to select the current item.
    /// </summary>
    public Func<T, bool>? CurrentItemPredicate { get; set; }

    /// <summary>
    /// Adds an item to the selector.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(Item item)
    {
        _ = this.items.Add(item);
        var button = (Component)item.Button;

        button.Parent = this.ListBox.ContentContainer;
        button.Transform.SizeChanged += (s, e) =>
        {
            if (this.ElementFixedHeight is not null)
            {
                button.Transform.SetRelativeSizeFromAbsolute(y: this.ElementFixedHeight);
            }
        };
    }

    /// <summary>
    /// Adds items to the selector.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddItems(IEnumerable<Item> items)
    {
        items.ToList().ForEach(this.AddItem);
    }

    /// <inheritdoc/>
    public void Open()
    {
        if (this.IsOpened)
        {
            return;
        }

        this.Opening?.Invoke(this, EventArgs.Empty);

        this.IsOpened = true;
        this.ActiveContainer.IsEnabled = true;
        this.InactiveContainer.IsEnabled = false;

        if (this.scrollToSelected && this.SelectedItem is not null)
        {
            var listBox = (ScrollableListBox)this.ListBox;
            int index = this.items.ToList().IndexOf(this.SelectedItem);
            float percentage = (index / (float)this.items.Count)
                + (listBox.Spacing / listBox.TotalLength / 2);
            listBox.ScrollBar.ScrollTo(Math.Clamp(percentage, 0f, 1f));
        }

        this.Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (!this.IsOpened)
        {
            return;
        }

        this.Closing?.Invoke(this, EventArgs.Empty);
        this.IsOpened = false;
        this.ActiveContainer.IsEnabled = false;
        this.InactiveContainer.IsEnabled = true;
        this.Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.IsOpened && (this.ClickedApartFromSelector() || this.ClickedEscape()))
        {
            // We have to update children components before closing the selector
            // to avoid the case where the selected item is not updated
            // (e.g. the selected item is a button and the button is clicked)
            base.Update(gameTime);
            this.Close();

            // Avoid updating the children components twice
            return;
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// Selects an item.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <exception cref="InvalidOperationException">The item is not in the list of items.</exception>
    public void SelectItem(Item? item)
    {
        if (item is not null && !this.items.Contains(item))
        {
            throw new InvalidOperationException("The item is not in the list of items");
        }

        this.ItemSelecting?.Invoke(this, item);

        this.SelectedItem = item;

        if (this.CloseAfterSelect)
        {
            this.Close();
        }

        this.ItemSelected?.Invoke(this, item);
    }

    /// <summary>
    /// Selects an item based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to select an item.</param>
    public void SelectItem(Predicate<Item> predicate)
    {
        this.SelectItem(this.Items.FirstOrDefault(x => predicate(x)));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the <see cref="CurrentItemPredicate"/> is not set,
    /// the current item is set to <see langword="null"/>.
    /// </remarks>
    public void SelectCurrentItem()
    {
        if (this.CurrentItemPredicate is not null)
        {
            this.SelectItem(x => this.CurrentItemPredicate(x.TValue));
        }
        else
        {
            this.SelectItem((Item?)null);
        }
    }

    /// <summary>
    /// Clears the items.
    /// </summary>
    public void ClearItems()
    {
        this.ListBox.Clear();
        this.items.Clear();
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<ISelector> style)
    {
        style.Apply(this);
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<Selector<T>> style)
    {
        style.Apply(this);
    }

    private bool ClickedApartFromSelector()
    {
        return MouseController.IsLeftButtonClicked()
            && !MouseController.WasDragStateChanged
            && !MouseController.IsComponentFocused(this.ActiveContainer)
            && MouseController.IsComponentFocused(this.Root);
    }

    private bool ClickedEscape()
    {
        return (!ScreenController.DisplayedOverlays.Any() || ((ScreenController.DisplayedOverlays.Last().Value as Scene)?.BaseComponent.IsAncestorOf(this) ?? true))
            && KeyboardController.IsKeyHit(Keys.Escape);
    }

    private void ActiveContainer_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        // Be sure that list box is on top of the active container
        if (e.Child != this.ListBox)
        {
            this.ListBox.Parent = null;
            this.ListBox.Parent = this.ActiveContainer;
        }
    }

    /// <summary>
    /// Represents an item in the selector.
    /// </summary>
    /// <param name="Button">The button that represents the item.</param>
    /// <param name="TValue">The generic value associated with the item.</param>
    /// <param name="Name">The name of the item.</param>
    public record class Item(IButton Button, T TValue, string? Name = null)
        : ISelector.Item(Button, TValue, Name);
}
