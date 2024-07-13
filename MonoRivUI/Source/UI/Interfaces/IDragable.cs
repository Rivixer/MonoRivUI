namespace MonoRivUI;

/// <summary>
/// Represents a dragable component.
/// </summary>
public interface IDragable
{
    /// <summary>
    /// Gets a value indicating whether the component is currently being dragged.
    /// </summary>
    bool IsDragging { get; }

    /// <summary>
    /// Gets a value indicating whether the component was being dragged.
    /// </summary>
    bool WasDragging { get; }

    /// <summary>
    /// Updates the drag state of the component.
    /// </summary>
    void UpdateDragState();
}
