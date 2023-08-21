using System;
using AnyPoly.UI;
using Microsoft.Xna.Framework;

namespace AnyPoly;

/// <summary>
/// Represents a base class for scenes.
/// </summary>
internal abstract class Scene
{
    private bool isInitialized;

    /// <summary>
    /// Gets or sets the components of the scene.
    /// </summary>
    public UIComponent BaseComponent { get; protected set; } = default!;

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Updates the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public virtual void Update(GameTime gameTime)
    {
        this.BaseComponent.Update(gameTime);
    }

    /// <summary>
    /// Draws the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public virtual void Draw(GameTime gameTime)
    {
        this.BaseComponent.Draw(gameTime);
    }

    /// <summary>
    /// Makes the scene as initialized.
    /// </summary>
    protected void MakeAsInitialized()
    {
        this.isInitialized = true;
    }

    /// <summary>
    /// Throws an exception if the scene has already been created.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the scene has already been created.
    /// </exception>
    protected void ThrowErrorIfAlreadyInitialized()
    {
        if (this.isInitialized)
        {
            throw new InvalidOperationException(
                $"The {this.GetType()} scene has already been created.");
        }
    }
}
