using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// A utility class that provides methods for recalculating UI components.
/// </summary>
internal static class RecalculationUtils
{
    /// <summary>
    /// Aligns a rectangle's position within a source rectangle based on the specified alignment.
    /// </summary>
    /// <param name="source">The source rectangle containing the alignment reference.</param>
    /// <param name="target">The target rectangle to be aligned.</param>
    /// <param name="alignment">The desired alignment mode.</param>
    /// <returns>The aligned rectangle.</returns>
    /// <remarks>
    /// This method modified the X and Y position of the target rectangle
    /// to align it within the source rectangle according to the specified alignment mode.
    /// </remarks>
    public static Rectangle AlignRectangle(Rectangle source, Rectangle target, Alignment alignment)
    {
        int x = target.X;
        int y = target.Y;

        switch (alignment)
        {
            case Alignment.TopLeft:
                break;
            case Alignment.Top:
                x += (source.Width - target.Width) / 2;
                break;
            case Alignment.TopRight:
                x += source.Width - target.Width;
                break;
            case Alignment.Left:
                y += (source.Height - target.Height) / 2;
                break;
            case Alignment.Center:
                x += (source.Width - target.Width) / 2;
                y += (source.Height - target.Height) / 2;
                break;
            case Alignment.Right:
                x += source.Width - target.Width;
                y += (source.Height - target.Height) / 2;
                break;
            case Alignment.BottomLeft:
                y += source.Height - target.Height;
                break;
            case Alignment.Bottom:
                x += (source.Width - target.Width) / 2;
                y += source.Height - target.Height;
                break;
            case Alignment.BottomRight:
                x += source.Width - target.Width;
                y += source.Height - target.Height;
                break;
        }

        return new Rectangle(x, y, target.Width, target.Height);
    }
}
