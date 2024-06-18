using System;

namespace MonoRivUI;

/// <summary>
/// Represents event data for changes in a transform element.
/// </summary>
/// <typeparam name="T">The type of the transform element.</typeparam>
public class TransformElementChangedEventArgs<T> : EventArgs
    where T : struct
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="TransformElementChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="before">The value before the change.</param>
    /// <param name="after">The value after the change,</param>
    public TransformElementChangedEventArgs(T before, T after)
    {
        this.Before = before;
        this.After = after;
    }

    /// <summary>
    /// Gets the value before the change.
    /// </summary>
    public T Before { get; }

    /// <summary>
    /// Gets the value after the change.
    /// </summary>
    public T After { get; }
}
