using System;

namespace MonoRivUI;

/// <summary>
/// Represents alignment options for UI components.
/// </summary>
[Flags]
public enum Alignment
{
    /// <summary>
    /// Aligns to the top center.
    /// </summary>
    Top = 1,

    /// <summary>
    /// Aligns to the bottom center.
    /// </summary>
    Bottom = 2,

    /// <summary>
    /// Aligns to the left center.
    /// </summary>
    Left = 4,

    /// <summary>
    /// Aligns to the right center.
    /// </summary>
    Right = 8,

    /// <summary>
    /// Aligns to the center.
    /// </summary>
    Center = 16,

    /// <summary>
    /// Aligns to the top-right corner.
    /// </summary>
    TopRight = Top | Right,

    /// <summary>
    /// Aligns to the top-left corner.
    /// </summary>
    TopLeft = Top | Left,

    /// <summary>
    /// Aligns to the bottom-left corner.
    /// </summary>
    BottomLeft = Bottom | Left,

    /// <summary>
    /// Aligns to the bottom-right corner.
    /// </summary>
    BottomRight = Bottom | Right,
}
