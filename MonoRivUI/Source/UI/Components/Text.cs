using System;
using System.Diagnostics;
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

    private TextFit textFit;
    private float fitScale;
    private float drawScale;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the displayed text.</param>
    public Text(ScalableFont font, Color color)
        : base(font, color)
    {
        this.Transform.Recalculated += (s, e) => this.Recalculate();
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
    /// Gets or sets the text fitting behavior.
    /// </summary>
    public TextFit TextFit
    {
        get => this.textFit;
        set
        {
            if (this.textFit == value)
            {
                return;
            }

            this.textFit = value;
            this.isRecalculationNeeded = true;
        }
    }

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
        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
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
        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        return this.Font
            .MeasureString(this.Value[startIndex..endIndex])
            .Scale(this.fitScale)
            .Scale(this.Scale);
    }

    private void Recalculate()
    {
        this.UpdateFitScale();

        this.dimensions = this.Font
            .MeasureString(this.Value)
            .Scale(this.fitScale)
            .Scale(this.Scale);

        this.UpdateDestinationLocation();

        this.drawScale = this.fitScale * this.Scale;

        if (this.AdjustSizeToText)
        {
            this.Transform.SetRelativeSizeFromAbsolute(
                x: this.dimensions.X, y: this.dimensions.Y);
        }

        this.isRecalculationNeeded = false;
    }

    private void UpdateFitScale()
    {
        if (this.textFit is TextFit.None || this.AdjustSizeToText)
        {
            this.fitScale = 1.0f;
            return;
        }

        Vector2 defaultDimensions = this.Font.MeasureString(this.Value);

        if (defaultDimensions == Vector2.Zero)
        {
            this.fitScale = 1.0f;
            return;
        }

        float scaleWidth = this.Transform.Size.X / defaultDimensions.X;
        float scaleHeight = this.Transform.Size.Y / defaultDimensions.Y;

        this.fitScale = this.textFit switch
        {
            TextFit.Width => scaleWidth,
            TextFit.Height => scaleHeight,
            TextFit.Both => Math.Min(scaleWidth, scaleHeight),
            _ => this.fitScale,
        };
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
}
