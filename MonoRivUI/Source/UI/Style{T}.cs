﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonoRivUI;

/// <summary>
/// Represents a style of a UI component.
/// </summary>
/// <typeparam name="T">The type of the component to which the style is applied.</typeparam>
public class Style<T> : Style
    where T : Component, IStyleable<T>
{
    /// <summary>
    /// Gets or sets the action to be performed when the style is applied.
    /// </summary>
    /// <remarks>
    /// Useful for creating child components.
    /// </remarks>
    public Action<T>? Action { get; set; }

    /// <summary>
    /// Gets or sets the custom properties of the style.
    /// </summary>
    public List<Property> CustomProperties { get; set; } = new();

    /// <summary>
    /// Gets the style of a component.
    /// </summary>
    /// <param name="component">The component to get the style of.</param>
    /// <returns>
    /// The style of the component.
    /// </returns>
    [Obsolete("Event handling is not supported yet.", true)] // TODO
    public static Style<T> Get(T component)
    {
#pragma warning disable CS8604, CS8602
        var style = new Style<T>();

        var properties = component
                .GetType()
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(Stylable)) || Attribute.IsDefined(prop, typeof(SubStylable)))
                .ToList();

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttribute<Stylable>() is not null)
            {
                style.CustomProperties.Add(new Property(prop.Name, prop.GetValue(component)));
            }
            else if (prop.GetCustomAttribute<SubStylable>() is not null)
            {
                Type innerType = prop.GetValue(component).GetType();
                MethodInfo getStyleMethod = typeof(IStyleable<>).MakeGenericType(innerType).GetMethod("GetStyle")!;
                var s = getStyleMethod.Invoke(prop.GetValue(component), null);
                style.CustomProperties.Add(new Property(prop.Name, s));
            }
        }

        return style;
#pragma warning restore CS8604, CS8602
    }

    /// <summary>
    /// Gets a property of the style.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to get.</typeparam>
    /// <param name="key">The key of the property.</param>
    /// <returns>The property of the style.</returns>
    public virtual TProp? GetProperty<TProp>(string key)
    {
        PropertyInfo? property = this.GetType().GetProperty(key);
        return (TProp?)(property?.GetValue(this) ?? this.CustomProperties.FirstOrDefault(v => v.Name == key)?.Value);
    }

    /// <summary>
    /// Gets a property of the style.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to get.</typeparam>
    /// <returns>The property of the style.</returns>
    /// <remarks>
    /// If there are multiple properties of the same type, the first one is returned.
    /// </remarks>
    public virtual TProp? GetPropertyOfType<TProp>()
    {
        PropertyInfo? property = this.GetType().GetProperties().FirstOrDefault(v => v.PropertyType == typeof(TProp));
        return (TProp?)(property?.GetValue(this) ?? this.CustomProperties.FirstOrDefault(v => v.Value is TProp)?.Value);
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
            if (componentProperty is not null)
            {
                if (Attribute.IsDefined(componentProperty, typeof(Stylable)))
                {
                    componentProperty.SetValue(component, property.GetValue(this));
                }
                else if (Attribute.IsDefined(componentProperty, typeof(SubStylable)))
                {
                    object? componentSubPropertyValue = componentProperty.GetValue(component);
                    if (componentSubPropertyValue is not null)
                    {
                        Type innerType = componentSubPropertyValue.GetType();
                        MethodInfo applyStyleMethod = typeof(IStyleable<>).MakeGenericType(innerType).GetMethod("ApplyStyle")!;
                        if (property.GetValue(this) is { } s)
                        {
                            _ = applyStyleMethod.Invoke(componentSubPropertyValue, new object[] { s });
                        }
                    }
                }

                continue;
            }

            EventInfo? componentEvent = component.GetType().GetEvent(propertyName);
            if (componentEvent != null && Attribute.IsDefined(componentEvent, typeof(Stylable)))
            {
                _ = componentEvent.AddMethod?.Invoke(component, new object?[] { property.GetValue(this) });
            }
        }

        foreach (Property property in this.CustomProperties)
        {
            string propertyName = property.Name;

            PropertyInfo? componentProperty = component.GetType().GetProperty(propertyName);
            if (componentProperty is not null)
            {
                if (Attribute.IsDefined(componentProperty, typeof(Stylable)))
                {
                    componentProperty.SetValue(component, property.Value);
                }
                else if (Attribute.IsDefined(componentProperty, typeof(SubStylable)))
                {
                    object? componentSubPropertyValue = componentProperty.GetValue(component);
                    if (componentSubPropertyValue is not null)
                    {
                        Type innerType = componentSubPropertyValue.GetType();
                        MethodInfo applyStyleMethod = typeof(IStyleable<>).MakeGenericType(innerType).GetMethod("ApplyStyle")!;
                        _ = applyStyleMethod.Invoke(componentSubPropertyValue, new object[] { property.Value });
                    }
                }
            }
        }

        this.Action?.Invoke(component);
    }
}
