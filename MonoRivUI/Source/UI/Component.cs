using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components.
/// </summary>
public abstract partial class Component : IUIComponentHierarchy, IUIReadOnlyComponent
{
    private static uint idCounter;
    private readonly List<Component> children = new();
    private Component? parent;
    private Transform? transform;

    /// <summary>
    /// An event raised when a parent of the component has been changed.
    /// </summary>
    public event EventHandler<ParentChangedEventArgs>? ParentChanged;

    /// <summary>
    /// An event raised when a child component has been added.
    /// </summary>
    public event EventHandler<ChildChangedEventArgs>? ChildAdded;

    /// <summary>
    /// An event raised when a child component has been removed.
    /// </summary>
    public event EventHandler<ChildChangedEventArgs>? ChildRemoved;

    /// <inheritdoc/>
    IUIReadOnlyTransform IUIReadOnlyComponent.Transform => this.Transform;

    /// <inheritdoc/>
    IUIReadOnlyComponent? IUIReadOnlyComponent.Parent => this.parent;

    /// <inheritdoc/>
    IEnumerable<IUIReadOnlyComponent> IUIReadOnlyComponent.Children => this.Children;

    /// <inheritdoc/>
    IUIComponentHierarchy? IUIComponentHierarchy.Parent => this.Parent;

    /// <inheritdoc/>
    IEnumerable<IUIComponentHierarchy> IUIComponentHierarchy.Children => this.Children;

    /// <summary>
    /// Gets or sets the parent component.
    /// </summary>
    /// <remarks>
    /// The parent component is responsible for managing child components.
    /// When setting the parent, several actions are performed:
    /// <list type="bullet">
    /// <item><description>
    /// Removes this component from its current parent's list of children
    /// and raises the <see cref="ChildRemoved"/> event on the parent.
    /// </description></item>
    /// <item><description>
    /// Adds this component to the new parent's list of children
    /// and raises the <see cref="ChildAdded"/> event on the new parent.
    /// </description></item>
    /// <item><description>
    /// Updates the <see cref="Transform.Type"/> of this component
    /// to <see cref="TransformType.Absolute"/> if the new parent is null,
    /// or to <see cref="TransformType.Relative"/> if there is a new parent.
    /// </description></item>
    /// <item><description>
    /// Raises the <see cref="ParentChanged"/> event to notify listeners
    /// about the change in the parent.
    /// </description></item>
    /// </list>
    /// </remarks>
    public IUIReadOnlyComponent? Parent
    {
        get => this.parent;
        set
        {
            if (this.parent == (Component)value!)
            {
                return;
            }

            Component? oldParent = this.parent;

            _ = this.parent?.children.Remove(this);
            this.parent?.ChildRemoved?.Invoke(this.parent, new ChildChangedEventArgs(this));

            this.parent = (Component)value!;

            this.parent?.children.Add(this);
            this.parent?.ChildAdded?.Invoke(this.parent, new ChildChangedEventArgs(this));

            this.Transform.Type = this.parent is null
                ? TransformType.Absolute
                : TransformType.Relative;

            this.ParentChanged?.Invoke(this, new ParentChangedEventArgs(this.parent, oldParent));
        }
    }

    /// <summary>
    /// Gets the transform of the component.
    /// </summary>
    public Transform Transform => this.transform ??= MonoRivUI.Transform.Default(this);

    /// <summary>
    /// Gets an enumerable collection of child components.
    /// </summary>
    public IEnumerable<Component> Children => this.children;

    /// <summary>
    /// Gets or sets a value indicating whether the component is enabled.
    /// </summary>
    /// <remarks>
    /// <para>If component is disabled, it will not be updated or drawn.</para>
    /// <para>The default value is <see langword="true"/>.</para>
    /// </remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the component
    /// automatically updates itself and its child components
    /// during the game loop.
    /// </summary>
    /// <remarks>
    /// If set to <see langword="true"/>, the
    /// <see cref="Update(GameTime)"/> method of this component
    /// and its child components will be invoked during each
    /// game update cycle. If set to <see langword="false"/>,
    /// the update logic must be invoked manually.
    /// The default value is <see langword="true"/>.
    /// </remarks>
    public bool AutoUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the component
    /// automatically draws itself and its child components
    /// during the game loop.
    /// </summary>
    /// <remarks>
    /// If set to <see langword="true"/>, the
    /// <see cref="Draw(GameTime)"/> method of this component
    /// and its child components will be invoked during
    /// each game draw cycle. If set to <see langword="false"/>,
    /// the drawing logic must be invoked manually.
    /// The default value is <see langword="true"/>.
    /// </remarks>
    public bool AutoDraw { get; set; } = true;

    /// <inheritdoc/>
    public uint Id { get; } = idCounter++;

    public static bool operator ==(Component? a, Component? b)
    {
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(Component? a, Component? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Updates the component and its child components.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    public virtual void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.Transform.RecalculateIfNeeded();

        foreach (Component child in this.children)
        {
            if (child.AutoUpdate)
            {
                child.Update(gameTime);
            }
        }
    }

    /// <summary>
    /// Draws the component and its child components.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    public virtual void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        foreach (Component child in this.children)
        {
            if (child.AutoDraw)
            {
                child.Draw(gameTime);
            }
        }
    }

    /// <summary>
    /// Returns the first child of a component
    /// that is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the child to get.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the child components.
    /// </param>
    /// <returns>
    /// The first child of a component that is of type <typeparamref name="T"/>,
    /// or <see langword="null"/> if no child is found.
    /// </returns>
    public T? GetChild<T>(Predicate<T>? predicate = null)
        where T : Component
    {
        return this.children.FirstOrDefault(c => c is T t && (predicate?.Invoke(t) ?? true)) as T;
    }

    /// <summary>
    /// Returns the first descendant of a component
    /// that is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the descendant to get.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the descendant components.</param>
    /// <returns>
    /// The first descendant of a component that is of type <typeparamref name="T"/>,
    /// or <see langword="null"/> if no descendant is found.
    /// </returns>
    public T? GetDescendant<T>(Predicate<T>? predicate = null)
        where T : Component
    {
        T? descendant = this.GetChild(predicate);
        foreach (Component child in this.children)
        {
            if (descendant is not null)
            {
                break;
            }

            descendant = child.GetDescendant(predicate);
        }

        return descendant;
    }

    /// <summary>
    /// Retrieves all children of a component
    /// that are of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the children to retrieve.</typeparam>
    /// <param name="predicate">
    /// An optional predicate used to filter the children components.
    /// </param>
    /// <returns>
    /// An enumerable containing all children of a component
    /// that are of type <typeparamref name="T"/>.
    /// </returns>
    public IEnumerable<T> GetAllChildren<T>(Predicate<T>? predicate = null)
        where T : Component
    {
        return this.children.OfType<T>().Where(c => predicate?.Invoke(c) ?? true);
    }

    /// <summary>
    /// Retrieves all descendants of a component
    /// that are of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the descendants to retrive.</typeparam>
    /// <param name="predicate">
    /// An optional predicate to use to filter the descendants components.
    /// </param>
    /// <returns>
    /// An enumerable containing all descendants of a component
    /// that are of type <typeparamref name="T"/>.
    /// </returns>
    public IEnumerable<T> GetAllDescendants<T>(Predicate<T>? predicate = null)
        where T : Component
    {
        foreach (T child in this.GetAllChildren(predicate))
        {
            yield return child;
        }

        foreach (Component child in this.children)
        {
            foreach (T descendant in child.GetAllDescendants(predicate))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Determines whether the component is equal
    /// to another object by comparing their IDs.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>
    /// <see langword="true"/> if the object are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Component component
            && this.Id.Equals(component.Id);
    }

    /// <summary>
    /// Returns the hash code for the component based on its ID.
    /// </summary>
    /// <returns>The hash code for component.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }

    /// <summary>
    /// Changes the parent of a child component.
    /// </summary>
    /// <param name="child">The component to be reparented.</param>
    /// <param name="newParent">
    /// The new parent component to which the child will be reparented.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the specified <paramref name="child"/>
    /// is not a direct child of this component.
    /// </exception>
    protected virtual void ReparentChild(Component child, Component newParent)
    {
        if (!this.children.Contains(child))
        {
            throw new InvalidOperationException(
                "The specified child is not a direct child of this component.");
        }

        child.Parent = newParent;
    }
}
