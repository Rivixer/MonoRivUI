using AnyPoly.UI;
using Microsoft.Xna.Framework;

namespace AnyPoly.Scenes;

/// <summary>
/// Represents a menu scene.
/// </summary>
/// <remarks>
/// This is the first scene
/// displayed when the game starts.
/// </remarks>
internal class MenuScene : Scene
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        this.ThrowErrorIfAlreadyInitialized();

        this.BaseComponent = new UIContainer();

        var text = new UIText(Color.Black)
        {
            Parent = this.BaseComponent,
            Text = "Hello World!",
            TextAlignment = Alignment.Center,
            Scale = 3.0f,
        };

        this.MakeAsInitialized();
    }
}
