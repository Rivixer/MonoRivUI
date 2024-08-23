using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a flexible list box, which can contain multiple components.
/// </summary>
/// <remarks>
/// <para>
/// A flexible list box resizes its components based on the resize factors
/// that are set for each component.
/// </para>
/// <para>
/// The resize factors are used to determine
/// how much space each component should take up in the list box.
/// For example, if a component has a resize factor of 2, it will take up
/// twice as much space as a component with a resize factor of the default value 1.
/// </para>
/// </remarks>
public class FlexListBox : ListBox
{
    private readonly Dictionary<IReadOnlyComponent, float> resizeFactors = new();

    /// <summary>
    /// Sets the resize factor for the specified component.
    /// </summary>
    /// <param name="component">The component to set the resize factor for.</param>
    /// <param name="factor">The resize factor to set for the component.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the component is not in the list box.
    /// </exception>
    public void SetResizeFactor(IReadOnlyComponent component, float factor)
    {
        if (!this.Components.Contains(component))
        {
            throw new InvalidOperationException("The component is not in the list box.");
        }

        this.resizeFactors[component] = factor;
    }

    /// <inheritdoc/>
    protected override void RecalculateContentElements(int currentOffset = 0)
    {
        this.ResizeElements();
        base.RecalculateContentElements(currentOffset);
    }

    private void ResizeElements()
    {
        int spacingLength = this.Spacing * (this.Components.Count() - 1);
        float weightCount = 0;
        foreach (Component component in this.Components)
        {
            if (this.resizeFactors.TryGetValue(component, out float factor))
            {
                weightCount += factor;
            }
            else
            {
                weightCount++;
            }
        }

        float resizeFactor = (1f - (spacingLength / this.ContentContainerLength)) / weightCount;
        foreach (Component component in this.Components)
        {
            var componentFactor = this.resizeFactors.TryGetValue(component, out float factor)
                               ? factor
                            : 1f;
            component.Transform.RelativeSize = this.Orientation switch
            {
                Orientation.Vertical => new Vector2(1, resizeFactor * componentFactor),
                Orientation.Horizontal => new Vector2(resizeFactor * componentFactor, 1),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
