using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for scenes.
/// </summary>
public abstract class Scene
{
    private static readonly List<Scene> Scenes = new();
    private bool isInitialized;

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public static Scene Current { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the components of the scene.
    /// </summary>
    public Component BaseComponent { get; protected set; } = default!;

    /// <summary>
    /// Initializes all scenes.
    /// </summary>
    public static void InitializeScenes()
    {
        var sceneTypes = typeof(Scene).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Scene)) && !t.IsAbstract);

        foreach (var sceneType in sceneTypes)
        {
            var scene = (Scene)Activator.CreateInstance(sceneType)!;
            Scenes.Add(scene);
            scene.Initialize();
        }
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    public static void Change<T>()
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        Current = scene;
    }

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
