namespace AnyPoly.UI;

/// <summary>
/// Represents event data for a hover state change event.
/// </summary>
/// <typeparam name="T">
/// The type of the component that changed its hover state.
/// </typeparam>
internal class HoverStateChangedEventArgs<T>
    where T : UIComponent
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="HoverStateChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="component">
    /// The component that changed its hover state.
    /// </param>
    public HoverStateChangedEventArgs(T component)
    {
        this.Component = component;
    }

    /// <summary>
    /// Gets the component that changed its hover state.
    /// </summary>
    public T Component { get; }
}
