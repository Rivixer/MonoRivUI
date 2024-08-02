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
    private static readonly Stack<(Scene, ChangeEventArgs)> SceneStack = new();
    private static readonly List<OverlayData> DisplayedOverlaysData = new();

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

        this.baseComponent.Transform.Recalculated += (s, e) =>
        {
            (s as Transform)!.Size = ScreenController.CurrentSize;
        };
    }

    /// <summary>
    /// Occurs when the scene is about to be shown.
    /// </summary>
    protected event EventHandler<ChangeEventArgs?>? Showing;

    /// <summary>
    /// Occurs when the scene has been shown.
    /// </summary>
    protected event EventHandler<ChangeEventArgs?>? Showed;

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
    public static ChangeEventArgs CurrentChangeEventArgs { get; private set; } = default!;

    /// <summary>
    /// Gets the currently displayed overlays.
    /// </summary>
    public static IEnumerable<OverlayData> DisplayedOverlays => DisplayedOverlaysData;

    /// <summary>
    /// Gets the base component of the scene.
    /// </summary>
    public IReadOnlyComponent BaseComponent => this.baseComponent;

    /// <summary>
    /// Gets a value indicating whether the scene is displayed.
    /// </summary>
    protected bool IsDisplayed => Current == this;

    /// <summary>
    /// Gets a value indicating whether the scene is displayed as an overlay.
    /// </summary>
    protected bool IsDisplayedOverlay => DisplayedOverlaysData.Any(x => x.Scene == this);

    /// <summary>
    /// Initializes all scenes in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <remarks>
    /// Scenes are initialized if they are subclasses of <see cref="Scene"/>
    /// and do not have the <see cref="NoAutoInitializeAttribute"/> attribute.
    /// </remarks>
    public static void InitializeScenes([NotNull] Assembly assembly)
    {
        var sceneTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Scene))
            && t.GetCustomAttribute<NoAutoInitializeAttribute>() is null
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
    /// and do not have the <see cref="NoAutoInitializeAttribute"/> attribute.
    /// </remarks>
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
        _ = Scenes.Add(scene);
        if (scene is IOverlayScene priorityScene)
        {
            priorityScene.PriorityChanged += (s, e) => SortOverlayPriorities();
        }
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
        Change<T>(ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <param name="args">The arguments for the change event.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change<T>(ChangeEventArgs args)
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
        Change(scene, ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <param name="args">The arguments for the change event.</param>
    /// <remarks>
    /// This method adds the current scene to the scene stack.
    /// </remarks>
    public static void Change(Scene scene, ChangeEventArgs args)
    {
        if (Current != null)
        {
            SceneStack.Push((Current, CurrentChangeEventArgs));
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
        ChangeWithoutStack<T>(ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <typeparam name="T">The type of the scene to change to.</typeparam>
    /// <param name="args">The arguments for the change event.</param>
    public static void ChangeWithoutStack<T>(ChangeEventArgs args)
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
        ChangeWithoutStack(scene, ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Changes the current scene to the specified scene
    /// without adding it to the scene stack.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <param name="args">The arguments for the change event.</param>
    public static void ChangeWithoutStack(Scene scene, ChangeEventArgs args)
    {
        Scene? previous = Current;

        previous?.Hiding?.Invoke(null, EventArgs.Empty);
        scene.Showing?.Invoke(null, args);

        Current = scene;
        CurrentChangeEventArgs = args;

        previous?.Hid?.Invoke(null, EventArgs.Empty);
        Current.Showed?.Invoke(null, args);

        Current.baseComponent.Transform.ForceRecalulcation();
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="options">The options for showing the overlay scene.</param>
    public static void ShowOverlay<T>(OverlayShowOptions options = default)
        where T : Scene, IOverlayScene
    {
        ShowOverlay<T>(options, ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="options">The options for showing the overlay scene.</param>
    /// <param name="args">The arguments for the change event.</param>
    public static void ShowOverlay<T>(OverlayShowOptions options, ChangeEventArgs args)
        where T : Scene, IOverlayScene
    {
        T scene = Scenes.OfType<T>().Single();
        ShowOverlay(scene, options, args);
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="scene">The overlay scene to show.</param>
    /// <param name="options">The options for showing the overlay scene.</param>
    public static void ShowOverlay<T>(T scene, OverlayShowOptions options = default)
        where T : Scene, IOverlayScene
    {
        ShowOverlay(scene, options, ChangeEventArgs.Empty);
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="scene">The overlay scene to show.</param>
    /// <param name="options">The options for showing the overlay scene.</param>
    /// <param name="args">The arguments for the change event.</param>
    public static void ShowOverlay<T>(T scene, OverlayShowOptions options, ChangeEventArgs args)
        where T : Scene, IOverlayScene
    {
        if (DisplayedOverlaysData.Any(x => x.Scene == scene))
        {
            return;
        }

        DisplayedOverlaysData.Add(new OverlayData(scene, options));
        SortOverlayPriorities();
        scene.Showed?.Invoke(null, args);
    }

    /// <summary>
    /// Hides an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to hide.</typeparam>
    public static void HideOverlay<T>()
        where T : Scene, IOverlayScene
    {
        T scene = Scenes.OfType<T>().Single();
        HideOverlay(scene);
    }

    /// <summary>
    /// Hides an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to hide.</typeparam>
    /// <param name="scene">The overlay scene to hide.</param>
    public static void HideOverlay<T>(T scene)
        where T : Scene, IOverlayScene
    {
        DisplayedOverlaysData.RemoveAt(DisplayedOverlaysData.FindIndex(x => x.Scene == scene));
        scene.Hid?.Invoke(null, EventArgs.Empty);
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

        var (previous, args) = SceneStack.Pop();
        Current.Hiding?.Invoke(null, EventArgs.Empty);
        previous.Showing?.Invoke(null, args);

        Current = previous;
        CurrentChangeEventArgs = args;
        Current.baseComponent.Transform.ForceRecalulcation();

        Current.Hid?.Invoke(null, EventArgs.Empty);
        Current.Showed?.Invoke(null, args);
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
    public static void ChangeToPreviousOrDefault<T>(ChangeEventArgs? defaultArgs = null)
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
    public static void ChangeToPreviousOrDefault(Scene scene, ChangeEventArgs? defaultArgs = null)
    {
        defaultArgs ??= ChangeEventArgs.Empty;

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
        ChangeEventArgs? defaultArgs = null)
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
        ChangeEventArgs? defaultArgs = null)
    {
        defaultArgs ??= ChangeEventArgs.Empty;

        if (SceneStack.Count > 0)
        {
            ChangeToPrevious();
        }
        else
        {
            ChangeWithoutStack(scene, defaultArgs);
        }
    }

    /// <summary>
    /// Updates the overlay scenes.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void UpdateOverlays(GameTime gameTime)
    {
        if (DisplayedOverlaysData.Count == 0)
        {
            return;
        }

        var stack = new Stack<OverlayData>();
        foreach (OverlayData data in DisplayedOverlaysData.AsEnumerable().Reverse())
        {
            stack.Push(data);
            if (data.Options.BlockUpdateOnUnderlyingScenes)
            {
                break;
            }
        }

        while (stack.Count > 0)
        {
            IOverlayScene scene = stack.Pop().Scene;
            scene.Update(gameTime);
        }
    }

    /// <summary>
    /// Draws the overlay scenes.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void DrawOverlays(GameTime gameTime)
    {
        if (DisplayedOverlaysData.Count == 0)
        {
            return;
        }

        var stack = new Stack<OverlayData>();
        foreach (OverlayData data in DisplayedOverlaysData.AsEnumerable().Reverse())
        {
            stack.Push(data);
            if (data.Options.BlockDrawOnUnderlyingScenes)
            {
                break;
            }
        }

        while (stack.Count > 0)
        {
            IOverlayScene scene = stack.Pop().Scene;
            scene.Draw(gameTime);
            Component.DrawPriorityComponents(gameTime);
        }
    }

    /// <inheritdoc/>
    public virtual void Update(GameTime gameTime)
    {
        if (DisplayedOverlaysData.Any(x => x.Options.BlockUpdateOnUnderlyingScenes))
        {
            return;
        }

        this.baseComponent.Update(gameTime);
    }

    /// <inheritdoc/>
    public virtual void Draw(GameTime gameTime)
    {
        if (DisplayedOverlaysData.Any(x => x.Options.BlockDrawOnUnderlyingScenes))
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
    /// Initializes the scene.
    /// </summary>
    /// <param name="baseComponent">The base component.</param>
    protected abstract void Initialize(Component baseComponent);

    /// <summary>
    /// Sets the background color of the scene.
    /// </summary>
    /// <param name="color">The color of the background.</param>
    protected void SetBackground(Color color)
    {
        this.baseComponent.Color = color;
    }

    private static void SortOverlayPriorities()
    {
        DisplayedOverlaysData.Sort((a, b) => a.Scene.Priority.CompareTo(b.Scene.Priority));
    }

    /// <summary>
    /// Represents options for displaying an overlay scene.
    /// </summary>
    /// <param name="BlockFocusOnUnderlyingScenes">
    /// Indicates whether to prevent underlying scenes from receiving focus.
    /// </param>
    /// <param name="BlockUpdateOnUnderlyingScenes">
    /// Indicates whether to prevent underlying scenes from being updated.
    /// </param>
    /// <param name="BlockDrawOnUnderlyingScenes">
    /// Indicates whether to prevent underlying scenes from being drawn.
    /// </param>
    public record struct OverlayShowOptions(
        bool BlockFocusOnUnderlyingScenes = false,
        bool BlockUpdateOnUnderlyingScenes = false,
        bool BlockDrawOnUnderlyingScenes = false);

    /// <summary>
    /// Represents a data structure for displaying an overlay scene.
    /// </summary>
    public class OverlayData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayData"/> class.
        /// </summary>
        /// <param name="scene">
        /// The overlay scene.
        /// </param>
        /// <param name="options">
        /// The options for showing the overlay scene.
        /// </param>
        public OverlayData(IOverlayScene scene, OverlayShowOptions options)
        {
            this.Scene = scene;
            this.Options = options;
        }

        /// <summary>
        /// Gets the overlay scene.
        /// </summary>
        public IOverlayScene Scene { get; }

        /// <summary>
        /// Gets the options for showing the overlay scene.
        /// </summary>
        public OverlayShowOptions Options { get; }
    }

    /// <summary>
    /// Represents a change scene event arguments.
    /// </summary>
    public abstract class ChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets an empty instance of the <see cref="ChangeEventArgs"/> class.
        /// </summary>
        public static new ChangeEventArgs Empty { get; } = new EmptyChangeEventArgs();

        private class EmptyChangeEventArgs : ChangeEventArgs
        {
        }
    }

    /// <summary>
    /// Represents an attribute that indicates that the class should not be auto-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NoAutoInitializeAttribute : Attribute
    {
    }
}
