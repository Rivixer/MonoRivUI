using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component that displays wrapped text.
/// </summary>
public class WrappedText : TextComponent, IEnumerable<Text>
{
    private readonly List<Text> textLines = new();

    private float unscaledLineSpacing;
    private float scaledLineSpacing;

    private Vector2 unscaledDimensions;
    private Vector2 scaledDimensions;

    private bool isRecalculationNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedText"/> class.
    /// </summary>
    /// <param name="color">The color of the text.</param>
    public WrappedText(Color color)
        : base(color)
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
    public override bool AdjustSizeToText
    {
        set
        {
            if (base.AdjustSizeToText == value)
            {
                return;
            }

            base.AdjustSizeToText = value;
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
    public Text this[int i] => this.textLines[i];

    public static explicit operator WrappedText(Text text)
    {
        return new WrappedText(text.Color)
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
                y: this.unscaledDimensions.Y);
        }

        this.PositionTextLines();
        this.isRecalculationNeeded = false;
    }

    private void UpdateDimensions()
    {
        float totalY = this.unscaledLineSpacing * (this.textLines.Count - 1);
        foreach (Text line in this.textLines)
        {
            Vector2 lineDimensions = line.UnscaledDimensions;
            totalY += lineDimensions.Y;
        }

        var dimX = this.Transform.UnscaledSize.X;
        this.unscaledDimensions = new Vector2(dimX, totalY);
        this.scaledDimensions = this.unscaledDimensions
            .Scale(ScreenController.Scale);
    }

    private void WrapText()
    {
        // Get the reference rectangle
        Rectangle reference = this.Transform.UnscaledRectangle;

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
        return new Text(this.Color)
        {
            Parent = this,
            Value = text,
            Color = this.Color,
            Scale = this.Scale,
            TextAlignment = this.TextAlignment,
            AdjustSizeToText = this.AdjustSizeToText,
        };
    }

    private void PositionTextLines()
    {
        float currentOffset = 0.0f;
        foreach (Text textLine in this.textLines)
        {
            textLine.Transform.SetRelativeOffsetFromScaledAbsolute(y: currentOffset);
            currentOffset += textLine.ScaledDimensions.Y + this.scaledLineSpacing;
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
        return this.Font.MeasureString(text) * this.Scale;
    }

    private Vector2 MeasureText(StringBuilder text)
    {
        return this.Font.MeasureString(text) * this.Scale;
    }
}
