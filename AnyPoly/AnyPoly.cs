using AnyPoly.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnyPoly;

/// <summary>
/// Represents the main game class.
/// </summary>
public class AnyPoly : Game
{
    private readonly GraphicsDeviceManager graphics;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyPoly"/> class.
    /// </summary>
    public AnyPoly()
    {
        Instance = this;

        this.graphics = new GraphicsDeviceManager(this);

        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="AnyPoly"/> class.
    /// </summary>
    public static AnyPoly Instance { get; private set; } = default!;

    /// <summary>
    /// Initializes the game.
    /// </summary>
    protected override void Initialize()
    {
        ContentController.Initialize(this.Content);

        var spriteBatch = new SpriteBatch(this.GraphicsDevice);
        SpriteBatchController.Initialize(spriteBatch);

        ScreenController.Initialize(this.graphics);
        ScreenController.Change(1366, 768, ScreenType.Windowed);
        ScreenController.ApplyChanges();

        base.Initialize();
    }

    /// <summary>
    /// Loads the game content.
    /// </summary>
    protected override void LoadContent()
    {
        base.LoadContent();
    }

    /// <summary>
    /// Updates the game state.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <remarks>It is called once per frame.</remarks>
    protected override void Update(GameTime gameTime)
    {
        KeyboardController.Update();
        MouseController.Update();
        ScreenController.Update();

        base.Update(gameTime);
    }

    /// <summary>
    /// Draws the game.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <remarks>It is called once per frame.</remarks>
    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
