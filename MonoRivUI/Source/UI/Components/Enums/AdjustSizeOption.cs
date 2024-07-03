using System;

namespace MonoRivUI;

/// <summary>
/// Represents the options for adjusting the size of an element.
/// </summary>
[Flags]
public enum AdjustSizeOption
{
    /// <summary>
    /// No adjusting.
    /// </summary>
    None = 0,

    /// <summary>
    /// Adjust the size of the element to fit the width of the target.
    /// </summary>
    OnlyWidth = 1,

    /// <summary>
    /// Adjust the size of the element to fit the height of the target.
    /// </summary>
    OnlyHeight = 2,

    /// <summary>
    /// Adjust the size of the element to fit both the height and width of the target.
    /// </summary>
    HeightAndWidth = OnlyWidth | OnlyHeight,
}
