using System;
using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Provides extension methods for transformation objects.
/// </summary>
internal static class TransformExtensions
{
    /// <summary>
    /// Scales a point by a vector.
    /// </summary>
    /// <param name="point">The point to be scaled.</param>
    /// <param name="vector">The scaling vector.</param>
    /// <returns>The scaled point.</returns>
    /// <remarks>The point components are converted to integers after scaling.</remarks>
    public static Point Scale(this Point point, Vector2 vector)
    {
        return new Point((int)(point.X * vector.X), (int)(point.Y * vector.Y));
    }

    /// <summary>
    /// Scales a point by a scalar.
    /// </summary>
    /// <param name="point">The point to be scaled.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The scaled point.</returns>
    /// <remarks>The point components are converted to integers after scaling.</remarks>
    public static Point Scale(this Point point, float scalar)
    {
        return new Point((int)(point.X * scalar), (int)(point.Y * scalar));
    }

    /// <summary>
    /// Unscales a point by a vector.
    /// </summary>
    /// <param name="point">The point to be unscaled.</param>
    /// <param name="vector">The original vector used for scaling.</param>
    /// <returns>The unscaled point.</returns>
    /// <remarks>The point components are rounded up to integers after unscaling.</remarks>
    public static Point Unscale(this Point point, Vector2 vector)
    {
        return new Point(
            (int)Math.Ceiling(point.X / vector.X),
            (int)Math.Ceiling(point.Y / vector.Y));
    }

    /// <summary>
    /// Unscales a point by a scalar.
    /// </summary>
    /// <param name="point">The point to be unscaled.</param>
    /// <param name="scalar">The original scalar value used for scaling.</param>
    /// <returns>The unscaled point.</returns>
    /// <remarks>The point components are rounded up to integers after unscaling.</remarks>
    public static Point Unscale(this Point point, float scalar)
    {
        return new Point(
            (int)Math.Ceiling(point.X / scalar),
            (int)Math.Ceiling(point.Y / scalar));
    }

    /// <summary>
    /// Scales a vector by another vector.
    /// </summary>
    /// <param name="vector">The vector to be scaled.</param>
    /// <param name="scalingVector">The scaling vector.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 Scale(this Vector2 vector, Vector2 scalingVector)
    {
        return new Vector2(vector.X * scalingVector.X, vector.Y * scalingVector.Y);
    }

    /// <summary>
    /// Scales a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector to be scaled.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 Scale(this Vector2 vector, float scalar)
    {
        return vector * scalar;
    }

    /// <summary>
    /// Unscales a vector by another vector.
    /// </summary>
    /// <param name="vector">The vector to be unscaled.</param>
    /// <param name="scalingVector">The original vector used for scaling.</param>
    /// <returns>The unscaled vector.</returns>
    public static Vector2 Unscale(this Vector2 vector, Vector2 scalingVector)
    {
        return new Vector2(vector.X / scalingVector.X, vector.Y / scalingVector.Y);
    }

    /// <summary>
    /// Unscales a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector to be unscaled.</param>
    /// <param name="scalar">The original scalar value used for scaling.</param>
    /// <returns>The unscaled vector.</returns>
    public static Vector2 Unscale(this Vector2 vector, float scalar)
    {
        return vector / scalar;
    }

    /// <summary>
    /// Converts a point to a ratio.
    /// </summary>
    /// <param name="point">The point to be converted.</param>
    /// <returns>The ratio.</returns>
    public static Ratio ToRatio(this Point point)
    {
        return new Ratio(point.X, point.Y);
    }

    /// <summary>
    /// Converts a floating-point value to a ratio using rational approximation.
    /// </summary>
    /// <param name="value">The floating-point value to be converted.</param>
    /// <param name="epsilon">The acceptable difference between the value and the resulting ratio.</param>
    /// <param name="maxDenominator">The maximum denominator for the ratio's fraction.</param>
    /// <returns>The rational approximation of the value as a ratio.</returns>
    /// <exception cref="ArgumentException">Thrown when the value cannot be represented as a ratio.</exception>
    /// <remarks>
    /// This method attempts to find a rational approximation of the given value by iteratively searching
    /// for the closest rational fraction with a denominator within the specified range
    /// that satisfied the given epsilon tolerance.
    /// </remarks>
    public static Ratio ToRatio(this float value, double epsilon = 1.0e-2, int maxDenominator = 1000)
    {
        for (int denominator = 1; denominator <= maxDenominator; denominator++)
        {
            int numerator = (int)Math.Round(value * denominator);
            double approximationError = Math.Abs(value - (numerator / (double)denominator));

            if (approximationError < epsilon)
            {
                return new Ratio(numerator, denominator);
            }
        }

        throw new ArgumentException($"Unable to represent {value} as a ratio.");
    }
}
