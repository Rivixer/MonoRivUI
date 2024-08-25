using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for UI components.
/// </summary>
public abstract partial class Component : IComponent
{
    private static readonly Queue<Component> PriorityComponents = new();
    private static uint idCounter;

    private readonly List<Component> children = new();
    private Component? parent;
    private Transform? transform;
    private bool isEnabled = true;

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
    /// An event raised when the component has been enabled.
    /// </summary>
    public event EventHandler? Enabled;

    /// <summary>
    /// An event raised when the component has been disabled.
    /// </summary>
    public event EventHandler? Disabled;

    /// <inheritdoc/>
    IComponent? IComponent.Parent => this.Parent;

    /// <inheritdoc/>
    IEnumerable<IComponent> IComponent.Children => this.Children;

    /// <inheritdoc/>
    IComponent IComponent.Root => this.Root;

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
    public IComponent? Parent
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
    public Transform Transform => this.transform ??= Transform.Default(this);

    /// <summary>
    /// Gets an enumerable collection of child components.
    /// </summary>
    public IEnumerable<Component> Children => this.children;

    /// <summary>
    /// Gets the root component of the hierarchy.
    /// </summary>
    public Component Root => this.parent?.Root ?? this;

    /// <inheritdoc/>
    public bool IsEnabled
    {
        get => this.isEnabled;
        set
        {
            if (this.isEnabled == value)
            {
                return;
            }

            this.isEnabled = value;
            (this.isEnabled ? this.Enabled : this.Disabled)?.Invoke(this, EventArgs.Empty);
        }
    }

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

    /// <summary>
    /// Determines whether two components are equal.
    /// </summary>
    /// <param name="a">The first component to compare.</param>
    /// <param name="b">The second component to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the components are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Component? a, Component? b)
    {
        return a?.Equals(b) ?? false;
    }

    /// <summary>
    /// Determines whether two components are not equal.
    /// </summary>
    /// <param name="a">The first component to compare.</param>
    /// <param name="b">The second component to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the components are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
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
    /// <remarks>
    /// <para>
    /// If the component is not enabled,
    /// this method will exit without performing any updates.
    /// </para>
    /// <para>
    /// The method ensures that the component's transform is recalculated
    /// if necessary by calling <see cref="Transform.RecalculateIfNeeded"/>.
    /// </para>
    /// <para>
    /// If the child component has <see cref="AutoUpdate"/>
    /// set to <see langword="false"/>, the child will not be updated.
    /// </para>
    /// </remarks>
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
    /// Forces the component to update their state immediately,
    /// outside the normal update cycle.
    /// </summary>
    /// <param name="withTransform">
    /// Optional parameter that determines whether the component's
    /// <see cref="Transform"/> should be recalculated:
    /// <list type="bullet">
    /// <item><description>
    /// <c>true</c>: Forces recalculation of the component's transform.
    /// </description></item>
    /// <item><description>
    /// <c>false</c>: Skips the transform recalculation.
    /// </description></item>
    /// <item><description>
    /// <c>null</c>: Recalculates the transform only if needed.
    /// </description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// This method does not rely on <see cref="GameTime"/> and should be used cautiously.
    /// It is intended for scenarios where a component's state must be updated immediately,
    /// such as after a property change that affects the component's behavior,
    /// but <see cref="Update(GameTime)"/> method in normal cycle has been already called.
    /// <para>
    /// If the component is not enabled,
    /// this method will exit without performing any updates.
    /// </para>
    /// <para>
    /// The method will also propagate the forced update to all child components.
    /// </para>
    /// </remarks>
    public virtual void ForceUpdate(bool? withTransform = null)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (withTransform == true)
        {
            this.Transform.ForceRecalulcation();
        }
        else if (withTransform == null)
        {
            this.Transform.RecalculateIfNeeded();
        }

        foreach (Component child in this.children.ToList())
        {
            child.ForceUpdate();
        }
    }

    /// <summary>
    /// Draws the component and its child components.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <remarks>
    /// <para>
    /// If the component is not enabled,
    /// this method will exit without performing any drawing operations.
    /// </para>
    /// <para>
    /// If the component is marked as priority, it will be added to the priority queue.
    /// To draw priority components, call <see cref="DrawPriorityComponents(GameTime)"/>.
    /// </para>
    /// <para>
    /// If the child component has <see cref="AutoDraw"/>
    /// set to <see langword="false"/>, the child will not be drawn.
    /// </para>
    /// </remarks>
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
        where T : IComponent
    {
        return this.Children.OfType<T>().FirstOrDefault(c => predicate?.Invoke(c) ?? true);
    }

    /// <inheritdoc/>
    public T? GetDescendant<T>(Predicate<T>? predicate = null)
        where T : IComponent
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
        where T : IComponent
    {
        return this.children.OfType<T>().Where(c => predicate?.Invoke(c) ?? true);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetAllDescendants<T>(Predicate<T>? predicate = null)
        where T : IComponent
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

    /// <inheritdoc/>
    public bool IsParentOf(IComponent component)
    {
        return (IComponent)this == component.Parent;
    }

    /// <inheritdoc/>
    public bool IsAncestorOf(IComponent component)
    {
        return this.IsParentOf(component)
            || (component.Parent is not null && this.IsAncestorOf(component.Parent));
    }

    /// <inheritdoc/>
    public bool IsChildOf(IComponent component)
    {
        return component.Children.Contains(this);
    }

    /// <inheritdoc/>
    public bool IsDescendantOf(IComponent component)
    {
        return this.IsChildOf(component)
            || (this.Parent?.IsDescendantOf(component) ?? false);
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
}
