namespace MonoRivUI;

/// <summary>
/// Interface for a styleable component.
/// </summary>
/// <typeparam name="T">The type of the component that can be styled.</typeparam>
public interface IStyleable<T>
    where T : IComponent, IStyleable<T>
{
    /// <summary>
    /// Applies a style to the component.
    /// </summary>
    /// <param name="style">The style to be applied to the component.</param>
    void ApplyStyle(Style<T> style);
}
