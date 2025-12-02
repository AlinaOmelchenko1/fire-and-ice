using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace fire_and_ice.Interfaces
{
    /// <summary>
    /// Base interface for all game objects that need to update and draw
    /// </summary>
    public interface IGameObject
    {
        /// <summary>
        /// Updates the game object's state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Draws the game object to the screen
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        void Draw(SpriteBatch spriteBatch);
    }
}
