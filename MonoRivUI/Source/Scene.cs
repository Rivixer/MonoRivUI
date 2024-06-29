using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for scenes.
/// </summary>
public abstract class Scene
{
    private static readonly List<Scene> Scenes = new();
    private static readonly Stack<Scene> SceneStack = new();
    private readonly Component baseComponent;
    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    /// <param name="backgroundColor">The color of the background.</param>
    protected Scene(Color backgroundColor)
    {
        this.baseComponent = new SolidColor(backgroundColor)
        {
            Transform =
            {
                Type = TransformType.Absolute,
            },
        };

        this.baseComponent.Transform.Recalculated += (s, e) => (s as Transform)!.Size = ScreenController.CurrentSize;
    }

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public static Scene Current { get; private set; } = default!;

    /// <summary>
    /// Gets the base component of the scene.
    /// </summary>
    public IUIReadOnlyComponent BaseComponent => this.baseComponent;

    /// <summary>
    /// Initializes all scenes.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public static void InitializeScenes([NotNull] Assembly assembly)
    {
        var sceneTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Scene))
            && t.GetCustomAttribute<NoAutoInitialize>() is null
            && !t.IsAbstract);

        foreach (var sceneType in sceneTypes)
        {
            var scene = (Scene)Activator.CreateInstance(sceneType)!;
            scene.Initialize();
            Scenes.Add(scene);
        }
    }

    /// <summary>
    /// Initializes all scenes.
    /// </summary>
    /// <param name="assemblyPath">The assembly path.</param>
    public static void InitializeScenes(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        InitializeScenes(assembly);
    }

    /// <summary>
    /// Adds a scene to the list of scenes.
    /// </summary>
    /// <param name="scene">The scene to add.</param>
    public static void AddScene(Scene scene)
    {
        Scenes.Add(scene);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <param name="addCurrentToStack">Whether to add the current scene to the scene stack.</param>
    public static void Change<T>(bool addCurrentToStack = true)
        where T : Scene
    {
        if (Current != null && addCurrentToStack)
        {
            SceneStack.Push(Current);
        }

        var scene = Scenes.OfType<T>().Single();
        Current = scene;
        scene.baseComponent.Transform.ForceRecalulcation();
    }

    /// <summary>
    /// Changes the current scene to the previous scene.
    /// </summary>
    /// <exception cref="InvalidOperationException">The current scene does not have a previous scene.</exception>
    public static void ChangeToPrevious()
    {
        if (SceneStack.Count == 0)
        {
            throw new InvalidOperationException("The current scene does not have a previous scene.");
        }

        Current = SceneStack.Pop();
        Current.baseComponent.Transform.ForceRecalulcation();
    }

    /// <summary>
    /// Changes the current scene to the previous scene if it exists,
    /// otherwise changes to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to if there is no previous scene.</typeparam>
    /// <param name="addCurrentToStack">
    /// Whether to add the current scene to the scene stack if there is no previous scene.
    /// </param>
    public static void ChangeToPreviousOr<T>(bool addCurrentToStack)
        where T : Scene
    {
        if (SceneStack.Count == 0)
        {
            Change<T>(addCurrentToStack);
        }
        else
        {
            ChangeToPrevious();
        }
    }

    /// <summary>
    /// Updates the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public virtual void Update(GameTime gameTime)
    {
        this.baseComponent.Update(gameTime);
    }

    /// <summary>
    /// Draws the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public virtual void Draw(GameTime gameTime)
    {
        this.baseComponent.Draw(gameTime);
    }

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    public void Initialize()
    {
        if (this.isInitialized)
        {
            throw new InvalidOperationException("The scene has already been initialized.");
        }

        this.Initialize(this.baseComponent);

        this.isInitialized = true;
    }

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    /// <param name="baseComponent">The base component.</param>
    protected abstract void Initialize(Component baseComponent);

    /// <summary>
    /// Represents an attribute that indicates that the class should not be auto-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NoAutoInitialize : Attribute
    {
    }
}
