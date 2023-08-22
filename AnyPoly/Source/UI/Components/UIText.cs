using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI text component.
/// </summary>
internal class UIText : UITextComponent
{
    private Vector2 dimensions;
    private Vector2 destinationLocation;

    private TextFit textFit = TextFit.None;
    private float fitScale = 1.0f;

    private bool isFitScaleUpdateNeeded;
    private bool isDestLocationUpdateNeeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIText"/> class.
    /// </summary>
    /// <param name="color">The color of the displayed text.</param>
    public UIText(Color color)
        : base(color)
    {
        this.Transform.Recalculated += this.Transform_Recalculated;
    }

    /// <inheritdoc/>
    public override string Text
    {
        set
        {
            if (base.Text == value)
            {
                return;
            }

            base.Text = value;
            this.dimensions = this.MeasureDimensions();
            this.isFitScaleUpdateNeeded = true;
            this.isDestLocationUpdateNeeded = true;
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
            this.isDestLocationUpdateNeeded = true;
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
            this.isDestLocationUpdateNeeded = true;
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
            this.isFitScaleUpdateNeeded = true;
            this.isDestLocationUpdateNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override Vector2 GetScaledDimensions()
    {
        return this.GetScaledDimensions(0, this.Text.Length);
    }

    /// <summary>
    /// Returns the scaled dimensions of a portion of the text.
    /// </summary>
    /// <param name="startIndex">The index at which to start measuring the text.</param>
    /// <param name="endIndex">The index at which to stop measuring the text.</param>
    /// <returns>
    /// The scaled dimensions of the specified portion of the text,
    /// corresponds to the size of the text.
    /// </returns>
    public Vector2 GetScaledDimensions(int startIndex, int endIndex)
    {
        return this.Font
            .MeasureString(this.Text[startIndex..endIndex])
            .Scale(this.fitScale)
            .Scale(this.Scale)
            .Scale(ScreenController.Scale);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isFitScaleUpdateNeeded)
        {
            this.UpdateFitScale();
        }

        if (this.isDestLocationUpdateNeeded)
        {
            this.UpdateDestinationLocation();
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatchController.SpriteBatch.DrawString(
            spriteFont: this.Font,
            text: this.Text,
            position: this.destinationLocation,
            color: this.Color,
            rotation: 0.0f,
            origin: Vector2.Zero,
            scale: Math.Min(ScreenController.Scale.X, ScreenController.Scale.Y) * this.fitScale * base.Scale,
            effects: SpriteEffects.None,
            layerDepth: 0.0f);

        base.Draw(gameTime);
    }

    private void UpdateFitScale()
    {
        if (this.dimensions.X == 0.0f || this.dimensions.Y == 0.0f)
        {
            return;
        }

        float scaleWidth = this.Transform.UnscaledSize.X / this.dimensions.X;
        float scaleHeight = this.Transform.UnscaledSize.Y / this.dimensions.Y;

        this.fitScale = this.textFit switch
        {
            TextFit.None => 1.0f,
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
            this.dimensions.Scale(this.fitScale * this.Scale).ToPoint());

        this.destinationLocation = RecalculationUtils.AlignRectangle(
            sourceRect, currentRect, this.TextAlignment)
            .Location.ToVector2().Scale(ScreenController.Scale);
    }

    private Vector2 MeasureDimensions()
    {
        return this.Font.MeasureString(this.Text);
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.UpdateFitScale();
        this.destinationLocation = this.Transform.UnscaledLocation.ToVector2();
        this.UpdateDestinationLocation();
    }
}
