using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a UI component that can be used as a button content.
/// </summary>
/// <typeparam name="T">
/// The type of the component that can be used as a button content.
/// </typeparam>
internal interface IUIButtonContent<out T>
    where T : UIComponent
{
    /// <summary>
    /// Determines whether the button content is currently
    /// being hovered by the mouse cursor.
    /// </summary>
    /// <param name="mousePosition">The position of the mouse cursor.</param>
    /// <returns>
    /// <see langword="true"/> if the button content is hovered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is used in addition to checking if the cursor is within
    /// the button's <see cref="UITransform.ScaledRectangle"/>.
    /// Both conditions must be met for the button content to be considered hovered.
    /// </remarks>
    bool IsButtonContentHovered(Point mousePosition);
}
