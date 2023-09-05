using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component that displays wrapped text.
/// </summary>
internal class UIWrappedText : UITextComponent
{
    private readonly List<UIText> textLines = new();

    private float unscaledLineSpacing;
    private float scaledLineSpacing;

    private Vector2 unscaledDimensions;
    private Vector2 scaledDimensions;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIWrappedText"/> class.
    /// </summary>
    /// <param name="color">The color of the text.</param>
    public UIWrappedText(Color color)
        : base(color)
    {
        this.Transform.SizeChanged += (s, e) => this.Recalculate();
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
    /// Gets or sets the line spacing of the text.
    /// </summary>
    /// <remarks>
    /// The line spacing is the distance between two lines of text.
    /// Measure in pixels.
    /// </remarks>
    public float LineSpacing
    {
        get => this.unscaledLineSpacing;
        set
        {
            if (this.unscaledLineSpacing == value)
            {
                return;
            }

            this.unscaledLineSpacing = value;
            this.scaledLineSpacing = value * ScreenController.Scale.Y;
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
    /// Gets a text line at the specified index.
    /// </summary>
    /// <param name="i">The index of the text line to retrieve.</param>
    /// <returns>The text line at the specified index.</returns>
    public UIText this[int i] => this.textLines[i];

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isRecalculationNeeded)
        {
            this.Recalculate();
        }

        base.Update(gameTime);
    }

    private void Recalculate()
    {
        this.WrapText();
        this.UpdateDimensions();

        if (this.AdjustSizeToText)
        {
            this.Transform.SetRelativeSizeFromUnscaledAbsolute(
                this.unscaledDimensions.X,
                this.unscaledDimensions.Y);
        }

        this.isRecalculationNeeded = false;
    }

    private void UpdateDimensions()
    {
        float maximumX = 0.0f;
        float totalY = this.unscaledLineSpacing * (this.textLines.Count - 1);

        foreach (UIText line in this.textLines)
        {
            Vector2 lineDimensions = line.UnscaledDimensions;
            maximumX = Math.Max(maximumX, lineDimensions.X);
            totalY += lineDimensions.Y;
        }

        this.unscaledDimensions = new Vector2(maximumX, totalY);
        this.scaledDimensions = this.unscaledDimensions
            .Scale(ScreenController.Scale);
    }

    private void WrapText()
    {
        // Get the reference rectangle
        Rectangle reference = this.Transform.UnscaledRectangle;

        // If the width of the text is smaller than
        // the reference width, no wrapping is needed
        if (this.MeasureText(this.Text).X < reference.Width)
        {
            this.ResetTextLines();
            var text = new UIText(this.Color)
            {
                Parent = this,
                Text = this.Text,
                Scale = this.Scale,
                TextAlignment = this.TextAlignment,
            };
            this.textLines.Add(text);
            return;
        }

        // Store the wrapped text lines
        var result = new List<string>();

        // Current line being built
        var currentLine = new StringBuilder();

        // Current width of the line being built
        float currentWidth = 0.0f;

        // Current character index in the text
        int currentCharIndex = 0;

        // Iterate through the text to wrap it into lines
        while (currentCharIndex < this.Text.Length)
        {
            // Current word being built
            var word = new StringBuilder();

            // Build a word until a whitespace character is encountered
            char whitespace = default;
            while (currentCharIndex < this.Text.Length)
            {
                char character = this.Text[currentCharIndex++];

                // If a whitespace character is encountered, store it and break
                if (char.IsWhiteSpace(character))
                {
                    whitespace = character;
                    break;
                }

                _ = word.Append(character);
            }

            // Measure the width of the current word
            float wordWidth = this.MeasureText(word).X;

            // If adding the current word would exceed the available width
            float remainingWidth = reference.Width - currentWidth;
            if (wordWidth > remainingWidth)
            {
                if (currentLine.Length > 0)
                {
                    // Add the current line to the result and reset for the next one
                    result.Add(currentLine.ToString());
                    _ = currentLine.Clear();
                }
                else
                {
                    // Add the current word to the result, even if it's too long
                    // TODO: Split the word if it's too long
                    result.Add(word.ToString());
                    _ = word.Clear();
                }

                currentWidth = 0.0f;
            }

            // Append the word and its width to the current line
            _ = currentLine.Append(word);
            currentWidth += wordWidth;

            // If a whitespace character was encountered earlier, add it to the current line
            if (whitespace != default)
            {
                _ = currentLine.Append(whitespace);
                float whitespaceWidth = this.MeasureText(whitespace.ToString()).X;
                currentWidth += whitespaceWidth;
            }
        }

        // Add the last built line to the result if it's not empty
        if (currentLine.Length > 0)
        {
            result.Add(currentLine.ToString());
        }

        this.ResetTextLines();

        // Initialize the offset for positioning the wrapped lines
        float currentOffset = 0.0f;

        // Create text components for each wrapped line
        // and position them accordingly
        foreach (string line in result)
        {
            var text = new UIText(this.Color)
            {
                Parent = this,
                Text = line,
                Scale = this.Scale,
                TextAlignment = this.TextAlignment,
            };

            // Position the text using the current offset
            text.Transform.SetRelativeOffsetFromScaledAbsolute(y: currentOffset);

            this.textLines.Add(text);

            // Update the offset for the next line
            currentOffset += text.ScaledDimensions.Y + this.scaledLineSpacing;
        }
    }

    private void ResetTextLines()
    {
        foreach (UIText textLine in this.textLines)
        {
            textLine.Parent = null;
        }

        this.textLines.Clear();
    }

    private Vector2 MeasureText(string text)
    {
        return this.Font.MeasureString(text) * this.Scale;
    }

    private Vector2 MeasureText(StringBuilder text)
    {
        return this.Font.MeasureString(text) * this.Scale;
    }
}
