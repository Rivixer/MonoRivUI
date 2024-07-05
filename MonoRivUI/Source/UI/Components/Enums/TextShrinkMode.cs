using System;

namespace MonoRivUI;

/// <summary>
/// Represents a mode for shrinking text if it
/// goes outside the bounds of the target.
/// </summary>
[Flags]
public enum TextShrinkMode
{
    /// <summary>
    /// No shrinking.
    /// </summary>
    None = 0,

    /// <summary>
    /// Shrink the text to fit the height.
    /// </summary>
    Height = 1,

    /// <summary>
    /// Shrink the text to fit the width.
    /// </summary>
    Width = 2,

    /// <summary>
    /// Shrink the text to fit both the height and width.
    /// </summary>
    HeightAndWidth = Height | Width,
}
