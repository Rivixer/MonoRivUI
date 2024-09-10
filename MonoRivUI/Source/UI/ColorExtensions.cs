using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Provides extension methods for the <see cref="Color"/> struct.
/// </summary>
internal static class ColorExtensions
{
    /// <summary>
    /// Returns a new <see cref="Color"/> with the specified alpha value.
    /// </summary>
    /// <param name="color">The color to change the alpha value of.</param>
    /// <param name="alpha">The new alpha value of the color.</param>
    /// <returns>
    /// A new <see cref="Color"/> with the specified alpha value.
    /// </returns>
    public static Color WithAlpha(this Color color, byte alpha)
    {
        color.A = alpha;
        return color;
    }
}
