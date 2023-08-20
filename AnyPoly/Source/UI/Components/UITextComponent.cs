using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnyPoly.UI;

/// <summary>
/// Represents a base class for UI components that display text.
/// </summary>
internal abstract class UITextComponent : UIComponent
{
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
    public virtual string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the displayed text.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets or sets the scaling factor of the text.
    /// </summary>
    public virtual float Scale { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the alignment of the text.
    /// </summary>
    public virtual Alignment TextAlignment { get; set; } = Alignment.TopLeft;

    /// <summary>
    /// Gets or sets the font used to display the text.
    /// </summary>
    public SpriteFont Font { get; protected set; }

    /// <summary>
    /// Returns the scaled dimensions of the text.
    /// </summary>
    /// <returns>
    /// The scaled dimensions of the text,
    /// corresponds to the size of the text.
    /// </returns>
    public abstract Vector2 GetScaledDimensions();
}
