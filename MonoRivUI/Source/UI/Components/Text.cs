using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRivUI;

/// <summary>
/// Represents a UI text component.
/// </summary>
internal class Text : TextComponent
{
    private Vector2 destinationLocation;
    private Vector2 unscaledDimensions;
    private Vector2 scaledDimensions;

    private TextFit textFit;
    private float fitScale;
    private float drawScale;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoRivUI.Text"/> class.
    /// </summary>
    /// <param name="color">The color of the displayed text.</param>
    public Text(Color color)
        : base(color)
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
    public override Vector2 UnscaledDimensions
    {
        get
        {
            if (this.isRecalculationNeeded)
            {
                this.Recalculate();
            }

            return this.unscaledDimensions;
        }
    }

    /// <inheritdoc/>
    public override Vector2 ScaledDimensions
    {
        get
        {
            if (this.isRecalculationNeeded)
            {
                this.Recalculate();
            }

            return this.scaledDimensions;
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
        SpriteBatchController.SpriteBatch.DrawString(
            spriteFont: this.Font,
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
    /// Measures the unscaled dimensions of a portion of the text.
    /// </summary>
    /// <param name="startIndex">The index at which to start measuring the text.</param>
    /// <param name="endIndex">The index at which to stop measuring the text (exclusive).</param>
    /// <returns>The unscaled dimensions of the specified portion of the text.</returns>
    /// <remarks>
    /// The unscaled dimensions represent the minimum size of the rectangle
    /// that can contain the unscaled text.
    /// </remarks>
    public Vector2 MeasureUnscaledDimensions(int startIndex, int endIndex)
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

        this.unscaledDimensions = this.Font
            .MeasureString(this.Value)
            .Scale(this.fitScale)
            .Scale(this.Scale);

        this.scaledDimensions = this.unscaledDimensions
            .Scale(ScreenController.Scale);

        this.UpdateDestinationLocation();

        this.drawScale = this.fitScale * this.Scale
            * Math.Min(ScreenController.Scale.X, ScreenController.Scale.Y);

        if (this.AdjustSizeToText)
        {
            this.Transform.SetRelativeSizeFromUnscaledAbsolute(
                this.unscaledDimensions.X,
                this.unscaledDimensions.Y);
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

        float scaleWidth = this.Transform.UnscaledSize.X / defaultDimensions.X;
        float scaleHeight = this.Transform.UnscaledSize.Y / defaultDimensions.Y;

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
        Rectangle sourceRect = this.Transform.UnscaledRectangle;
        var currentRect = new Rectangle(
            this.Transform.UnscaledRectangle.Location,
            this.unscaledDimensions.ToPoint());

        this.destinationLocation = RecalculationUtils.AlignRectangle(
            sourceRect, currentRect, this.TextAlignment)
            .Location.ToVector2().Scale(ScreenController.Scale);
    }
}
