namespace MonoRivUI;

/// <summary>
/// Specifies the type of transformation.
/// </summary>
public enum TransformType
{
    /// <summary>
    /// Represents a relative transformation.
    /// </summary>
    /// <remarks>
    /// The transformation is relative
    /// to the parent component's state.
    /// </remarks>
    Relative,

    /// <summary>
    /// Represents an absolute transformation.
    /// </summary>
    /// <remarks>
    /// The transformation is based on
    /// <see cref="ScreenController.DefaultSize"/>.
    /// </remarks>
    Absolute,
}
