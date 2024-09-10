using System;
using System.Linq;

namespace MonoRivUI;

/// <summary>
/// Represents a list box, that allows content elements to be aligned.
/// </summary>
public class AlignedListBox : ListBox
{
    private Alignment elementsAlignment = Alignment.Top;

    /// <inheritdoc/>
    /// <remarks>
    /// If the <see cref="ElementsAlignment"/> is not allowed for the new orientation,
    /// the alignment is set to the first allowed alignment for the new orientation.
    /// </remarks>
    public override Orientation Orientation
    {
        get => base.Orientation;
        set
        {
            if (base.Orientation == value)
            {
                return;
            }

            base.Orientation = value;

            var allowedAlignments = GetAllowedAlignments(value);
            if (!allowedAlignments.Contains(this.elementsAlignment))
            {
                this.ElementsAlignment = allowedAlignments.First();
            }
        }
    }

    /// <summary>
    /// Gets or sets the alignment of the content elements.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified alignment
    /// is not allowed for the current orientation.
    /// </exception>
    public Alignment ElementsAlignment
    {
        get => this.elementsAlignment;
        set
        {
            if (this.elementsAlignment == value)
            {
                return;
            }

            var allowedAlignments = GetAllowedAlignments(this.Orientation);
            if (!allowedAlignments.Contains(value))
            {
                var message = $"The {value} alignment is not allowed for the " +
                    $"{this.Orientation} orientation. " +
                    $"Allowed alignments are: {string.Join(", ", allowedAlignments)}.";
                throw new ArgumentException(message);
            }

            this.elementsAlignment = value;
            this.ContentContainer.Transform.Alignment = value;
            this.IsRecalulcationNeeded = true;
        }
    }

    /// <inheritdoc/>
    protected override void RecalculateContentElements(int currentOffset = 0)
    {
        var componentsLength = this.Components.Sum(this.GetComponentLength);
        componentsLength += this.Spacing * (this.Components.Count() - 1);
        Component? firstComponent = this.Components.FirstOrDefault();
        float firstComponentLength = firstComponent is { } c ? this.GetComponentLength(c) : 0;

        float absoluteOffset = this.Orientation switch
        {
            Orientation.Vertical =>
                this.elementsAlignment switch
                {
                    Alignment.Top => currentOffset,
                    Alignment.Center => (this.Transform.Size.Y - componentsLength - (firstComponentLength / 2f)) / 2f,
                    Alignment.Bottom => this.Transform.Size.Y - componentsLength - (firstComponentLength / 2f),
                    _ => throw new NotSupportedException(),
                },
            Orientation.Horizontal =>
                this.elementsAlignment switch
                {
                    Alignment.Left => 0,
                    Alignment.Center => (this.Transform.Size.Y - componentsLength - (firstComponentLength / 2f)) / 2f,
                    Alignment.Right => this.Transform.Size.Y - componentsLength - (firstComponentLength / 2f),
                    _ => throw new NotSupportedException(),
                },
            _ => throw new NotSupportedException(),
        };

        base.RecalculateContentElements((int)absoluteOffset);
    }

    /// <summary>
    /// Returns the allowed alignments for the specified orientation.
    /// </summary>
    /// <param name="orientation">The orientation to get the allowed alignments for.</param>
    /// <returns>The allowed alignments for the specified orientation.</returns>
    private static Alignment[] GetAllowedAlignments(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Vertical => new[] { Alignment.Top, Alignment.Center, Alignment.Bottom },
            Orientation.Horizontal => new[] { Alignment.Left, Alignment.Center, Alignment.Right },
            _ => throw new NotSupportedException(),
        };
    }
}
