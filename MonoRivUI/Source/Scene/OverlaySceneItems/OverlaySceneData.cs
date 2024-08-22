namespace MonoRivUI;

/// <summary>
/// Represents a data structure for displaying an overlay scene.
/// </summary>
public class OverlaySceneData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverlaySceneData"/> class.
    /// </summary>
    /// <param name="scene">
    /// The overlay scene.
    /// </param>
    /// <param name="options">
    /// The options for showing the overlay scene.
    /// </param>
    public OverlaySceneData(IOverlayScene scene, OverlaySceneShowOptions options)
    {
        this.Scene = scene;
        this.Options = options;
    }

    /// <summary>
    /// Gets the overlay scene.
    /// </summary>
    public IOverlayScene Scene { get; }

    /// <summary>
    /// Gets the options for showing the overlay scene.
    /// </summary>
    public OverlaySceneShowOptions Options { get; }
}