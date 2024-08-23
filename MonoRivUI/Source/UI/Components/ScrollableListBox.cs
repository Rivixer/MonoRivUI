using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a scrollable list box, which can contain multiple components.
/// </summary>
/// <remarks>
/// A scrollable list box is a list box that can be scrolled vertically or horizontally.
/// </remarks>
public class ScrollableListBox : ListBox
{
    private int totalLength;
    private int currentScrollOffset;
    private bool isScrollBarNeeded;
    private bool isTotalLengthMeasureNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollableListBox"/> class.
    /// </summary>
    /// <param name="thumb">The thumb component of the scroll bar.</param>
    /// <param name="relativeSize">The relative size of the scroll bar.</param>
    /// <remarks>
    /// This constructor creates a basic scroll bar with a thumb component
    /// and a relative size. Use <see cref="ScrollableListBox(ScrollBar)"/>
    /// to create a list box with a custom scroll bar.
    /// </remarks>
    public ScrollableListBox(Component thumb, float relativeSize)
        : base()
    {
        this.ScrollBar = new ScrollBar(this.ContentContainer, thumb)
        {
            Parent = this,
            IsEnabled = false,
            Orientation = this.Orientation,
            Transform = { IgnoreParentPadding = true },
            RelativeSize = relativeSize,
        };

        this.ScrollBar.Scrolled += this.ScrollBar_Scrolled;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollableListBox"/> class.
    /// </summary>
    /// <param name="scrollBar">The scroll bar component.</param>
    public ScrollableListBox(ScrollBar scrollBar)
        : base()
    {
        this.ScrollBar = scrollBar;
        this.ScrollBar.Parent = this;
        this.ScrollBar.ContentContainer = this.ContentContainer;
        this.ScrollBar.Scrolled += this.ScrollBar_Scrolled;
    }

    /// <summary>
    /// Occurs when the scroll bar needed state has changed.
    /// </summary>
    public event EventHandler? ScrollBarNeededChanged;

    /// <summary>
    /// Gets the scroll bar component.
    /// </summary>
    public ScrollBar ScrollBar { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// This property also sets the orientation of the scroll bar.
    /// </remarks>
    public override Orientation Orientation
    {
        get => base.Orientation;
        set
        {
            base.Orientation = value;
            this.ScrollBar.Orientation = value;
        }
    }

    /// <summary>
    /// Gets the total length of the list box components.
    /// </summary>
    public int TotalLength
    {
        get
        {
            if (this.isTotalLengthMeasureNeeded)
            {
                this.MeasureTotalLength();
                this.isTotalLengthMeasureNeeded = false;
            }

            return this.totalLength;
        }

        private set
        {
            this.totalLength = value;
            this.ScrollBar.TotalLength = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content
    /// should be also drawn on the parent padding.
    /// </summary>
    public bool DrawContentOnParentPadding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the scroll bar
    /// should be shown even if it is not needed.
    /// </summary>
    public bool ShowScrollBarIfNotNeeded { get; set; }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        // We update all components before recalculating ListBox
        // to act on already calculated components.
        // It is safe, because we disable AutoUpdate for components,
        // so base.Update(GameTime) will not update them again.
        this.UpdateVisibleComponents(gameTime);

        if (this.IsRecalulcationNeeded)
        {
            this.MeasureTotalLength();
        }

        this.UpdateScrollBarPresence();

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void ForceUpdate(bool? withTransform = null)
    {
        base.ForceUpdate(withTransform);
        this.MeasureTotalLength();
        this.UpdateScrollBarPresence();
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        base.Draw(gameTime);

        SpriteBatch spriteBatch = SpriteBatchController.SpriteBatch;
        spriteBatch.End();

        var rasterizerState = new RasterizerState() { ScissorTestEnable = true };
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: BlendState.NonPremultiplied,
            samplerState: null,
            depthStencilState: null,
            rasterizerState: rasterizerState);

        Rectangle tempRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

        Rectangle scissorRect = (this.DrawContentOnParentPadding ? this : this.ContentContainer).Transform.DestRectangle;
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;

        foreach (Component component in this.Components)
        {
            if (this.IsComponentVisible(component))
            {
                component.Draw(gameTime);
            }
        }

        spriteBatch.End();
        rasterizerState.Dispose();
        spriteBatch.GraphicsDevice.ScissorRectangle = tempRectangle;
        spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
    }

    /// <inheritdoc/>
    protected override void RecalculateContentElements(int currentOffset)
    {
        base.RecalculateContentElements(-this.currentScrollOffset);
    }

    /// <inheritdoc/>
    protected override void ContentContainer_ChildAdded(object? sender, ChildChangedEventArgs e)
    {
        // Improve performance by disabling automatic updating and drawing of the
        // child component. It will only be updated and drawn when it is visible.
        // Disabling these options also allows the component
        // to be queued until initialization is done.
        e.Child.AutoUpdate = false;
        e.Child.AutoDraw = false;

        base.ContentContainer_ChildAdded(sender, e);
    }

    private bool IsComponentVisible(IReadOnlyComponent component)
    {
        return component.Transform.DestRectangle.Bottom > this.Transform.DestRectangle.Top
            && component.Transform.DestRectangle.Top < this.Transform.DestRectangle.Bottom;
    }

    private void UpdateVisibleComponents(GameTime gameTime)
    {
        foreach (Component component in this.Components)
        {
            if (this.IsComponentVisible(component))
            {
                component.Update(gameTime);
            }
        }
    }

    private void MeasureTotalLength()
    {
        int spacingLength = this.Spacing * (this.Components.Count() - 1);
        this.TotalLength = spacingLength + this.Components
            .ToList()
            .Select(this.GetComponentLength)
            .Sum();
    }

    private void UpdateScrollBarPresence()
    {
        bool wasScrollBarNeeded = this.isScrollBarNeeded;
        this.isScrollBarNeeded = this.TotalLength > this.ContentContainerLength;

        if (wasScrollBarNeeded != this.isScrollBarNeeded)
        {
            this.ScrollBarNeededChanged?.Invoke(this, EventArgs.Empty);
        }

        this.ScrollBar.IsEnabled = this.isScrollBarNeeded || this.ShowScrollBarIfNotNeeded;
    }

    private void ScrollBar_Scrolled(object? sender, ScrolledEventArgs e)
    {
        this.currentScrollOffset = (int)e.Current;
        this.ForceUpdate(withTransform: false);
    }
}
