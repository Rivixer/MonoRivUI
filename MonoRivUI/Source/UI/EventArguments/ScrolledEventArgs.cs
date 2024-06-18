using System;

namespace MonoRivUI;

/// <summary>
/// Represents event data for a scroll event.
/// </summary>
public class ScrolledEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrolledEventArgs"/> class.
    /// </summary>
    /// <param name="current">The current position of the scrolled content.</param>
    /// <param name="total">The total range of the scrollable content.</param>
    public ScrolledEventArgs(float current, float total)
    {
        this.Current = current;
        this.Total = total;
    }

    /// <summary>
    /// Gets the current position of the scrolled content.
    /// </summary>
    public float Current { get; }

    /// <summary>
    /// Gets the total range of the scrollable content.
    /// </summary>
    public float Total { get; }
}
