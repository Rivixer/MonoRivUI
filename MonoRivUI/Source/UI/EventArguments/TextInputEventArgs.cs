using System;

namespace MonoRivUI;

/// <summary>
/// Represents event data for a text input event.
/// </summary>
internal class TextInputEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputEventArgs"/> class.
    /// </summary>
    /// <param name="value">The input text value.</param>
    public TextInputEventArgs(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the input text value.
    /// </summary>
    public string Value { get; }
}
