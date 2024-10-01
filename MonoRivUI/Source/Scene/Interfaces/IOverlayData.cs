namespace MonoRivUI;

/// <summary>
/// Represents an overlay data.
/// </summary>
/// <typeparam name="T">The type of overlay.</typeparam>
public interface IOverlayData<out T>
    where T : IOverlay
{
    /// <summary>
    /// Gets the overlay.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the options for showing the overlay.
    /// </summary>
    public OverlayShowOptions Options { get; }
}
