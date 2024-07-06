using Microsoft.Xna.Framework;
using System;

namespace MonoRivUI;

/// <summary>
/// Represents a scene in the game.
/// </summary>
public interface IScene
{
    /// <summary>
    /// Updates the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draws the scene's components.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Draw(GameTime gameTime);
}
