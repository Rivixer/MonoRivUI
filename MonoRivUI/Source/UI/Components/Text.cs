using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a UI text component.
/// </summary>
public class Text : TextComponent
{
    private Vector2 destinationLocation;
    private Vector2 dimensions;
    private float heightOffset;

    private float drawScale;
    private float shrinkScale;

    private TextShrinkMode textShrink;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the displayed text.</param>
    public Text(ScalableFont font, Color color)
        : base(font, color)
    {
        this.Transform.Recalculated += this.Transform_Recalculated;
    }

    /// <inheritdoc/>
    public override string Value
    {
        set
        {
            if (base.Value == value)
            {
                return;
            }

            base.Value = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override float Scale
    {
        set
        {
            if (base.Scale == value)
            {
                return;
            }

            base.Scale = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override Alignment TextAlignment
    {
        set
        {
            if (base.TextAlignment == value)
            {
                return;
            }

            base.TextAlignment = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the text shrink mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The text shrink mode determines how the text is scaled
    /// to fit the transform size if it goes beyond the bounds.
    /// </para>
    /// <para>
    /// If the <see cref="TextComponent.AdjustTransformSizeToText"/> property is not
    /// set to <see cref="AdjustSizeOption.None"/>, the text shrink mode is ignored.
    /// </para>
    /// <para>
    /// If the mode is set to <see cref="TextShrinkMode.SafeCharHeight"/>,
    /// the text height does not exceed the height, considering safe dimensions.
    /// The <see cref="FixedHeight"/> will be overridden by the safe height.
    /// </para>
    /// </remarks>
    public TextShrinkMode TextShrink
    {
        get => this.textShrink;
        set
        {
            if (this.textShrink == value)
            {
                return;
            }

            this.textShrink = value;
            this.isRecalculationNeeded = true;
        }
    }

    /// <remarks>
    /// <inheritdoc/>
    /// <para>
    /// If the <see cref="FixedHeight"/> property is set, the dimensions
    /// contain the current width of the text and this height.
    /// </para>
    /// </remarks>
    /// <inheritdoc/>
    public override Vector2 Dimensions
    {
        get
        {
            if (this.isRecalculationNeeded)
            {
                this.Recalculate();
            }

            return this.dimensions;
        }
    }

    /// <summary>
    /// Gets or sets the spacing between characters.
    /// </summary>
    public float? Spacing { get; set; }

    /// <summary>
    /// Gets or sets the fixed height of the text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If it is set, the <see cref="Dimensions"/> property contains
    /// the current width of the text and this height.
    /// </para>
    /// <para>
    /// If it is <see langword="null"/>, the <see cref="Dimensions"/>
    /// property contains the current width and height of the text.
    /// </para>
    /// <para>
    /// If the <see cref="TextShrink"/> property is set to
    /// <see cref="TextShrinkMode.SafeCharHeight"/>, the fixed height
    /// will be overridden by the safe height.
    /// </para>
    /// </remarks>
    public int? FixedHeight { get; set; }

    /// <summary>
    /// Gets a character at the specified index within the text.
    /// </summary>
    /// <param name="i">The index of the character to get.</param>
    /// <returns>The character at the specified index.</returns>
    public char this[int i] => this.Value[i];

    /// <summary>
    /// Retrieves a substring of the text specified by the given range.
    /// </summary>
    /// <param name="range">
    /// The range indicating the portion of the text to retrieve.
    /// </param>
    /// <returns>
    /// A substring of the text based on the specified range.
    /// </returns>
    public string this[Range range] => this.Value[range];

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        this.Font.DrawString(
            text: this.Value,
            position: this.destinationLocation,
            color: this.Color,
            rotation: 0.0f,
            origin: Vector2.Zero,
            scale: this.drawScale,
            effects: SpriteEffects.None,
            layerDepth: 0.0f,
            spacing: this.Spacing);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Measures the dimensions of a portion of the text.
    /// </summary>
    /// <param name="startIndex">The index at which to start measuring the text.</param>
    /// <param name="endIndex">The index at which to stop measuring the text (exclusive).</param>
    /// <returns>The dimensions of the specified portion of the text.</returns>
    /// <remarks>
    /// The dimensions represent the minimum size of the rectangle
    /// that can contain the text.
    /// </remarks>
    public Vector2 MeasureDimensions(int startIndex, int endIndex)
    {
        return this.MeasureDimensions(startIndex, endIndex, out _);
    }

    /// <summary>
    /// Measures the dimensions of a portion of the text.
    /// </summary>
    /// <param name="startIndex">The index at which to start measuring the text.</param>
    /// <param name="endIndex">The index at which to stop measuring the text (exclusive).</param>
    /// <param name="heightOffset">
    /// The difference between the height of the text and the height of the base character.
    /// </param>
    /// <returns>The dimensions of the specified portion of the text.</returns>
    /// <remarks>
    /// The dimensions represent the minimum size of the rectangle
    /// that can contain the text.
    /// </remarks>
    public Vector2 MeasureDimensions(int startIndex, int endIndex, out float heightOffset)
    {
        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        return this.Font.MeasureString(this.Value[startIndex..endIndex], out heightOffset, this.Spacing) * this.drawScale;
    }

    private void Recalculate()
    {
        this.UpdateShrinkScale();
        this.drawScale = this.Scale * this.shrinkScale;

        this.dimensions = this.Font
            .MeasureString(this.Value, out this.heightOffset, this.Spacing * this.drawScale)
            .Scale(this.drawScale);

        this.dimensions.Y -= this.heightOffset;

        if (this.FixedHeight is { } height)
        {
            this.dimensions.Y = height;
        }

        if (this.TextShrink is TextShrinkMode.SafeCharHeight)
        {
            this.dimensions.Y = Math.Min(this.FixedHeight ?? this.dimensions.Y, this.Transform.Size.Y / ScalableFont.SafeFactor);
        }

        this.UpdateDestinationLocation();
        this.AdjustSizeToText(this.dimensions);

        this.isRecalculationNeeded = false;
    }

    private void UpdateDestinationLocation()
    {
        Rectangle sourceRect = this.Transform.DestRectangle;
        var currentRect = new Rectangle(
            this.Transform.DestRectangle.Location,
            this.dimensions.ToPoint());

        this.destinationLocation = RecalculationUtils.AlignRectangle(
            sourceRect, currentRect, this.TextAlignment)
            .Location.ToVector2();
    }

    private void UpdateShrinkScale()
    {
        if (this.TextShrink is TextShrinkMode.None || this.AdjustTransformSizeToText is not AdjustSizeOption.None)
        {
            this.shrinkScale = 1.0f;
            return;
        }

        if (this.TextShrink is TextShrinkMode.SafeCharHeight)
        {
            this.shrinkScale = Math.Min(1f, this.Transform.Size.Y / this.Font.SafeDimensions.Y);
            return;
        }

        Vector2 nativeDimensions = this.Font.MeasureString(this.Value, this.Spacing);
        float scaleX = this.Transform.Size.X / nativeDimensions.X;
        float scaleY = this.Transform.Size.Y / nativeDimensions.Y;

        this.shrinkScale = this.TextShrink switch
        {
            TextShrinkMode.Width => Math.Min(1f, scaleX),
            TextShrinkMode.Height => Math.Min(1f, scaleY),
            TextShrinkMode.HeightAndWidth => Math.Min(1f, Math.Min(scaleX, scaleY)),
            _ => this.shrinkScale,
        };
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.Recalculate();
    }
}
