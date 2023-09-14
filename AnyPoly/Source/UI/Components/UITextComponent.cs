using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents a base class for UI components that display text.
/// </summary>
internal abstract class UITextComponent : UIComponent
{
    private string text = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="UITextComponent"/> class.
    /// </summary>
    /// <param name="color">The color of the text.</param>
    protected UITextComponent(Color color)
    {
        this.Color = color;

        // TODO: Change default font
        this.Font = ContentController.Content.Load<SpriteFont>("Fonts/DebugFont");
    }

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public virtual string Text
    {
        get => this.text;
        set => this.text = value.Replace("\t", "    ", StringComparison.Ordinal);
    }

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
    /// <see cref="UITransform.UnscaledSize"/> and
    /// <see cref="UITransform.ScaledSize"/> will be adjusted
    /// to match the size of the text content.<br/>
    /// If this is set to <see langword="false"/>, the sizes
    /// will remain unchanged, regardless of the text size.
    /// </remarks>
    public virtual bool AdjustSizeToText { get; set; }

    /// <summary>
    /// Gets or sets the font used to display the text.
    /// </summary>
    public SpriteFont Font { get; protected set; }

    /// <summary>
    /// Gets the unscaled dimensions of the text.
    /// </summary>
    /// <remarks>
    /// The unscaled dimensions represent the minimum
    /// size of the rectangle that can contain
    /// the unscaled text.
    /// </remarks>
    public abstract Vector2 UnscaledDimensions { get; }

    /// <summary>
    /// Gets the scaled dimensions of the text.
    /// </summary>
    /// <remarks>
    /// The scaled dimensions represent the minimum
    /// size of the rectangle that can contain
    /// the scaled text.
    /// </remarks>
    public abstract Vector2 ScaledDimensions { get; }
}
