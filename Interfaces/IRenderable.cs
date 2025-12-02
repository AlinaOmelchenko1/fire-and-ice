using Microsoft.Xna.Framework.Graphics;

namespace fire_and_ice.Interfaces
{
    /// <summary>
    /// Interface for objects that can be rendered to the screen with a specific draw order
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Draws the object to the screen
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Gets the draw order for this renderable object.
        /// Lower values are drawn first (background), higher values are drawn last (foreground).
        /// Typical values:
        /// - Background: 0-100
        /// - Platforms/Terrain: 100-200
        /// - Game Objects: 200-300
        /// - Players: 300-400
        /// - Effects/Particles: 400-500
        /// - UI Elements: 500+
        /// </summary>
        int DrawOrder { get; }
    }
}
