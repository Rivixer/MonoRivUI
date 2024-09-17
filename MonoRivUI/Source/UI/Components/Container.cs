using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a UI container component.
/// </summary>
public class Container : Component, IButtonContent<Container>, IStyleable<Container>
{
    /// <inheritdoc/>
    /// <remarks>The container component is always hovered.</remarks>
    public bool IsButtonContentHovered(Point mousePosition)
    {
        return true;
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<Container> style)
    {
        style.Apply(this);
    }
}
