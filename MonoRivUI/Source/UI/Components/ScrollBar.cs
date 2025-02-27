using System;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a scrollbar component.
/// </summary>
public class ScrollBar : Component, IDragable, IStyleable<ScrollBar>
{
    private TextureComponent? thumb;
    private Container? contentContainer;

    private float relativeSize = 0.02f;
    private float total;
    private float current;
    private Orientation orientation;

    private float thumbDragOffset;

    private bool isUpdateThumbSizeNeeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor does not set the content container
    /// nor the thumb component. Remember to set them using
    /// the <see cref="ContentContainer"/> and <see cref="Thumb"/>
    /// properties.
    /// </remarks>
    public ScrollBar()
        : base()
    {
        this.ParentChanged += this.UIScrollBar_ParentChanged;
        this.Transform.Recalculated += this.Transform_Recalculated;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    /// <param name="contentContainer">
    /// The container that contains the content that is scrolled.
    /// </param>
    /// <remarks>
    /// Using this constructor, remember to set the thumb component
    /// using the <see cref="Thumb"/> property.
    /// </remarks>
    public ScrollBar(Container contentContainer)
        : this()
    {
        this.contentContainer = contentContainer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    /// <param name="thumb">The thumb component of the scrollbar.</param>
    /// <remarks>
    /// Using this constructor, remember to set the content container
    /// using the <see cref="ContentContainer"/> property.
    /// </remarks>
    public ScrollBar(TextureComponent thumb)
        : this()
    {
        this.thumb = thumb;
        this.thumb.Parent = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollBar"/> class.
    /// </summary>
    /// <param name="contentContainer">
    /// The container that contains the content that is scrolled.
    /// </param>
    /// <param name="thumb">The thumb component of the scrollbar.</param>
    public ScrollBar(Container contentContainer, TextureComponent thumb)
        : this()
    {
        this.thumb = thumb;
        this.thumb.Parent = this;
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
    /// Gets a value indicating whether
    /// the scrollbar thumb was being dragged.
    /// </summary>
    public bool WasThumbDragging { get; private set; }

    /// <summary>
    /// Gets or sets the content container
    /// that contains the content that is scrolled.
    /// </summary>
    public Container ContentContainer
    {
        get => this.contentContainer
            ?? throw new InvalidOperationException(
                "The content container is not set. " +
                $"Use the {nameof(this.ContentContainer)} property " +
                $"or the constructor with the {nameof(this.contentContainer)} parameter " +
                "to set the content container.");
        set => this.contentContainer = value;
    }

    /// <summary>
    /// Gets or sets the thumb component of the scrollbar.
    /// </summary>
    public TextureComponent Thumb
    {
        get => this.thumb
            ?? throw new InvalidOperationException(
                "The thumb component is not set. " +
                $"Use the {nameof(this.Thumb)} property " +
                $"or the constructor with the {nameof(this.thumb)} parameter " +
                "to set the thumb component.");
        set
        {
            if (this.thumb == value)
            {
                return;
            }

            if (this.thumb is not null && this.IsParentOf(this.thumb))
            {
                this.thumb.Parent = null;
            }

            this.thumb = value;
            this.thumb.Parent = this;
        }
    }

    /// <summary>
    /// Gets or sets the orientation of the scrollbar.
    /// </summary>
    public Orientation Orientation
    {
        get => this.orientation;
        set
        {
            this.orientation = value;
            this.UpdateScrollBarSize();
        }
    }

    /// <inheritdoc/>
    bool IDragable.IsDragging => this.IsThumbDragging;

    /// <inheritdoc/>
    bool IDragable.WasDragging => this.WasThumbDragging;

    /// <summary>
    /// Gets the position of the scrollbar.
    /// </summary>
    /// <remarks>
    /// The value is between <c>0.0f</c> and <c>1.0f</c>
    /// where <c>0.0f</c> is the top and <c>1.0f</c> is the bottom.
    /// </remarks>
    public float Position => this.current / (this.total - this.Orientation switch
    {
        Orientation.Vertical => this.ContentContainer.Transform.Size.Y,
        Orientation.Horizontal => this.ContentContainer.Transform.Size.X,
        _ => throw new NotImplementedException(),
    });

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

    /// <inheritdoc/>
    public void ApplyStyle(Style<ScrollBar> style)
    {
        style.Apply(this);
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
    public void UpdateDragState()
    {
        this.WasThumbDragging = this.IsThumbDragging;
        this.IsThumbDragging = MouseController.IsLeftButtonPressed()
            && ((MouseController.WasLeftButtonReleased()
                    && MouseController.IsComponentFocused(this.Thumb))
                || this.IsThumbDragging);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.isUpdateThumbSizeNeeded)
        {
            this.UpdateThumbSize();
            this.UpdateThumbPosition(scrollDelta: 0.0f);
            this.isUpdateThumbSizeNeeded = false;
        }

        if (!this.WasThumbDragging && this.IsThumbDragging)
        {
            this.thumbDragOffset = 0.0f;
        }

        if (this.IsThumbDragging)
        {
            this.HandleThumbDrag();
        }
        else if (this.IsScrollBarClicked()
            && !MouseController.IsComponentFocused(this.Thumb))
        {
            this.HandleScrollBarClick();
        }
        else if (this.IsScrollWheelScrolled())
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

    private bool IsScrollWheelScrolled()
    {
        return MouseController.ScrollDelta != 0
            && (MouseController.IsComponentFocused(this.ContentContainer)
                || MouseController.IsComponentFocused(this));
    }

    private void HandleThumbDrag()
    {
        int mouseDelta = this.Orientation switch
        {
            Orientation.Vertical => MouseController.MouseDelta.Y,
            Orientation.Horizontal => MouseController.MouseDelta.X,
            _ => throw new NotImplementedException(),
        };

        float scrollPercentage = (float)mouseDelta / this.Orientation switch
        {
            Orientation.Vertical => this.Transform.Size.Y,
            Orientation.Horizontal => this.Transform.Size.X,
            _ => throw new NotImplementedException(),
        };

        int dragOffsetSign = Math.Sign(this.thumbDragOffset);

        if (this.thumbDragOffset != 0f)
        {
            this.thumbDragOffset -= mouseDelta;
        }

        if (this.thumbDragOffset == 0 || dragOffsetSign != Math.Sign(this.thumbDragOffset))
        {
            this.thumbDragOffset = 0;
            this.UpdateThumbPosition(-scrollPercentage * this.total, out float clampedValue);
            if (clampedValue != 0f)
            {
                this.thumbDragOffset -= (int)(clampedValue / (scrollPercentage * this.total) * mouseDelta);
            }
        }
    }

    private void HandleScrollBarClick()
    {
        bool clickedAbove = (this.Orientation == Orientation.Vertical)
            ? MouseController.Position.Y < this.Thumb.Transform.DestRectangle.Y
            : MouseController.Position.X < this.Thumb.Transform.DestRectangle.X;

        Vector2 thumbRelativeSize = this.Thumb.Transform.RelativeSize;
        float shiftValue = this.total * this.Orientation switch
        {
            Orientation.Vertical => thumbRelativeSize.Y,
            Orientation.Horizontal => thumbRelativeSize.X,
            _ => throw new NotImplementedException(),
        };

        this.UpdateThumbPosition(clickedAbove ? shiftValue : -shiftValue);
    }

    private void UpdateThumbPosition(float scrollDelta)
    {
        this.UpdateThumbPosition(scrollDelta, out _);
    }

    private void UpdateThumbPosition(float scrollDelta, out float clampedValue)
    {
        float maxCurrentLength = this.total - this.Orientation switch
        {
            Orientation.Vertical => this.ContentContainer.Transform.Size.Y,
            Orientation.Horizontal => this.ContentContainer.Transform.Size.X,
            _ => throw new NotImplementedException(),
        };

        float value = this.current - scrollDelta;
        clampedValue = value < 0 ? value : value > maxCurrentLength ? value - maxCurrentLength : 0;

        if (maxCurrentLength < 0)
        {
            return;
        }

        var previousCurrent = this.current;
        this.current = Math.Clamp(this.current - scrollDelta, 0.0f, maxCurrentLength);
        var clampedDelta = this.current - previousCurrent;

        this.UpdateThumbOffset();

        var scrolledArgs = new ScrolledEventArgs(this.current, this.total, clampedDelta);
        this.Scrolled?.Invoke(this, scrolledArgs);
    }

    private void UIScrollBar_ParentChanged(object? sender, ParentChangedEventArgs e)
    {
        if (e.Current is null)
        {
            throw new InvalidOperationException("Cannot remove parent from Scrollbar.");
        }

        this.UpdateScrollBarSize();
    }

    private void UpdateScrollBarSize()
    {
        switch (this.Orientation)
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
        // Due to the transform recalculation,
        // the content container is sometimes null
        if (this.ContentContainer is null)
        {
            return;
        }

        switch (this.Orientation)
        {
            case Orientation.Vertical:
                int frameHeight = this.ContentContainer.Transform.Size.Y;
                float newRelativeThumbHeight = Math.Clamp(frameHeight / this.total, 0.0f, 1.0f);
                this.Thumb.Transform.RelativeSize = new Vector2(1.0f, newRelativeThumbHeight);
                break;
            case Orientation.Horizontal:
                int frameWidth = this.ContentContainer.Transform.Size.X;
                float newRelativeThumbWidth = Math.Clamp(frameWidth / this.total, 0.0f, 1.0f);
                this.Thumb.Transform.RelativeSize = new Vector2(newRelativeThumbWidth, 1.0f);
                break;
        }
    }

    private void UpdateThumbOffset()
    {
        float relativeScrollPosition = this.current / this.total;
        this.Thumb.Transform.RelativeOffset = this.Orientation switch
        {
            Orientation.Vertical => new Vector2(0.0f, relativeScrollPosition),
            Orientation.Horizontal => new Vector2(relativeScrollPosition, 0.0f),
            _ => throw new NotImplementedException(),
        };
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.UpdateScrollBarSize();
        this.UpdateThumbSize();
        this.UpdateThumbOffset();
    }
}
