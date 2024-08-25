using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a UI component that can be used as a button content.
/// </summary>
/// <typeparam name="T">
/// The type of the component that can be used as a button content.
/// </typeparam>
public interface IButtonContent<out T>
    where T : IComponent
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
    bool IsButtonContentHovered(Point mousePosition);
}
