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
public abstract class Scene : IScene
{
    private static readonly HashSet<Scene> Scenes = new();
    private static readonly Stack<(Scene, SceneDisplayEventArgs)> SceneStack = new();

    private readonly SolidColor baseComponent;
    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    /// <remarks>This constructor initializes a scene with a transparent background.</remarks>
    protected Scene()
        : this(Color.Transparent)
    {
    }

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

        this.baseComponent.Load();

        this.baseComponent.Transform.Recalculating += (s, e) =>
        {
            (s as Transform)!.Size = ScreenController.CurrentSize;
        };
    }

    /// <summary>
    /// Occurs when the scene has changed.
    /// </summary>
    public static event EventHandler<SceneChangedEventArgs>? SceneChanged;

    /// <summary>
    /// Occurs when the scene is about to be shown.
    /// </summary>
    protected event EventHandler<SceneDisplayEventArgs?>? Showing;

    /// <summary>
    /// Occurs when the scene has been shown.
    /// </summary>
    protected event EventHandler<SceneDisplayEventArgs?>? Showed;

    /// <summary>
    /// Occurs when the scene is about to be hidden.
    /// </summary>
    protected event EventHandler? Hiding;

    /// <summary>
    /// Occurs when the scene has been hidden.
    /// </summary>
    protected event EventHandler? Hid;

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public static Scene Current { get; private set; } = default!;

    /// <summary>
    /// Gets the current change event arguments.
    /// </summary>
    public static SceneDisplayEventArgs CurrentDisplayEventArgs { get; private set; } = default!;

    /// <summary>
    /// Gets a value indicating whether the scene content is loaded.
    /// </summary>
    public bool IsContentLoaded { get; private set; }

    /// <summary>
    /// Gets the base component of the scene.
    /// </summary>
    public IComponent BaseComponent => this.baseComponent;

    /// <summary>
    /// Gets a value indicating whether the scene is displayed.
    /// </summary>
    protected bool IsDisplayed => Current == this;

    /// <summary>
    /// Gets a value indicating whether the scene is currently displayed as an overlay.
    /// </summary>
    protected bool IsDisplayedOverlay => this is IOverlayScene s && ScreenController.IsOverlayDisplayed(s);

    /// <summary>
    /// Initializes all scenes in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <remarks>
    /// Scenes are initialized if they are subclasses of <see cref="Scene"/>
    /// and have the <see cref="AutoInitializeAttribute"/> attribute.
    /// </remarks>
    public static void InitializeScenes([NotNull] Assembly assembly)
    {
        var sceneTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Scene))
            && t.GetCustomAttribute<AutoInitializeAttribute>() is not null
            && !t.IsAbstract);

        foreach (var sceneType in sceneTypes)
        {
            var scene = (Scene)Activator.CreateInstance(sceneType)!;
            scene.Initialize();
            AddScene(scene);
        }
    }

    /// <summary>
    /// Initializes all scenes in the specified assembly.
    /// </summary>
    /// <param name="assemblyPath">The assembly path.</param>
    /// /// <remarks>
    /// Scenes are initialized if they are subclasses of <see cref="Scene"/>
    /// and have the <see cref="AutoInitializeAttribute"/> attribute.
    /// </remarks>
    public static void InitializeScenes(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        InitializeScenes(assembly);
    }

    /// <summary>
    /// Loads the content of all scenes.
    /// </summary>
    public static void LoadAllContent()
    {
        foreach (var scene in Scenes)
        {
            if (scene.GetType().GetCustomAttribute<AutoLoadContentAttribute>() is not null)
            {
                scene.LoadSceneContent();
            }
        }
    }

    /// <summary>
    /// Adds a scene to the list of scenes.
    /// </summary>
    /// <param name="scene">The scene to add.</param>
    public static void AddScene(Scene scene)
    {
        _ = Scenes.Add(scene);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change<T>()
         where T : Scene
    {
        Change<T>(SceneDisplayEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <param name="args">The arguments for the change event.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change<T>(SceneDisplayEventArgs args)
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        Change(scene, args);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change(Scene scene)
    {
        Change(scene, SceneDisplayEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <param name="args">The arguments for the change event.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change(Scene scene, SceneDisplayEventArgs args)
    {
        if (Current != null)
        {
            SceneStack.Push((Current, CurrentDisplayEventArgs));
        }

        ChangeWithoutStack(scene, args);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    public static void ChangeWithoutStack<T>()
        where T : Scene
    {
        ChangeWithoutStack<T>(SceneDisplayEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <param name="args">The arguments for the change event.</param>
    public static void ChangeWithoutStack<T>(SceneDisplayEventArgs args)
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        ChangeWithoutStack(scene, args);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    public static void ChangeWithoutStack(Scene scene)
    {
        ChangeWithoutStack(scene, SceneDisplayEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <param name="args">The arguments for the change event.</param>
    public static void ChangeWithoutStack(Scene scene, SceneDisplayEventArgs args)
    {
        Scene? previous = Current;

        OnSceneHiding(previous);
        OnSceneShowing(scene, args);

        Current = scene;
        CurrentDisplayEventArgs = args;

        Current.baseComponent.ForceUpdate(withTransform: true);

        OnSceneHid(previous);
        OnSceneShowed(scene, args);
        OnSceneChanged(previous, Current);
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="options">The options for displaying the overlay scene.</param>
    /// <param name="args">The arguments for the display event.</param>
    public static void ShowOverlay<T>(OverlayShowOptions options = default, SceneDisplayEventArgs? args = null)
        where T : Scene, IOverlayScene
    {
        var scene = Scenes.OfType<T>().Single();
        args ??= SceneDisplayEventArgs.Empty;

        if (!ScreenController.IsOverlayDisplayed(scene))
        {
            OnSceneShowing(scene, args);
            ScreenController.ShowOverlay(scene, options);
            OnSceneShowed(scene, args);
        }
    }

    /// <summary>
    /// Hides an overlay scene.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the overlay scene to hide.
    /// </typeparam>
    public static void HideOverlay<T>()
        where T : Scene, IOverlayScene
    {
        var scene = Scenes.OfType<T>().Single();

        if (ScreenController.IsOverlayDisplayed(scene))
        {
            OnSceneHiding(scene);
            ScreenController.HideOverlay(scene);
            OnSceneHid(scene);
        }
    }

    /// <summary>
    /// Changes the current scene to the previous scene.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The current scene does not have a previous scene.
    /// </exception>
    public static void ChangeToPrevious()
    {
        if (SceneStack.Count == 0)
        {
            throw new InvalidOperationException(
                "The current scene does not have a previous scene.");
        }

        var (previousScene, previousArgs) = SceneStack.Pop();

        OnSceneHiding(Current);
        OnSceneShowing(previousScene, previousArgs);

        var previousCurrent = Current;
        Current = previousScene;
        CurrentDisplayEventArgs = previousArgs;

        Current.baseComponent.ForceUpdate(withTransform: true);

        OnSceneHid(previousCurrent);
        OnSceneShowed(Current, previousArgs);
        OnSceneChanged(previousCurrent, previousScene);
    }

    /// <summary>
    /// Resets the scene stack.
    /// </summary>
    public static void ResetSceneStack()
    {
        SceneStack.Clear();
    }

    /// <summary>
    /// Changes the current scene to the previous scene if it exists,
    /// otherwise changes to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to if there is no previous scene.</typeparam>
    /// <param name="defaultArgs">The arguments for the change event if there is no previous scene.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack if there is no previous scene.
    /// </remarks>
    public static void ChangeToPreviousOrDefault<T>(SceneDisplayEventArgs? defaultArgs = null)
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        ChangeToPreviousOrDefault(scene, defaultArgs);
    }

    /// <summary>
    /// Changes the current scene to the previous scene if it exists.
    /// </summary>
    /// <param name="scene">The scene to change to if there is no previous scene.</param>
    /// <param name="defaultArgs">The arguments for the change event if there is no previous scene.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack if there is no previous scene.
    /// </remarks>
    public static void ChangeToPreviousOrDefault(Scene scene, SceneDisplayEventArgs? defaultArgs = null)
    {
        defaultArgs ??= SceneDisplayEventArgs.Empty;

        if (SceneStack.Count > 0)
        {
            ChangeToPrevious();
        }
        else
        {
            Change(scene, defaultArgs);
        }
    }

    /// <summary>
    /// Changes the current scene to the previous scene.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the scene to change to if there is no previous scene.
    /// </typeparam>
    /// <param name="defaultArgs">
    /// The arguments for the change event if there is no previous scene.
    /// </param>
    /// <remarks>
    /// This method does not add the current scene
    /// to the scene stack if there is no previous scene.
    /// </remarks>
    public static void ChangeToPreviousOrDefaultWithoutStack<T>(
        SceneDisplayEventArgs? defaultArgs = null)
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        ChangeToPreviousOrDefaultWithoutStack(scene, defaultArgs);
    }

    /// <summary>
    /// Changes the current scene to the previous scene.
    /// </summary>
    /// <param name="scene">
    /// The scene to change to if there is no previous scene.
    /// </param>
    /// <param name="defaultArgs">
    /// The arguments for the change event if there is no previous scene.
    /// </param>
    /// <remarks>
    /// This method does not add the current scene
    /// to the scene stack if there is no previous scene.
    /// </remarks>
    public static void ChangeToPreviousOrDefaultWithoutStack(
        Scene scene,
        SceneDisplayEventArgs? defaultArgs = null)
    {
        defaultArgs ??= SceneDisplayEventArgs.Empty;

        if (SceneStack.Count > 0)
        {
            ChangeToPrevious();
        }
        else
        {
            ChangeWithoutStack(scene, defaultArgs);
        }
    }

    /// <inheritdoc/>
    public virtual void Update(GameTime gameTime)
    {
        if (ScreenController.DisplayedOverlays.Any(x => x.Options.BlockUpdateOnUnderlyingScenes))
        {
            return;
        }

        this.baseComponent.Update(gameTime);
    }

    /// <inheritdoc/>
    public virtual void Draw(GameTime gameTime)
    {
        if (ScreenController.DisplayedOverlays.Any(x => x.Options.BlockDrawOnUnderlyingScenes))
        {
            return;
        }

        this.baseComponent.Draw(gameTime);
        Component.DrawPriorityComponents(gameTime);
    }

    /// <inheritdoc/>
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
    /// Loads the content of the scene.
    /// </summary>
    public void LoadContent()
    {
        if (this.IsContentLoaded)
        {
            throw new InvalidOperationException("The scene content has already been loaded.");
        }

        if (!this.isInitialized)
        {
            throw new InvalidOperationException("The scene has not been initialized.");
        }

        this.baseComponent.Load();
        this.LoadSceneContent();
        this.IsContentLoaded = true;
    }

    /// <summary>
    /// Gets the scene of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the scene to get.</typeparam>
    /// <returns>
    /// The scene of the specified type if it exists;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    protected static T? GetScene<T>()
        where T : Scene
    {
        return Scenes.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    /// <param name="baseComponent">The base component.</param>
    protected abstract void Initialize(Component baseComponent);

    /// <summary>
    /// Loads the content of the scene.
    /// </summary>
    protected abstract void LoadSceneContent();

    /// <summary>
    /// Sets the background color of the scene.
    /// </summary>
    /// <param name="color">The color of the background.</param>
    protected void SetBackground(Color color)
    {
        this.baseComponent.Color = color;
    }

    private static void OnSceneHiding(Scene? scene)
    {
        scene?.Hiding?.Invoke(null, EventArgs.Empty);
    }

    private static void OnSceneShowing(Scene scene, SceneDisplayEventArgs? args)
    {
        scene.Showing?.Invoke(null, args);
    }

    private static void OnSceneHid(Scene? scene)
    {
        scene?.Hid?.Invoke(null, EventArgs.Empty);
    }

    private static void OnSceneShowed(Scene scene, SceneDisplayEventArgs? args)
    {
        scene.Showed?.Invoke(null, args);
    }

    private static void OnSceneChanged(Scene? previous, Scene current)
    {
        SceneChanged?.Invoke(null, new SceneChangedEventArgs(previous, current));
    }
}
