using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoRivUI;

/// <summary>
/// A static class that provides method to interact with the mouse input.
/// </summary>
public static class MouseController
{
    private static MouseState previousState;
    private static MouseState currentState;

    private static IComponent? focusedComponent;
    private static IComponent? draggedComponent;
    private static IComponent? previousDraggedComponent;
    private static bool isFocusedComponentPriority;

    /// <summary>
    /// Gets the current position of the mouse cursor.
    /// </summary>
    public static Point Position => currentState.Position;

    /// <summary>
    /// Gets the difference in position of the mouse cursor
    /// between the current and previous frames.
    /// </summary>
    public static Point MouseDelta => currentState.Position - previousState.Position;

    /// <summary>
    /// Gets the amount that the mouse scroll wheel has changed
    /// between the current and previous frames.
    /// </summary>
    /// <remarks>
    /// Positive values indicate scrolling upwards,
    /// while negative values indicate scrolling downwards.
    /// </remarks>
    public static int ScrollDelta => currentState.ScrollWheelValue - previousState.ScrollWheelValue;

    /// <summary>
    /// Gets the component that is currently dragged by the mouse.
    /// </summary>
    public static IComponent? DraggedComponent => draggedComponent;

    /// <summary>
    /// Gets a value indicating whether the drag state has changed.
    /// </summary>
    public static bool WasDragStateChanged => previousDraggedComponent != draggedComponent;

    /// <summary>
    /// Updates the mouse state.
    /// </summary>
    /// <remarks>
    /// This method should be called once per frame
    /// to keep the mouse state up to date.
    /// </remarks>
    public static void Update()
    {
        previousState = currentState;
        currentState = Mouse.GetState();

        DetectFocusedComponent();

        previousDraggedComponent = draggedComponent;
        DetectDraggedComponent();
    }

    /// <summary>
    /// Checks if a UI component is currently focused by the mouse.
    /// </summary>
    /// <param name="component">The UI component to be checked.</param>
    /// <returns>
    /// <see langword="true"/> if the specified component is focused by the mouse;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The <paramref name="component"/> is considered focused
    /// if the mouse cursor is within its boundaries and
    /// no other components appear above it.<br/>
    /// It is also considered focused if it is an ancestor
    /// of the currently focused component and the mouse position
    /// is within the <paramref name="component"/>'s boundaries.
    /// </remarks>
    public static bool IsComponentFocused(IComponent component)
    {
        return focusedComponent is not null
            && (focusedComponent == component
                || (component.Transform.DestRectangle.Contains(Position)
                        && component.IsAncestorOf(focusedComponent)
                    && (!isFocusedComponentPriority || component.IsPriority || IsParentPriority(component))));
    }

    /// <summary>
    /// Checks if the left mouse button is currently pressed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button is pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonPressed()
    {
        return currentState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the left mouse button is currently released.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button is released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonReleased()
    {
        return currentState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the left mouse button has been clicked.
    /// </summary>
    /// <remarks>
    /// The button has been clicked if pressed and
    /// then released since the last frame.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if the left mouse button has been clicked;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLeftButtonClicked()
    {
        return previousState.LeftButton == ButtonState.Pressed
            && currentState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the left mouse button was pressed in the previous frame.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button was pressed in the previous frame;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool WasLeftButtonPressed()
    {
        return previousState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the left mouse button was released in the previous frame.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the left mouse button was released in the previous frame;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool WasLeftButtonReleased()
    {
        return previousState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the right mouse button is currently pressed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button is pressed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRightButtonPressed()
    {
        return currentState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the right mouse button is currently released.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button is released;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRightButtonReleased()
    {
        return currentState.RightButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the right mouse button has been clicked.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button has been clicked;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The button has been clicked if it was pressed
    /// then released from the last frame.
    /// </remarks>
    public static bool IsRightButtonClicked()
    {
        return previousState.RightButton == ButtonState.Pressed
            && currentState.RightButton == ButtonState.Released;
    }

    /// <summary>
    /// Checks if the right mouse button was pressed in the previous frame.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button was pressed in the previous frame;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool WasRightButtonPressed()
    {
        return previousState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the right mouse button was released in the previous frame.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the right mouse button was released in the previous frame;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool WasRightButtonReleased()
    {
        return previousState.RightButton == ButtonState.Released;
    }

    private static void DetectFocusedComponent()
    {
        /* REFACTOR !!!
         * Detecting focusing component sucks, but works for now. */

        Queue<IOverlayScene> queue = new();
        foreach (OverlaySceneData data in Scene.DisplayedOverlays.Reverse())
        {
            queue.Enqueue(data.Scene);
            if (data.Options.BlockFocusOnUnderlyingScenes)
            {
                break;
            }
        }

        focusedComponent = null;
        while (queue.Count != 0)
        {
            IOverlayScene scene = queue.Dequeue();
            foreach (IComponent component in scene.OverlayComponents)
            {
                if (focusedComponent is not null)
                {
                    break;
                }

                UpdateFocusedPriorityComponent(component);
                if (focusedComponent is not null)
                {
                    isFocusedComponentPriority = true;
                }
                else
                {
                    UpdateFocusedComponent(component);
                    isFocusedComponentPriority = false;
                }
            }

            if (focusedComponent is not null)
            {
                break;
            }
        }

        if (focusedComponent is not null)
        {
            return;
        }

        UpdateFocusedPriorityComponent(Scene.Current.BaseComponent);

        if (focusedComponent is not null)
        {
            isFocusedComponentPriority = true;
        }
        else
        {
            UpdateFocusedComponent(Scene.Current.BaseComponent);
            isFocusedComponentPriority = false;
        }
    }

    private static void DetectDraggedComponent()
    {
        draggedComponent = null;

        foreach (OverlaySceneData data in Scene.DisplayedOverlays.Reverse())
        {
            foreach (IComponent component in data.Scene.OverlayComponents)
            {
                UpdateDraggedComponent(component);

                if (draggedComponent is not null)
                {
                    return;
                }
            }
        }

        foreach (IComponent component in Scene.Current.BaseComponent.Children.ToList())
        {
            UpdateDraggedComponent(component);

            if (draggedComponent is not null)
            {
                return;
            }
        }
    }

    private static bool IsParentPriority(IComponent component)
    {
        return component.Parent is not null
            && (component.Parent.IsPriority || IsParentPriority(component.Parent));
    }

    private static void UpdateFocusedPriorityComponent(IComponent component, bool parentPriority = false)
    {
        if (component.IsEnabled && (parentPriority || component.IsPriority) && component.Transform.DestRectangle.Contains(Position))
        {
            focusedComponent = component;
            parentPriority = true;
        }

        foreach (IComponent child in component.Children.ToList())
        {
            UpdateFocusedPriorityComponent(child, parentPriority);
        }
    }

    private static void UpdateFocusedComponent(IComponent component)
    {
        if (component.IsEnabled && component.Transform.DestRectangle.Contains(Position))
        {
            focusedComponent = component;
            foreach (IComponent child in component.Children.ToList())
            {
                UpdateFocusedComponent(child);
            }
        }
    }

    private static void UpdateDraggedComponent(IComponent component)
    {
        if (component is IDragable dragable)
        {
            dragable.UpdateDragState();

            if (dragable.IsDragging)
            {
                draggedComponent = component;
                return;
            }
        }

        foreach (IComponent child in component.Children.ToList())
        {
            UpdateDraggedComponent(child);
        }
    }
}
