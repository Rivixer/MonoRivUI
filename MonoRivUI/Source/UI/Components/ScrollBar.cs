using System;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a scrollbar component.
/// </summary>
public class ScrollBar : Component
{
    private readonly Frame frame;
    private readonly SolidColor thumb;
    private readonly Orientation orientation;
    private readonly IUIReadOnlyComponent contentContainer;

    private float relativeSize = 0.02f;
    private float total;
    private float current;

    private bool isUpdateThumbSizeNeeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    /// <param name="orientation">The oritentation of the scrollbar.</param>
    /// <param name="contentContainer">
    /// The container that contains the content that is scrolled.
    /// </param>
    public ScrollBar(Orientation orientation, IUIReadOnlyComponent contentContainer)
    {
        this.orientation = orientation;
        this.frame = new Frame(Color.Gray, thickness: 2) { Parent = this };
        this.thumb = new SolidColor(Color.DarkGray) { Parent = this.frame.InnerContainer };
        this.ParentChanged += this.UIScrollBar_ParentChanged;
        this.Transform.Recalculated += this.Transform_Recalculated;
        this.contentContainer = contentContainer;
    }

    /// <summary>
    /// An event that is raised when the scrollbar is scrolled.
    /// </summary>
    public event EventHandler<ScrolledEventArgs>? Scrolled;

    /// <summary>
    /// Gets a value indicating whether
    /// the scrollbar thumb is being dragged.
    /// </summary>
    public bool IsThumbDragging { get; private set; }

    /// <summary>
    /// Gets the current scroll position.
    /// </summary>
    /// <remarks>
    /// The value is between <c>0.0f</c> and <c>1.0f</c>
    /// where <c>0.0f</c> is the top and <c>1.0f</c> is the bottom.
    /// </remarks>
    public float Position => this.current / (this.total - this.orientation switch
    {
        Orientation.Vertical => this.contentContainer.Transform.Size.Y,
        Orientation.Horizontal => this.contentContainer.Transform.Size.X,
        _ => throw new NotImplementedException(),
    });

    /// <summary>
    /// Gets or sets the color of the scrollbar frame.
    /// </summary>
    public Color FrameColor
    {
        get => this.frame.Color;
        set => this.frame.Color = value;
    }

    /// <summary>
    /// Gets or sets the color of the scrollbar thumb.
    /// </summary>
    public Color ThumbColor
    {
        get => this.thumb.Color;
        set => this.thumb.Color = value;
    }

    /// <summary>
    /// Gets or sets the relative size of the scrollbar.
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

    /// <summary>
    /// Scrolls to the specified position.
    /// </summary>
    /// <param name="position">The position to scroll to.</param>
    /// <remarks>
    /// The value should be between <c>0.0f</c> and <c>1.0f</c>
    /// where <c>0.0f</c> is the top and <c>1.0f</c> is the bottom.
    /// </remarks>
    public void ScrollTo(float position)
    {
        if (position is < 0.0f or > 1.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "The position should be between 0.0f and 1.0f.");
        }

        float destination = position * this.total;
        this.UpdateThumbPosition(this.current - destination);
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
                    && MouseController.IsComponentFocused(this.thumb))
                || this.IsThumbDragging);

        if (this.IsThumbDragging)
        {
            this.HandleThumbDrag();
        }
        else if (this.IsScrollBarClicked()
            && !MouseController.IsComponentFocused(this.thumb))
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
        return MouseController.IsLeftButtonClicked()
            && MouseController.IsComponentFocused(this);
    }

    private bool IsScrollWheelScrolledOnParent()
    {
        return MouseController.ScrollDelta != 0
            && MouseController.IsComponentFocused(this.contentContainer);
    }

    private void HandleThumbDrag()
    {
        Point mouseDelta = MouseController.MouseDelta;
        Rectangle frameRect = this.frame.InnerContainer.Transform.DestRectangle;

        float scrollPercentage = this.orientation switch
        {
            Orientation.Vertical => mouseDelta.Y / (float)frameRect.Height,
            Orientation.Horizontal => mouseDelta.X / (float)frameRect.Width,
            _ => throw new NotImplementedException(),
        };

        this.UpdateThumbPosition(-scrollPercentage * this.total);
    }

    private void HandleScrollBarClick()
    {
        bool clickedAbove = (this.orientation == Orientation.Vertical)
            ? MouseController.Position.Y < this.thumb.Transform.DestRectangle.Y
            : MouseController.Position.X < this.thumb.Transform.DestRectangle.X;

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
            Orientation.Vertical => this.contentContainer.Transform.Size.Y,
            Orientation.Horizontal => this.contentContainer.Transform.Size.X,
            _ => throw new NotImplementedException(),
        };

        this.current = Math.Clamp(this.current - scrollDelta, 0.0f, maxCurrentLength);
        this.UpdateThumbOffset();
        this.Scrolled?.Invoke(this, new ScrolledEventArgs(this.current, this.total));
    }

    private void UIScrollBar_ParentChanged(object? sender, ParentChangedEventArgs e)
    {
        if (e.Current is null)
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
                int frameHeight = this.contentContainer.Transform.Size.Y;
                float newRelativeThumbHeight = Math.Clamp(frameHeight / this.total, 0.0f, 1.0f);
                this.thumb.Transform.RelativeSize = new Vector2(1.0f, newRelativeThumbHeight);
                break;
            case Orientation.Horizontal:
                int frameWidth = this.contentContainer.Transform.Size.X;
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

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.UpdateScrollBarSize();
        this.UpdateThumbSize();
        this.UpdateThumbOffset();
    }
}
