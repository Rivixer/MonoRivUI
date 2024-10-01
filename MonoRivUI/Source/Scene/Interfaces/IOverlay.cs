using Microsoft.Xna.Framework;

namespace MonoRivUI;

/// <summary>
/// Represents an overlay object.
/// </summary>
public interface IOverlay
{
    /// <summary>
    /// Gets the priority of the overlay object.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Updates the overlay object.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draws the overlay object.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Draw(GameTime gameTime);
}
