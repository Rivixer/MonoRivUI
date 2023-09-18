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

        this.BaseComponent = new UISolidColor(Color.Khaki);

        var text = new UIText(Color.Black)
        {
            Parent = this.BaseComponent,
            Text = "Hello world!",
            TextAlignment = Alignment.Top,
            Scale = 3.0f,
        };

        var buttonListBox = new UIListBox()
        {
            Parent = this.BaseComponent,
            Spacing = 12,
            Orientation = Orientation.Vertical,
            ResizeContent = true,
            Transform =
            {
                Alignment = Alignment.Bottom,
                RelativeSize = new Vector2(0.20f),
                RelativeOffset = new Vector2(0.0f, -0.2f),
            },
        };

        var newGameButton = CreateMenuButton(buttonListBox.ContentContainer, "New game");
        newGameButton.Clicked += (s, e) =>
        {
            Debug.WriteLine("New game button has been clicked.");
        };

        var settingsButton = CreateMenuButton(buttonListBox.ContentContainer, "Settings");
        settingsButton.Clicked += (s, e) =>
        {
            Debug.WriteLine("Settings button has been clicked.");
        };

        var exitButton = CreateMenuButton(buttonListBox.ContentContainer, "Exit");
        exitButton.Clicked += (s, e) => AnyPolyGame.Instance.Exit();

        static UIButton<UIFrame> CreateMenuButton(IUIReadOnlyComponent parent, string text)
        {
            var frame = new UIFrame(Color.Black, thickness: 2);
            var frameFill = new UISolidColor(Color.Gray) { Parent = frame.InnerContainer };
            var button = new UIButton<UIFrame>(frame) { Parent = parent };
            var buttonText = new UIText(Color.White)
            {
                Parent = button,
                Text = text,
                TextAlignment = Alignment.Center,
                TextFit = TextFit.Both,
                Scale = 0.8f,
            };
            button.HoverEntered += (s, e) => frameFill.Color = Color.DarkGray;
            button.HoverExited += (s, e) => frameFill.Color = Color.Gray;
            return button;
        }

        this.MakeAsInitialized();
    }
}
