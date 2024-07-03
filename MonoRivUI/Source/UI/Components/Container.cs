using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a UI container component.
/// </summary>
public class Container : Component, IButtonContent<Container>
{
    /// <inheritdoc/>
    /// <remarks>The container component is always hovered.</remarks>
    public bool IsButtonContentHovered(Point mousePosition)
    {
        return true;
    }
}
