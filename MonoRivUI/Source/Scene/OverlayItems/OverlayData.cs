namespace MonoRivUI;

/// <summary>
/// Represents a data structure for displaying an overlay.
/// </summary>
/// <typeparam name="T">The type of overlay.</typeparam>
public class OverlayData<T> : IOverlayData<T>
    where T : IOverlay
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayData{T}"/> class.
    /// </summary>
    /// <param name="overlay">
    /// The overlay scene.
    /// </param>
    /// <param name="options">
    /// The options for showing the overlay.
    /// </param>
    public OverlayData(T overlay, OverlayShowOptions options)
    {
        this.Value = overlay;
        this.Options = options;
    }

    /// <summary>
    /// Gets the overlay scene.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the options for showing the overlay scene.
    /// </summary>
    public OverlayShowOptions Options { get; }
}
