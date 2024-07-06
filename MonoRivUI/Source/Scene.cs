using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents a base class for scenes.
/// </summary>
public abstract class Scene : IScene
{
    private static readonly HashSet<Scene> Scenes = new();
    private static readonly Stack<Scene> SceneStack = new();
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

    /// <inheritdoc cref="IScene.Showed"/>
    protected event EventHandler? Showed;

    /// <inheritdoc cref="IScene.Hid"/>
    protected event EventHandler? Hid;

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public static Scene Current { get; private set; } = default!;

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
    /// and do not have the <see cref="NoAutoInitialize"/> attribute.
    /// </remarks>
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
            AddScene(scene);
        }
    }

    /// <summary>
    /// Initializes all scenes in the specified assembly.
    /// </summary>
    /// <param name="assemblyPath">The assembly path.</param>
    /// /// <remarks>
    /// Scenes are initialized if they are subclasses of <see cref="Scene"/>
    /// and do not have the <see cref="NoAutoInitialize"/> attribute.
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
    /// <param name="addCurrentToStack">Whether to add the current scene to the scene stack.</param>
    public static void Change<T>(bool addCurrentToStack = true)
        where T : Scene
    {
        var scene = Scenes.OfType<T>().Single();
        Change(scene, addCurrentToStack);
    }

    /// <summary>
    /// Changes the current scene to the specified scene.
    /// </summary>
    /// <param name="scene">The scene to change to.</param>
    /// <param name="addCurrentToStack">Whether to add the current scene to the scene stack.</param>
    public static void Change(Scene scene, bool addCurrentToStack = true)
    {
        if (Current != null && addCurrentToStack)
        {
            SceneStack.Push(Current);
        }

        Current?.Hid?.Invoke(null, EventArgs.Empty);
        Current = scene;
        Current.Showed?.Invoke(null, EventArgs.Empty);
        Current.baseComponent.Transform.ForceRecalulcation();
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="options">The options for showing the overlay scene.</param>
    public static void ShowOverlay<T>(OverlayShowOptions options)
        where T : Scene, IOverlayScene
    {
        T scene = Scenes.OfType<T>().Single();
        ShowOverlay(scene, options);
    }

    /// <summary>
    /// Shows an overlay scene.
    /// </summary>
    /// <typeparam name="T">The type of the overlay scene to show.</typeparam>
    /// <param name="scene">The overlay scene to show.</param>
    /// <param name="options">The options for showing the overlay scene.</param>
    public static void ShowOverlay<T>(T scene, OverlayShowOptions options)
        where T : Scene, IOverlayScene
    {
        if (DisplayedOverlaysData.Any(x => x.Scene == scene))
        {
            return;
        }

        DisplayedOverlaysData.Add(new OverlayData(scene, options));
        SortOverlayPriorities();
        scene.Showed?.Invoke(null, EventArgs.Empty);
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
    public static void ChangeToPreviousOr<T>(bool addCurrentToStack = false)
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
    /// Updates the overlay scenes.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public static void UpdateOverlays(GameTime gameTime)
    {
        if (!DisplayedOverlaysData.Any())
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
        if (!DisplayedOverlaysData.Any())
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
    /// Represents an attribute that indicates that the class should not be auto-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NoAutoInitialize : Attribute
    {
    }
}
