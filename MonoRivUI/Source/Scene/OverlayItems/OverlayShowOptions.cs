namespace MonoRivUI;

/// <summary>
/// Represents options for displaying an overlay scene.
/// </summary>
/// <param name="BlockFocusOnUnderlyingScenes">
/// Indicates whether to prevent underlying scenes from receiving focus.
/// </param>
/// <param name="BlockUpdateOnUnderlyingScenes">
/// Indicates whether to prevent underlying scenes from being updated.
/// </param>
/// <param name="BlockDrawOnUnderlyingScenes">
/// Indicates whether to prevent underlying scenes from being drawn.
/// </param>
public record struct OverlayShowOptions(
    bool BlockFocusOnUnderlyingScenes = false,
    bool BlockUpdateOnUnderlyingScenes = false,
    bool BlockDrawOnUnderlyingScenes = false);
