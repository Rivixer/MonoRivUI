using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components.
/// </summary>
public abstract partial class Component : IComponentHierarchy, IReadOnlyComponent
{
    private static readonly Queue<Component> PriorityComponents = new();
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

    /// <summary>
    /// An event raised when a child component has been removed.
    /// </summary>
    public event EventHandler<ChildChangedEventArgs>? ChildCloned;

    /// <inheritdoc/>
    IReadOnlyTransform IReadOnlyComponent.Transform => this.Transform;

    /// <inheritdoc/>
    IReadOnlyComponent? IReadOnlyComponent.Parent => this.parent;

    /// <inheritdoc/>
    IEnumerable<IReadOnlyComponent> IReadOnlyComponent.Children => this.Children;

    /// <inheritdoc/>
    IReadOnlyComponent IReadOnlyComponent.Root => this.Root;

    /// <inheritdoc/>
    IComponentHierarchy? IComponentHierarchy.Parent => this.Parent;

    /// <inheritdoc/>
    IEnumerable<IComponentHierarchy> IComponentHierarchy.Children => this.Children;

    /// <inheritdoc/>
    IComponentHierarchy IComponentHierarchy.Root => this.Root;

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
    public IReadOnlyComponent? Parent
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
    /// Gets the root component of the hierarchy.
    /// </summary>
    public Component Root => this.parent?.Root ?? this;

    /// <summary>
    /// Gets or sets a value indicating whether the component is enabled.
    /// </summary>
    /// <remarks>
    /// <inheritdoc/>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the component is priority.
    /// </summary>
    /// <inheritdoc/>
    public bool IsPriority { get; set; }

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
    public uint Id { get; private set; } = idCounter++;

    public static bool operator ==(Component? a, Component? b)
    {
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(Component? a, Component? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Draws the priority components.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    public static void DrawPriorityComponents(GameTime gameTime)
    {
        while (PriorityComponents.Count > 0)
        {
            Component component = PriorityComponents.Peek();
            component.Draw(gameTime);
            _ = PriorityComponents.Dequeue();
        }
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

        foreach (Component child in this.children.ToList())
        {
            if (!child.AutoUpdate)
            {
                continue;
            }

            child.Update(gameTime);
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

        if (this.IsPriority && !PriorityComponents.Contains(this))
        {
            PriorityComponents.Enqueue(this);
            return;
        }

        foreach (Component child in this.children.ToList())
        {
            if (!child.AutoDraw)
            {
                continue;
            }

            child.Draw(gameTime);
        }
    }

    /// <inheritdoc/>
    public T? GetChild<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent
    {
        return this.Children.OfType<T>().FirstOrDefault(c => predicate?.Invoke(c) ?? true);
    }

    /// <inheritdoc/>
    public T? GetDescendant<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent
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

    /// <inheritdoc/>
    public IEnumerable<T> GetAllChildren<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent
    {
        return this.children.OfType<T>().Where(c => predicate?.Invoke(c) ?? true);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetAllDescendants<T>(Predicate<T>? predicate = null)
        where T : IReadOnlyComponent
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
