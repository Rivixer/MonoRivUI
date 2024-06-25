using System;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components that display text.
/// </summary>
public abstract class TextComponent : Component
{
    private string text = string.Empty;

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
        set => this.text = value.Replace("\t", "    ", StringComparison.Ordinal);
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
    /// Gets or sets a value indicating whether the transform size
    /// should dynamically adapt to the text content.
    /// </summary>
    /// <remarks>
    /// If this is set to <see langword="true"/>,
    /// <see cref="Transform.Size"/> will be adjusted
    /// to match the size of the text content.<br/>
    /// If this is set to <see langword="false"/>, the sizes
    /// will remain unchanged, regardless of the text size.
    /// </remarks>
    public virtual bool AdjustSizeToText { get; set; }

    /// <summary>
    /// Gets the dimensions of the text.
    /// </summary>
    /// <remarks>
    /// The dimensions represent the minimum size
    /// of the rectangle that can contain the text.
    /// </remarks>
    public abstract Vector2 Dimensions { get; }
}
