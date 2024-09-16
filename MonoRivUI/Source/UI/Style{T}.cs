using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonoRivUI;

/// <summary>
/// Represents a style of a UI component.
/// </summary>
/// <typeparam name="T">The type of the component to which the style is applied.</typeparam>
public class Style<T> : Style
    where T : IComponent, IStyleable<T>
{
    private readonly Dictionary<string, object?> customProperties = new();

    /// <summary>
    /// Gets or sets the action to be performed when the style is applied.
    /// </summary>
    /// <remarks>
    /// Useful for creating child components.
    /// </remarks>
    public Action<T>? Action { get; set; }

    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    /// <param name="key">The key of the property.
    /// </param>
    /// <returns>The property of the style.</returns>
    public object? this[string key]
    {
        get => this.customProperties.First(v => v.Key == key).Value;
        set => this.customProperties[key] = value;
    }

    /// <summary>
    /// Gets a property of the style.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to get.</typeparam>
    /// <param name="key">The key of the property.</param>
    /// <returns>The property of the style.</returns>
    public virtual TProp? GetProperty<TProp>(string key)
    {
        return (TProp?)this.customProperties[key];
    }

    /// <summary>
    /// Gets a property of the style.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to get.</typeparam>
    /// <returns>The property of the style.</returns>
    /// <remarks>
    /// If there are multiple properties of the same type, the first one is returned.
    /// </remarks>
    public virtual TProp GetPropertyOfType<TProp>()
        where TProp : notnull
    {
        return this.customProperties.Values.OfType<TProp>().First();
    }

    /// <summary>
    /// Gets all properties of the style.
    /// </summary>
    /// <typeparam name="TProp">The type of the properties to get.</typeparam>
    /// <returns>The properties of the style.</returns>
    public virtual IEnumerable<TProp> GetAllPropertiesOfType<TProp>()
        where TProp : notnull
    {
        return this.customProperties.Values.OfType<TProp>();
    }

    /// <summary>
    /// Applies the style to a component.
    /// </summary>
    /// <param name="component">The component to apply the style to.</param>
    public virtual void Apply(T component)
    {
        PropertyInfo[] properties = this.GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            string propertyName = property.Name;

            if (Attribute.IsDefined(property, typeof(Name)))
            {
                propertyName = ((Name)Attribute.GetCustomAttribute(property, typeof(Name))!).Value;
            }

            PropertyInfo? componentProperty = component.GetType().GetProperty(propertyName);
            if (componentProperty is { } p)
            {
                if (Attribute.IsDefined(p, typeof(Stylable)))
                {
                    p.SetValue(component, property.GetValue(this));
                }

                continue;
            }

            EventInfo? componentEvent = component.GetType().GetEvent(propertyName);
            if (componentEvent != null && Attribute.IsDefined(componentEvent, typeof(Stylable)))
            {
                _ = componentEvent.AddMethod?.Invoke(component, new object?[] { property.GetValue(this) });
            }
        }

        foreach (var property in this.customProperties)
        {
            PropertyInfo? componentProperty = component.GetType().GetProperty(property.Key);
            if (componentProperty is { } p && Attribute.IsDefined(p, typeof(Stylable)))
            {
                p.SetValue(component, property.Value);
            }
        }

        this.Action?.Invoke(component);
    }
}
