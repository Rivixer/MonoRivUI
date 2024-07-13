using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// Represents a selector component that allows the user to select an item from a list of items.
/// </summary>
/// <typeparam name="T">The type of the value associated with each item.</typeparam>
public class Selector<T> : Component, ISelector
{
    private readonly HashSet<Item> items = new();
    private readonly Text? selectedItemText;

    private Component? background;
    private Component? selectedItemBackground;
    private Func<T, bool>? currentItemPredicate;
    private bool scrollToSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="Selector{T}"/> class.
    /// </summary>
    /// <remarks>
    /// Use this constructor to create a selector without a text component
    /// that display the selected item's name.
    /// </remarks>
    public Selector()
        : base()
    {
        this.ActiveContainer = new Container()
        {
            Parent = this,
            IsEnabled = false,
            IsPriority = true,
        };

        this.InactiveContainer = new Container() { Parent = this };

        this.ListBox = new ListBox() { Parent = this.ActiveContainer };
        this.ListBox.ComponentDequeued += (s, e) =>
        {
            if (this.ElementFixedHeight is { } fixedSizeY)
            {
                e.Transform.SetRelativeSizeFromAbsolute(y: fixedSizeY);
            }
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Selector{T}"/> class.
    /// </summary>
    /// <param name="font">The font of the text component that displays the selected item's name.</param>
    public Selector(ScalableFont font)
        : this()
    {
        this.selectedItemText = new Text(font, Color.White)
        {
            Parent = this,
            TextAlignment = Alignment.Center,
        };
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
    public Container ActiveContainer { get; }

    /// <inheritdoc/>
    public Container InactiveContainer { get; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IEnumerable<Item> Items => this.items;

    /// <inheritdoc/>
    public ListBox ListBox { get; private set; }

    /// <inheritdoc/>
    public bool IsOpened { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the selector
    /// should scroll to the selected item after opening.
    /// </summary>
    /// <remarks>
    /// The list box must be scrollable, otherwise throws an exception.
    /// </remarks>
    public bool ScrollToSelected
    {
        get => this.scrollToSelected;
        set
        {
            if (!this.ListBox.IsScrollable)
            {
                throw new InvalidOperationException(
                    $"The {nameof(this.ListBox)} must have the " +
                    $"{nameof(this.ListBox.IsScrollable)} property set to true");
            }

            this.scrollToSelected = value;
        }
    }

    /// <inheritdoc/>
    public float RelativeHeight
    {
        get => this.ActiveContainer.Transform.RelativeSize.Y;
        set => this.ActiveContainer.Transform.RelativeSize = new Vector2(this.ActiveContainer.Transform.RelativeSize.X, value);
    }

    /// <inheritdoc/>
    public int? ElementFixedHeight { get; set; }

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

    /// <inheritdoc/>
    public Component? ActiveBackground
    {
        get => this.background;
        set
        {
            if (value is null)
            {
                if (this.background is not null)
                {
                    this.background.Parent = null;
                }

                this.ListBox.Parent = this.ActiveContainer;
            }
            else
            {
                value.Parent = this.ActiveContainer;
                this.ListBox.Parent = value;
            }

            this.background = value;
        }
    }

    /// <inheritdoc/>
    public Component? InactiveBackground
    {
        get => this.selectedItemBackground;
        set
        {
            if (value is null)
            {
                if (this.selectedItemBackground is not null)
                {
                    this.selectedItemBackground.Parent = null;
                }

                if (this.selectedItemText is not null)
                {
                    this.selectedItemText.Parent = this.InactiveContainer;
                }
            }
            else
            {
                value.Parent = this.InactiveContainer;
                if (this.selectedItemText is not null)
                {
                    this.selectedItemText.Parent = value;
                }
            }

            this.selectedItemBackground = value;
        }
    }

    /// <summary>
    /// Gets or sets the predicate used to select the current item.
    /// </summary>
    public Func<T, bool>? CurrentItemPredicate
    {
        get => this.currentItemPredicate;
        set => this.currentItemPredicate = value;
    }

    /// <summary>
    /// Adds an item to the selector.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(Item item)
    {
        _ = this.items.Add(item);
        (item.Button as Component)!.Parent = this.ListBox.ContentContainer;
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
            // Be sure that all components are dequeued before scrolling
            this.ListBox.DequeueComponents();
            int index = this.items.ToList().IndexOf(this.SelectedItem);
            float percentage = (index / (float)this.items.Count)
                + (this.ListBox.Spacing / this.ListBox.TotalLength / 2);
            this.ListBox.ScrollBar?.ScrollTo(Math.Clamp(percentage, 0f, 1f));
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

        if (!this.IsOpened
            && !MouseController.WasDragStateChanged
            && MouseController.IsLeftButtonClicked()
            && MouseController.IsComponentFocused(this))
        {
            this.Open();
        }
        else if (this.IsOpened && (this.ClickedApartFromSelector() || this.ClickedEscape()))
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
            this.SelectItem(x => this.CurrentItemPredicate(x.Value));
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

    private bool ClickedApartFromSelector()
    {
        return MouseController.IsLeftButtonClicked()
            && !MouseController.WasDragStateChanged
            && (!this.ListBox.ScrollBar?.IsThumbDragging ?? true)
            && !MouseController.IsComponentFocused(this.ActiveContainer)
            && MouseController.IsComponentFocused(this.Root);
    }

    private bool ClickedEscape()
    {
        return !Scene.DisplayedOverlays.Any()
            && KeyboardController.IsKeyHit(Keys.Escape);
    }

    /// <summary>
    /// Represents an item in the selector.
    /// </summary>
    /// <param name="Button">The button that represents the item.</param>
    /// <param name="Value">The value associated with the item.</param>
    /// <param name="Name">The name of the item.</param>
    public record class Item(IButton Button, T Value, string? Name = null);
}
