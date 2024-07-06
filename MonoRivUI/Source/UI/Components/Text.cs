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
    /// If the <see cref="AdjustTransformSizeToText"/> property is not set to
    /// <see cref="AdjustSizeOption.None"/>, the text shrink mode is ignored.
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

            return this.FixedHeight.HasValue
                ? new Vector2(this.dimensions.X, this.FixedHeight.Value)
                : this.dimensions;
        }
    }

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

        this.Font.DrawString(
            text: this.Value,
            position: this.destinationLocation,
            color: this.Color,
            rotation: 0.0f,
            origin: Vector2.Zero,
            scale: this.drawScale,
            effects: SpriteEffects.None,
            layerDepth: 0.0f);

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

    /// <inheritdoc cref="MeasureDimensions(int, int)"/>
    /// <param name="heightOffset">
    /// The difference between the height of the text and the height of the base character.
    /// </param>
    public Vector2 MeasureDimensions(int startIndex, int endIndex, out float heightOffset)
    {
        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        return this.Font.MeasureString(this.Value[startIndex..endIndex], out heightOffset);
    }

    private void Recalculate()
    {
        this.UpdateShrinkScale();
        this.drawScale = this.Scale * this.shrinkScale;

        this.dimensions = this.Font
            .MeasureString(this.Value, out this.heightOffset)
            .Scale(this.Scale);

        this.UpdateDestinationLocation();
        this.AdjustSizeToText(this.dimensions);

        this.isRecalculationNeeded = false;
    }

    private void UpdateDestinationLocation()
    {
        Rectangle sourceRect = this.Transform.DestRectangle;
        var currentRect = new Rectangle(
            this.Transform.DestRectangle.Location,
            ((this.dimensions * this.drawScale) - new Vector2(0, this.heightOffset)).ToPoint());

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

        Vector2 nativeDimensions = this.Font.MeasureString(this.Value);

        float scaleX = this.Transform.Size.X / nativeDimensions.X;
        float scaleY = this.Transform.Size.Y / nativeDimensions.Y;

        this.shrinkScale = this.TextShrink switch
        {
            TextShrinkMode.Width => Math.Min(1f, scaleX),
            TextShrinkMode.Height => Math.Min(1f, scaleY),
            TextShrinkMode.HeightAndWidth => Math.Min(1f, Math.Min(scaleX, scaleY)),
            _ => 1.0f,
        };
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.Recalculate();
    }
}
