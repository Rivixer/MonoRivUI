using AnyPoly.UI;
using Microsoft.Xna.Framework;

namespace AnyPoly.Scenes;

/// <summary>
/// Represents the lobby scene.
/// </summary>
internal class LobbyScene : Scene
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        this.ThrowErrorIfAlreadyInitialized();

        this.BaseComponent = new UISolidColor(Color.LightBlue);

        // Back button
        {
            var frame = new UIFrame(Color.Black, thickness: 4);
            var button = new UIButton<UIFrame>(frame)
            {
                Parent = this.BaseComponent,
                Transform =
                {
                    Alignment = Alignment.BottomLeft,
                    RelativeOffset = new Vector2(0.02f, -0.03f),
                    RelativeSize = new Vector2(0.13f, 0.05f),
                },
            };
            var background = new UISolidColor(Color.Gray) { Parent = frame.InnerContainer };
            var text = new UIText(Color.White)
            {
                Parent = frame.InnerContainer,
                Text = "Back to menu",
                TextAlignment = Alignment.Center,
                TextFit = TextFit.Both,
                Scale = 0.95f,
            };

            button.Clicked += (s, e) => Change<MenuScene>();
            button.HoverEntered += (s, e) => background.Color = Color.DarkGray;
            button.HoverExited += (s, e) => background.Color = Color.Gray;
        }

        this.MakeAsInitialized();
    }
}
