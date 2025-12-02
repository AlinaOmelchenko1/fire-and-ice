using Microsoft.Xna.Framework;

namespace fire_and_ice.Interfaces
{
    /// <summary>
    /// Interface for objects that can participate in collision detection
    /// </summary>
    public interface ICollidable
    {
        /// <summary>
        /// Gets the bounding rectangle for collision detection.
        /// This represents the area that can collide with other objects.
        /// </summary>
        /// <returns>A Rectangle representing the collision bounds</returns>
        Rectangle GetBounds();

        /// <summary>
        /// Checks if this object collides with another collidable object.
        /// This method should perform the collision test and return true if a collision occurs.
        /// </summary>
        /// <param name="other">The other collidable object to check against</param>
        /// <returns>True if the objects are colliding, false otherwise</returns>
        bool CheckCollision(ICollidable other);
    }
}
