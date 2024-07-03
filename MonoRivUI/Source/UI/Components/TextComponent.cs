using System.Globalization;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components that display text.
/// </summary>
public abstract class TextComponent : Component
{
    private string rawText = string.Empty;
    private string text = string.Empty;
    private TextCase textCase = TextCase.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextComponent"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the text.</param>
    protected TextComponent(ScalableFont font, Color color)
    {
        this.Font = font;
        this.Color = color;
    }

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public virtual string Value
    {
        get => this.text;
        set
        {
            this.rawText = this.text = value;
            this.ApplyTextCase();
        }
    }

    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    public ScalableFont Font { get; set; }

    /// <summary>
    /// Gets or sets the color of the displayed text.
    /// </summary>
    public virtual Color Color { get; set; }

    /// <summary>
    /// Gets or sets the scaling factor of the text.
    /// </summary>
    public virtual float Scale { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the alignment of the text.
    /// </summary>
    public virtual Alignment TextAlignment { get; set; } = Alignment.TopLeft;

    /// <summary>
    /// Gets or sets the size adjustment behavior.
    /// </summary>
    /// <remarks>
    /// The size adjustment behavior specifies how the transform size
    /// is adjusted to fit the text content.
    /// </remarks>
    public virtual AdjustSizeOption AdjustTransformSizeToText { get; set; }

    /// <summary>
    /// Gets the dimensions of the text.
    /// </summary>
    /// <remarks>
    /// The dimensions represent the minimum size
    /// of the rectangle that can contain the text.
    /// </remarks>
    public abstract Vector2 Dimensions { get; }

    /// <summary>
    /// Gets or sets the case of the text.
    /// </summary>
    public TextCase Case
    {
        get => this.textCase;
        set
        {
            if (this.textCase == value)
            {
                return;
            }

            this.textCase = value;
            this.ApplyTextCase();
        }
    }

    /// <summary>
    /// Adjusts the size of the transform to fit the text content.
    /// </summary>
    /// <param name="dimensions">The dimensions of the text content.</param>
    protected virtual void AdjustSizeToText(Vector2 dimensions)
    {
        switch (this.AdjustTransformSizeToText)
        {
            case AdjustSizeOption.OnlyHeight:
                this.Transform.SetRelativeSizeFromAbsolute(
                                     y: dimensions.Y);
                break;
            case AdjustSizeOption.OnlyWidth:
                this.Transform.SetRelativeSizeFromAbsolute(
                    x: dimensions.X);
                break;
            case AdjustSizeOption.HeightAndWidth:
                this.Transform.SetRelativeSizeFromAbsolute(
                    x: dimensions.X, y: dimensions.Y);
                break;
        }
    }

    /// <summary>
    /// Applies the text case to the text.
    /// </summary>
    protected void ApplyTextCase()
    {
        switch (this.Case)
        {
            case TextCase.None:
                this.text = this.rawText;
                break;
            case TextCase.Upper:
                this.text = this.text.ToUpper();
                break;
            case TextCase.Lower:
                this.text = this.text.ToLower();
                break;
            case TextCase.Title:
                this.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.text.ToLower());
                break;
        }
    }
}
