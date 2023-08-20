using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component that displays wrapped text.
/// </summary>
internal class UIWrappedText : UITextComponent
{
    private readonly List<UIText> textLines = new List<UIText>();
    private float lineSpacing;

    private bool isWrapUpdateNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIWrappedText"/> class.
    /// </summary>
    /// <param name="color">The color of the text.</param>
    public UIWrappedText(Color color)
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
            this.isWrapUpdateNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override float Scale
    {
        set
        {
            if (this.Scale == value)
            {
                return;
            }

            base.Scale = value;
            this.isWrapUpdateNeeded = true;
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
            this.isWrapUpdateNeeded = true;
        }
    }

    /// <summary>
    /// Gets or sets the line spacing of the text.
    /// </summary>
    public float LineSpacing
    {
        get => this.lineSpacing;
        set
        {
            if (this.lineSpacing == value)
            {
                return;
            }

            this.lineSpacing = value;
            this.isWrapUpdateNeeded = true;
        }
    }

    /// <inheritdoc/>
    public override Vector2 GetScaledDimensions()
    {
        float maximumX = 0.0f;
        float totalY = this.lineSpacing * (this.textLines.Count - 1);

        foreach (UIText line in this.textLines)
        {
            Vector2 lineDimensions = line.GetScaledDimensions();
            maximumX = Math.Max(maximumX, lineDimensions.X);
            totalY += lineDimensions.Y;
        }

        return new Vector2(maximumX, totalY);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isWrapUpdateNeeded)
        {
            this.WrapText();
            this.isWrapUpdateNeeded = false;
        }

        base.Update(gameTime);
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

                word.Append(character);
            }

            // Measure the width of the current word
            float wordWidth = this.MeasureText(word).X;

            // If adding the current word would exceed the available width
            if (reference.Width - currentWidth < wordWidth)
            {
                // Add the current line to the result and reset for the next one
                if (currentLine.Length > 0)
                {
                    result.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentWidth = 0.0f;
                }
                else
                {
                    // Add the current word to the result, even if it's too long
                    // TODO: Split the word if it's too long
                    result.Add(word.ToString());
                    word.Clear();
                    currentWidth = 0.0f;
                }
            }

            // Append the word and its width to the current line
            currentLine.Append(word);
            currentWidth += wordWidth;

            // If a whitespace character was encountered earlier, add it to the current line
            if (whitespace != default)
            {
                currentLine.Append(whitespace);
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
        Vector2 currentOffset = Vector2.Zero;

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
            text.Transform.SetRelativeOffsetFromScaledAbsolute(currentOffset);

            this.textLines.Add(text);

            // Update the offset for the next line
            currentOffset.Y += text.GetScaledDimensions().Y
                + (this.lineSpacing * ScreenController.Scale.Y);
        }
    }

    private void ResetTextLines()
    {
        foreach (UIText textLine in this.textLines)
        {
            textLine.Parent = null;
        }
    }

    private Vector2 MeasureText(string text)
    {
        return this.Font.MeasureString(text) * this.Scale;
    }

    private Vector2 MeasureText(StringBuilder text)
    {
        return this.Font.MeasureString(text) * this.Scale;
    }

    private void Transform_Recalculated(object? sender, EventArgs e)
    {
        this.isWrapUpdateNeeded = true;
    }
}
