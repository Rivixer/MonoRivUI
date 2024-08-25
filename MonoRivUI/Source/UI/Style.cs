using System;

namespace MonoRivUI;

/// <summary>
/// Represents a component style.
/// </summary>
public class Style
{
    /// <summary>
    /// Represents a style property.
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public Property(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object Value { get; }
    }

    /// <summary>
    /// Represents a stylable property or event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Event)]
    public class Stylable : Attribute
    {
    }

    /// <summary>
    /// Represents a name attribute.
    /// </summary>
    /// <remarks>
    /// This attribute is used to specify the name of a property or event,
    /// if it is different from the property.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    protected class Name : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Name"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public Name(string name)
        {
            this.Value = name;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Value { get; }
    }
}
