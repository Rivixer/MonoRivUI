namespace MonoRivUI;

/// <summary>
/// Represents an overlay component.
/// </summary>
public interface IOverlayComponent : IOverlay, IComponent
{
    /// <summary>
    /// Called when the overlay component is shown.
    /// </summary>
    void OnShow();

    /// <summary>
    /// Called when the overlay component is hidden.
    /// </summary>
    void OnHide();
}
