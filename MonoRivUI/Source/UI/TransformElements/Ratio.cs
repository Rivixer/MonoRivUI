using System;

namespace MonoRivUI;

/// <summary>
/// Represents a ratio between two integers.
/// </summary>
public readonly struct Ratio : IEquatable<Ratio>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ratio"/> struct.
    /// </summary>
    /// <param name="numerator">The numerator of the ratio.</param>
    /// <param name="denominator">The denominator of the ratio.</param>
    public Ratio(int numerator, int denominator)
    {
        if (numerator == 0 && denominator == 0)
        {
            this.Numerator = 0;
            this.Denominator = 0;
            return;
        }

        int gcd = MathUtils.GreatestCommonDivisor(numerator, denominator);
        this.Numerator = numerator / gcd;
        this.Denominator = denominator / gcd;
    }

    /// <summary>
    /// Gets an unspecified ratio.
    /// </summary>
    /// <value>
    /// The unspecified ratio that is equal to 0/0.
    /// </value>
    public static Ratio Unspecified => new(0, 0);

    /// <summary>
    /// Gets the numerator of the ratio.
    /// </summary>
    public int Numerator { get; }

    /// <summary>
    /// Gets the denominator of the ratio.
    /// </summary>
    public int Denominator { get; }

    public static bool operator ==(Ratio a, Ratio b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Ratio a, Ratio b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Determines whether the current ratio is equal to another object.
    /// </summary>
    /// <param name="obj">The object to be compared.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal
    /// to the current ratio; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Ratio ratio
            && this.Equals(ratio);
    }

    /// <summary>
    /// Returns the hash code for the current ratio.
    /// </summary>
    /// <returns>The hash code for the current ratio.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Numerator, this.Denominator);
    }

    /// <summary>
    /// Determines whether the current ratio is equal to another ratio.
    /// </summary>
    /// <param name="other">The other ratio to be compared.</param>
    /// <returns>
    /// <see langword="true"/> if the specified ratio is equal
    /// to the current ratio; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Ratio other)
    {
        return this.Numerator.Equals(other.Numerator)
            && this.Denominator.Equals(other.Denominator);
    }

    /// <summary>
    /// Converts the current ratio to a floating-point value.
    /// </summary>
    /// <returns>The converted floating-point value.</returns>
    public float ToFloat()
    {
        return this.Numerator / (float)this.Denominator;
    }

    /// <summary>
    /// Converts the current ratio to a string.
    /// </summary>
    /// <returns>
    /// The string representation of the current ratio
    /// in the format "Numerator:Denominator".
    /// </returns>
    public override string ToString()
    {
        return $"{this.Numerator}:{this.Denominator}";
    }
}
