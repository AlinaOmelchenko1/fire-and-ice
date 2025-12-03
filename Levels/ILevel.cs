using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Levels
{
    /// <summary>
    /// Represents a game level with its entities, platforms, and gameplay logic.
    /// Provides a common interface for all level implementations.
    /// </summary>
    public interface ILevel
    {
        /// <summary>
        /// Gets the unique identifier for this level
        /// </summary>
        string LevelId { get; }

        /// <summary>
        /// Gets the display name of the level
        /// </summary>
        string LevelName { get; }

        /// <summary>
        /// Gets the starting position for player 1
        /// </summary>
        Vector2 Player1StartPosition { get; }

        /// <summary>
        /// Gets the starting position for player 2
        /// </summary>
        Vector2 Player2StartPosition { get; }

        /// <summary>
        /// Gets the list of platforms in this level
        /// </summary>
        List<InteractableObject> Platforms { get; }

        /// <summary>
        /// Gets whether the level has been loaded
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Loads the level, creating all entities and initializing resources.
        /// This is called when entering the level.
        /// </summary>
        /// <param name="content">Content manager for loading textures and assets</param>
        /// <param name="graphicsDevice">Graphics device for creating textures</param>
        void Load(ContentManager content, GraphicsDevice graphicsDevice);

        /// <summary>
        /// Unloads the level, releasing resources and cleaning up entities.
        /// This is called when exiting the level.
        /// </summary>
        void Unload();

        /// <summary>
        /// Gets all game entities in this level (players, keys, doors, hazards, etc.)
        /// </summary>
        /// <returns>List of all entities in the level</returns>
        List<IGameObject> GetEntities();

        /// <summary>
        /// Gets all keys in this level
        /// </summary>
        /// <returns>List of keys</returns>
        List<Key> GetKeys();

        /// <summary>
        /// Gets all doors in this level
        /// </summary>
        /// <returns>List of doors</returns>
        List<Door> GetDoors();

        /// <summary>
        /// Gets all flames (fire hazards) in this level
        /// </summary>
        /// <returns>List of flames</returns>
        List<Flame> GetFlames();

        /// <summary>
        /// Gets all ice shards (ice hazards) in this level
        /// </summary>
        /// <returns>List of ice shards</returns>
        List<IceShard> GetIceShards();

        /// <summary>
        /// Gets the background texture for this level
        /// </summary>
        /// <returns>Background texture, or null if not loaded</returns>
        Texture2D GetBackgroundTexture();

        /// <summary>
        /// Resets the level to its initial state (respawns keys, closes doors, etc.)
        /// </summary>
        void Reset();
    }
}
