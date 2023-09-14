using System;

namespace AnyPoly.UI;

/// <summary>
/// Represents event data for a text input event.
/// </summary>
internal class TextInputEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputEventArgs"/> class.
    /// </summary>
    /// <param name="text">The input text.</param>
    public TextInputEventArgs(string text)
    {
        this.Text = text;
    }

    /// <summary>
    /// Gets the input text.
    /// </summary>
    public string Text { get; }
}
