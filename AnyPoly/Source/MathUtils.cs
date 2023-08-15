namespace AnyPoly;

/// <summary>
/// A utility class that provides mathematical methods.
/// </summary>
internal static class MathUtils
{
    /// <summary>
    /// Calculates the greatest common divisor (GCD) of two integers.
    /// </summary>
    /// <param name="x">The first integer.</param>
    /// <param name="y">The second integer.</param>
    /// <returns>The greatest common divisor of the two input integers.</returns>
    /// <remarks>
    /// This method calculates the GCD using the recursive Euclidean algorithm.
    /// </remarks>
    public static int GreatestCommonDivisor(int x, int y)
    {
        return y == 0 ? x : GreatestCommonDivisor(y, x % y);
    }
}
