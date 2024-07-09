using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component that displays wrapped text.
/// </summary>
public class WrappedText : TextComponent, IEnumerable<Text>
{
    private readonly List<Text> textLines = new();

    private float lineSpacing;
    private Vector2 dimensions;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedText"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the text.</param>
    public WrappedText(ScalableFont font, Color color)
        : base(font, color)
    {
        this.Transform.SizeChanged += (s, e) =>
        {
            if (e.Before.X != e.After.X)
            {
                this.Recalculate();
            }
        };
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

    /// <inheritdoc/>
    public override AdjustSizeOption AdjustTransformSizeToText
    {
        set
        {
            if (base.AdjustTransformSizeToText == value)
            {
                return;
            }

            base.AdjustTransformSizeToText = value;
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
        get => this.lineSpacing;
        set
        {
            if (this.lineSpacing == value)
            {
                return;
            }

            this.lineSpacing = value;
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
    /// Gets a text line at the specified index.
    /// </summary>
    /// <param name="i">The index of the text line to retrieve.</param>
    /// <returns>The text line at the specified index.</returns>
    public Text this[int i] => this.textLines[i];

    public static explicit operator WrappedText(Text text)
    {
        return new WrappedText(text.Font, text.Color)
        {
            Value = text.Value,
            Scale = text.Scale,
            TextAlignment = text.TextAlignment,
        };
    }

    /// <summary>
    /// Returns an enumerator that iterates through the text lines.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the text lines.
    /// </returns>
    public IEnumerator<Text> GetEnumerator()
    {
        return this.textLines.GetEnumerator();
    }

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

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

    private void Recalculate()
    {
        this.WrapText();
        this.UpdateDimensions();
        this.AdjustSizeToText(this.dimensions);
        this.PositionTextLines();
        this.isRecalculationNeeded = false;
    }

    private void UpdateDimensions()
    {
        float totalY = this.lineSpacing * (this.textLines.Count - 1);
        foreach (Text line in this.textLines.ToList())
        {
            Vector2 lineDimensions = line.Dimensions;
            totalY += lineDimensions.Y;
        }

        var dimX = this.Transform.Size.X;
        this.dimensions = new Vector2(dimX, totalY);
    }

    private void WrapText()
    {
        // Get the reference rectangle
        Rectangle reference = this.Transform.DestRectangle;

        // If the width of the text is smaller than
        // the reference width, no wrapping is needed
        if (this.MeasureText(this.Value).X < reference.Width)
        {
            this.ResetTextLines();
            Text textLine = this.CreateTextLine(this.Value);
            this.textLines.Add(textLine);
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
        while (currentCharIndex < this.Value.Length)
        {
            // Current word being built
            var word = new StringBuilder();

            // Build a word until a whitespace character is encountered
            char whitespace = default;
            while (currentCharIndex < this.Value.Length)
            {
                char character = this.Value[currentCharIndex++];

                // If a whitespace character is encountered, store it and break
                if (char.IsWhiteSpace(character))
                {
                    whitespace = character;
                    break;
                }

                _ = word.Append(character);
            }

            if (whitespace == '\n')
            {
                // Add the current line to the result and reset for the next one
                result.Add(currentLine.ToString() + word.ToString());
                _ = currentLine.Clear();
                _ = word.Clear();
                currentWidth = 0.0f;
                continue;
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

        // Create text components for each wrapped line
        foreach (string line in result)
        {
            Text textLine = this.CreateTextLine(line);
            this.textLines.Add(textLine);
        }
    }

    private Text CreateTextLine(string text)
    {
        var result = new Text(this.Font, this.Color)
        {
            Parent = this,
            Value = text,
            Color = this.Color,
            Scale = this.Scale,
            TextAlignment = this.TextAlignment,
            AdjustTransformSizeToText = AdjustSizeOption.OnlyHeight,
            FixedHeight = (int)this.Font.SafeDimensions.Y,
        };
        return result;
    }

    private void PositionTextLines()
    {
        float currentOffset = 0.0f;
        foreach (Text textLine in this.textLines)
        {
            textLine.Transform.SetRelativeOffsetFromAbsolute(y: currentOffset);
            currentOffset += textLine.Dimensions.Y + this.lineSpacing;
        }
    }

    private void ResetTextLines()
    {
        foreach (Text textLine in this.textLines)
        {
            textLine.Parent = null;
        }

        this.textLines.Clear();
    }

    private Vector2 MeasureText(string text)
    {
        var result = this.Font.MeasureString(text, out var heightOffset);
        result.Y += heightOffset;
        return result;
    }

    private Vector2 MeasureText(StringBuilder text)
    {
        return this.MeasureText(text.ToString());
    }
}
