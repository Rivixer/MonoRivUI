namespace MonoRivUI;

/// <summary>
/// Interface for a styleable component.
/// </summary>
/// <typeparam name="T">The type of the component that can be styled.</typeparam>
public interface IStyleable<T>
    where T : Component, IStyleable<T>
{
    /// <summary>
    /// Applies a style to the component.
    /// </summary>
    /// <typeparam name="T_C">The type of the component that can be styled.</typeparam>
    /// <param name="style">The style to be applied to the component.</param>
    /// <returns>The component with the applied style for chaining.</returns>
    T ApplyStyle(Style<T> style);

    /// <summary>
    /// Gets the style of the component.
    /// </summary>
    /// <typeparam name="T_C">The type of the component that can be styled.</typeparam>
    /// <returns>The style of the component.</returns>
    Style<T> GetStyle();
}
