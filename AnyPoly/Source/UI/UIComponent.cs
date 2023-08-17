using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AnyPoly.UI;

/// <summary>
/// Represents a base class for UI components.
/// </summary>
internal abstract class UIComponent
{
    private static uint idCounter;
    private readonly List<UIComponent> children = new List<UIComponent>();
    private UIComponent? parent;
    private UITransform? transform;

    /// <summary>
    /// An event raised when a parent of the component has been changed.
    /// </summary>
    public event EventHandler<ParentChangeEventArgs>? OnParentChanged;

    /// <summary>
    /// An event raised when a child component has been added.
    /// </summary>
    public event EventHandler<ChildChangeEventArgs>? OnChildAdded;

    /// <summary>
    /// An event raised when a child component has been removed.
    /// </summary>
    public event EventHandler<ChildChangeEventArgs>? OnChildRemoved;

    /// <summary>
    /// Gets the transorm of the component.
    /// </summary>
    public UITransform Transform => this.transform ??= UITransform.Default(this);

    /// <summary>
    /// Gets or sets the parent component.
    /// </summary>
    /// <remarks>
    /// The parent component is responsible for managing child components.
    /// When setting the parent, several actions are performed:
    /// <list type="bullet">
    /// <item><description>
    /// Removes this component from its current parent's list of children
    /// and raises the <see cref="OnChildRemoved"/> event on the parent.
    /// </description></item>
    /// <item><description>
    /// Adds this component to the new parent's list of children
    /// and raises the <see cref="OnChildAdded"/> event on the new parent.
    /// </description></item>
    /// <item><description>
    /// Updates the <see cref="Transform.TransformType"/> of this component
    /// to <see cref="TransformType.Absolute"/> if the new parent is null,
    /// or to <see cref="TransformType.Relative"/> if there is a new parent.
    /// </description></item>
    /// <item><description>
    /// Raises the <see cref="OnParentChanged"/> event to notify listeners
    /// about the change in the parent.
    /// </description></item>
    /// </list>
    /// </remarks>
    public UIComponent? Parent
    {
        get => this.parent;
        set
        {
            if (this.parent == value)
            {
                return;
            }

            UIComponent? oldParent = this.parent;

            this.parent?.children.Remove(this);
            this.parent?.OnChildRemoved?.Invoke(this.parent, new ChildChangeEventArgs(this));

            this.parent = value;

            this.parent?.children.Add(this);
            this.parent?.OnChildAdded?.Invoke(this.parent, new ChildChangeEventArgs(this));

            this.Transform.TransformType = this.parent is null
                ? TransformType.Absolute
                : TransformType.Relative;

            this.OnParentChanged?.Invoke(this, new ParentChangeEventArgs(this.parent, oldParent));
        }
    }

    /// <summary>
    /// Gets an enumerable collection of child components.
    /// </summary>
    public IEnumerable<UIComponent> Children => this.children;

    /// <summary>
    /// Gets or sets a value indicating whether the component is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

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
    public bool AutoUpdate { protected get; set; } = true;

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
    public bool AutoDraw { protected get; set; } = true;

    /// <summary>
    /// Gets a unique indentifier for the component.
    /// </summary>
    protected uint Id { get; } = idCounter++;

    public static bool operator ==(UIComponent? a, UIComponent? b)
    {
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(UIComponent? a, UIComponent? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Updates the component and its child components.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    public virtual void Update(GameTime gameTime)
    {
        this.Transform.RecalculateIfNeeded();
        foreach (UIComponent child in this.children)
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
        foreach (UIComponent child in this.children)
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
        where T : UIComponent
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
        where T : UIComponent
    {
        T? descendant = this.GetChild(predicate);
        foreach (UIComponent child in this.children)
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
        where T : UIComponent
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
        where T : UIComponent
    {
        foreach (T child in this.GetAllChildren(predicate))
        {
            yield return child;
        }

        foreach (UIComponent child in this.children)
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
        return obj is UIComponent component
            && this.Id.Equals(component.Id);
    }

    /// <summary>
    /// Returns the hash code for the component based on its ID.
    /// </summary>
    /// <returns>The hash code for component.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 23) + this.Id.GetHashCode();
            return hash;
        }
    }
}
