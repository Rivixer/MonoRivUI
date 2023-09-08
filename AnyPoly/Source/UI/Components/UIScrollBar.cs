using Microsoft.Xna.Framework;
using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents a scroll bar component.
/// </summary>
internal class UIScrollBar : UIComponent
{
    private readonly UIFrame frame;
    private readonly UISolidColor thumb;
    private readonly Orientation orientation;

    private float relativeSize = 0.02f;
    private float total;
    private float current;

    private bool isUpdateThumbSizeNeeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIScrollBar"/> class.
    /// </summary>
    /// <param name="orientation">The oritentation of the scroll bar.</param>
    public UIScrollBar(Orientation orientation)
    {
        this.orientation = orientation;
        this.frame = new UIFrame(Color.Gray) { Parent = this, RelativeThickness = 0.01f };
        this.thumb = new UISolidColor(Color.DarkGray) { Parent = this.frame };
        this.ParentChanged += this.UIScrollBar_ParentChanged;
    }

    /// <summary>
    /// An event that is raised when the scroll bar is scrolled.
    /// </summary>
    public event EventHandler<ScrolledEventArgs>? Scrolled;

    /// <summary>
    /// Gets a value indicating whether
    /// the scroll bar thumb is being dragged.
    /// </summary>
    public bool IsThumbDragging { get; private set; }

    /// <summary>
    /// Gets or sets the color of the scroll bar frame.
    /// </summary>
    public Color FrameColor
    {
        get => this.frame.Color;
        set => this.frame.Color = value;
    }

    /// <summary>
    /// Gets or sets the color of the scroll bar thumb.
    /// </summary>
    public Color ThumbColor
    {
        get => this.thumb.Color;
        set => this.thumb.Color = value;
    }

    /// <summary>
    /// Gets or sets the relative size of the scroll bar.
    /// </summary>
    public float RelativeSize
    {
        get => this.relativeSize;
        set
        {
            if (this.relativeSize == value)
            {
                return;
            }

            this.relativeSize = value;
            this.UpdateScrollBarSize();
        }
    }

    /// <summary>
    /// Gets or sets the total length of the scrollable content.
    /// </summary>
    public float TotalLength
    {
        get => this.total;
        set
        {
            if (this.total == value)
            {
                return;
            }

            this.total = value;
            this.isUpdateThumbSizeNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isUpdateThumbSizeNeeded)
        {
            this.UpdateThumbSize();
            this.UpdateThumbPosition(scrollDelta: 0.0f);
            this.isUpdateThumbSizeNeeded = false;
        }

        this.IsThumbDragging = MouseController.IsLeftButtonPressed()
            && ((MouseController.WasLeftButtonReleased()
                && this.thumb.Transform.ScaledRectangle.Contains(MouseController.Position))
                || this.IsThumbDragging);

        if (this.IsThumbDragging)
        {
            this.HandleThumbDrag();
        }
        else if (this.IsScrollBarClicked()
            && !this.thumb.Transform.ScaledRectangle.Contains(MouseController.Position))
        {
            this.HandleScrollBarClick();
        }
        else if (this.IsScrollWheelScrolledOnParent())
        {
            this.UpdateThumbPosition(MouseController.ScrollDelta);
        }

        base.Update(gameTime);
    }

    private bool IsScrollBarClicked()
    {
        return MouseController.IsLeftButtonClicked() &&
            this.Transform.ScaledRectangle.Contains(MouseController.Position);
    }

    private bool IsScrollWheelScrolledOnParent()
    {
        return (MouseController.ScrollDelta != 0)
            && this.Parent!.Transform.ScaledRectangle.Contains(MouseController.Position);
    }

    private void HandleThumbDrag()
    {
        Point mouseDelta = MouseController.MouseDelta;
        Rectangle frameScaledRect = this.frame.Transform.ScaledRectangle;

        float scrollPercentage = this.orientation switch
        {
            Orientation.Vertical => mouseDelta.Y / (float)frameScaledRect.Height,
            Orientation.Horizontal => mouseDelta.X / (float)frameScaledRect.Width,
            _ => throw new NotImplementedException(),
        };

        this.UpdateThumbPosition(-scrollPercentage * this.total);
    }

    private void HandleScrollBarClick()
    {
        bool clickedAbove = (this.orientation == Orientation.Vertical)
            ? MouseController.Position.Y < this.thumb.Transform.ScaledRectangle.Y
            : MouseController.Position.X < this.thumb.Transform.ScaledRectangle.X;

        Vector2 thumbRelativeSize = this.thumb.Transform.RelativeSize;
        float shiftValue = this.total * this.orientation switch
        {
            Orientation.Vertical => thumbRelativeSize.Y,
            Orientation.Horizontal => thumbRelativeSize.X,
            _ => throw new NotImplementedException(),
        };

        this.UpdateThumbPosition(clickedAbove ? shiftValue : -shiftValue);
    }

    private void UpdateThumbPosition(float scrollDelta)
    {
        float maxCurrentLength = this.total - this.orientation switch
        {
            Orientation.Vertical => this.Transform.UnscaledSize.Y,
            Orientation.Horizontal => this.Transform.UnscaledSize.X,
            _ => throw new NotImplementedException(),
        };

        this.current = Math.Clamp(this.current - scrollDelta, 0.0f, maxCurrentLength);
        this.UpdateThumbOffset();
        this.Scrolled?.Invoke(this, new ScrolledEventArgs(this.current, this.total));
    }

    private void UIScrollBar_ParentChanged(object? sender, ParentChangedEventArgs e)
    {
        if (e.NewParent is null)
        {
            throw new InvalidOperationException("Cannot remove parent from UIScrollbar.");
        }

        this.UpdateScrollBarSize();
    }

    private void UpdateScrollBarSize()
    {
        switch (this.orientation)
        {
            case Orientation.Vertical:
                this.Transform.Alignment = Alignment.Right;
                this.Transform.RelativeSize = new Vector2(this.relativeSize, 1.0f);
                break;
            case Orientation.Horizontal:
                this.Transform.Alignment = Alignment.Bottom;
                this.Transform.RelativeSize = new Vector2(1.0f, this.relativeSize);
                break;
        }
    }

    private void UpdateThumbSize()
    {
        switch (this.orientation)
        {
            case Orientation.Vertical:
                int frameHeight = this.frame.Transform.UnscaledSize.Y;
                float newRelativeThumbHeight = Math.Clamp(frameHeight / this.total, 0.0f, 1.0f);
                this.thumb.Transform.RelativeSize = new Vector2(1.0f, newRelativeThumbHeight);
                break;
            case Orientation.Horizontal:
                int frameWidth = this.frame.Transform.UnscaledSize.X;
                float newRelativeThumbWidth = Math.Clamp(frameWidth / this.total, 0.0f, 1.0f);
                this.thumb.Transform.RelativeSize = new Vector2(newRelativeThumbWidth, 1.0f);
                break;
        }
    }

    private void UpdateThumbOffset()
    {
        float relativeScrollPosition = this.current / this.total;
        this.thumb.Transform.RelativeOffset = this.orientation switch
        {
            Orientation.Vertical => new Vector2(0.0f, relativeScrollPosition),
            Orientation.Horizontal => new Vector2(relativeScrollPosition, 0.0f),
            _ => throw new NotImplementedException(),
        };
    }
}
