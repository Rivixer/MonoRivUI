using System;

namespace MonoRivUI;

/// <summary>
/// Specifies different text scaling modes for fitting within its rectangle.
/// </summary>
[Flags]
public enum TextFit
{
    /// <summary>
    /// No scaling applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Scale to fit the rectangle's height.
    /// </summary>
    Height = 1,

    /// <summary>
    /// Scale to fit the rectangle's width.
    /// </summary>
    Width = 2,

    /// <summary>
    /// Scale to fit both the rectangle's width and height.
    /// </summary>
    Both = Height | Width,
}
