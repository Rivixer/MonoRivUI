using Microsoft.Xna.Framework;

namespace MonoRivUI.Scenes;

/// <summary>
/// The base scene.
/// </summary>
/// <remarks>
/// It is used to test UI componentes.
/// </remarks>
internal sealed class BaseScene : Scene
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        this.BaseComponent = new SolidColor(Color.Goldenrod);

        /* Add components here */
    }
}
