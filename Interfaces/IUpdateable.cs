using Microsoft.Xna.Framework;

namespace fire_and_ice.Interfaces
{
    /// <summary>
    /// Interface for objects that can be updated each frame
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Updates the object's state based on game time
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Gets or sets whether this object should be updated.
        /// When false, the Update method should not be called.
        /// This allows objects to be temporarily disabled without removing them from collections.
        /// </summary>
        bool Enabled { get; set; }
    }
}
