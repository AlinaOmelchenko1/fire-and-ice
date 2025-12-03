using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Levels
{
    /// <summary>
    /// Implementation of Level 1 - the first level of the game.
    /// Contains platforms, keys, doors, fire hazards, and ice hazards.
    /// </summary>
    public class Level1 : ILevel
    {
        private Texture2D _backgroundTexture;
        private Texture2D _pixelTexture;
        private List<InteractableObject> _platforms;
        private List<Key> _keys;
        private List<Door> _doors;
        private List<Flame> _flames;
        private List<IceShard> _iceShards;
        private bool _isLoaded;

        /// <summary>
        /// Gets the unique identifier for this level
        /// </summary>
        public string LevelId => "level1";

        /// <summary>
        /// Gets the display name of the level
        /// </summary>
        public string LevelName => "Level 1: Fire and Ice Temple";

        /// <summary>
        /// Gets the starting position for player 1 (fire character)
        /// </summary>
        public Vector2 Player1StartPosition => GameConstants.SpawnPositions.Player1Start;

        /// <summary>
        /// Gets the starting position for player 2 (ice character)
        /// </summary>
        public Vector2 Player2StartPosition => GameConstants.SpawnPositions.Player2Start;

        /// <summary>
        /// Gets the list of platforms in this level
        /// </summary>
        public List<InteractableObject> Platforms => _platforms;

        /// <summary>
        /// Gets whether the level has been loaded
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// Creates a new instance of Level 1
        /// </summary>
        public Level1()
        {
            _platforms = new List<InteractableObject>();
            _keys = new List<Key>();
            _doors = new List<Door>();
            _flames = new List<Flame>();
            _iceShards = new List<IceShard>();
            _isLoaded = false;
        }

        /// <summary>
        /// Loads the level, creating all entities and initializing resources
        /// </summary>
        /// <param name="content">Content manager for loading textures and assets</param>
        /// <param name="graphicsDevice">Graphics device for creating textures</param>
        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            if (_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine($"Level '{LevelId}' is already loaded");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Loading level: {LevelId} ({LevelName})");

            // Load background texture
            _backgroundTexture = content.Load<Texture2D>("first_level");

            // Create pixel texture for drawing primitives
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Load platforms from LevelPlatforms
            LoadPlatforms();

            // Create keys
            CreateKeys();

            // Create doors
            CreateDoors();

            // Create hazards (flames and ice shards based on platform types)
            CreateHazards();

            _isLoaded = true;

            System.Diagnostics.Debug.WriteLine($"Level loaded: {_platforms.Count} platforms, " +
                                              $"{_keys.Count} keys, {_doors.Count} doors, " +
                                              $"{_flames.Count} flames, {_iceShards.Count} ice shards");
        }

        /// <summary>
        /// Unloads the level, releasing resources and cleaning up entities
        /// </summary>
        public void Unload()
        {
            if (!_isLoaded)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Unloading level: {LevelId}");

            // Clear all collections
            _platforms.Clear();
            _keys.Clear();
            _doors.Clear();
            _flames.Clear();
            _iceShards.Clear();

            // Dispose of created textures
            _pixelTexture?.Dispose();
            _pixelTexture = null;

            // Note: Don't dispose _backgroundTexture as it's managed by ContentManager

            _isLoaded = false;

            System.Diagnostics.Debug.WriteLine($"Level unloaded: {LevelId}");
        }

        /// <summary>
        /// Gets all game entities in this level
        /// </summary>
        /// <returns>List of all entities</returns>
        public List<IGameObject> GetEntities()
        {
            var entities = new List<IGameObject>();

            // Add all keys
            entities.AddRange(_keys);

            // Add all doors
            entities.AddRange(_doors);

            // Add all flames
            entities.AddRange(_flames);

            // Add all ice shards
            entities.AddRange(_iceShards);

            return entities;
        }

        /// <summary>
        /// Gets all keys in this level
        /// </summary>
        /// <returns>List of keys</returns>
        public List<Key> GetKeys()
        {
            return new List<Key>(_keys);
        }

        /// <summary>
        /// Gets all doors in this level
        /// </summary>
        /// <returns>List of doors</returns>
        public List<Door> GetDoors()
        {
            return new List<Door>(_doors);
        }

        /// <summary>
        /// Gets all flames (fire hazards) in this level
        /// </summary>
        /// <returns>List of flames</returns>
        public List<Flame> GetFlames()
        {
            return new List<Flame>(_flames);
        }

        /// <summary>
        /// Gets all ice shards (ice hazards) in this level
        /// </summary>
        /// <returns>List of ice shards</returns>
        public List<IceShard> GetIceShards()
        {
            return new List<IceShard>(_iceShards);
        }

        /// <summary>
        /// Gets the background texture for this level
        /// </summary>
        /// <returns>Background texture</returns>
        public Texture2D GetBackgroundTexture()
        {
            return _backgroundTexture;
        }

        /// <summary>
        /// Resets the level to its initial state
        /// </summary>
        public void Reset()
        {
            System.Diagnostics.Debug.WriteLine($"Resetting level: {LevelId}");

            // Reset keys
            foreach (var key in _keys)
            {
                key.IsCollected = false;
            }

            // Reset doors
            foreach (var door in _doors)
            {
                door.Reset();
            }

            System.Diagnostics.Debug.WriteLine($"Level reset complete: {LevelId}");
        }

        /// <summary>
        /// Loads platforms from LevelPlatforms static data
        /// </summary>
        private void LoadPlatforms()
        {
            // Get platform data from LevelPlatforms
            _platforms = LevelPlatforms.GetLevel1Platforms();

            System.Diagnostics.Debug.WriteLine($"Loaded {_platforms.Count} platforms");
        }

        /// <summary>
        /// Creates keys at their spawn positions
        /// </summary>
        private void CreateKeys()
        {
            // Create key 1 (left side)
            var key1 = new Key(GameConstants.SpawnPositions.Key1Position);
            key1.SetPixelTexture(_pixelTexture);
            _keys.Add(key1);

            // Create key 2 (right side)
            var key2 = new Key(GameConstants.SpawnPositions.Key2Position);
            key2.SetPixelTexture(_pixelTexture);
            _keys.Add(key2);

            System.Diagnostics.Debug.WriteLine($"Created {_keys.Count} keys");
        }

        /// <summary>
        /// Creates doors at their spawn positions
        /// </summary>
        private void CreateDoors()
        {
            // Create door 1 (top left)
            var door1 = new Door(GameConstants.SpawnPositions.Door1Position);
            door1.SetPixelTexture(_pixelTexture);
            _doors.Add(door1);

            // Create door 2 (top right)
            var door2 = new Door(GameConstants.SpawnPositions.Door2Position);
            door2.SetPixelTexture(_pixelTexture);
            _doors.Add(door2);

            System.Diagnostics.Debug.WriteLine($"Created {_doors.Count} doors");
        }

        /// <summary>
        /// Creates hazards (flames and ice shards) based on platform types
        /// </summary>
        private void CreateHazards()
        {
            // Find all fire hazard platforms
            var firePlatforms = _platforms.Where(p => p.Type == SurfaceType.Fire).ToList();
            System.Diagnostics.Debug.WriteLine($"Fire platforms: {firePlatforms.Count}");

            // Create animated flames for fire hazards
            foreach (var platform in firePlatforms)
            {
                var flame = new Flame(platform.Bounds);
                flame.SetPixelTexture(_pixelTexture);
                _flames.Add(flame);
            }

            System.Diagnostics.Debug.WriteLine($"Created {_flames.Count} animated flames");

            // Find all ice hazard platforms
            var icePlatforms = _platforms.Where(p => p.Type == SurfaceType.IceHazard).ToList();
            System.Diagnostics.Debug.WriteLine($"Ice hazard platforms: {icePlatforms.Count}");

            // Create animated ice shards for ice hazards
            foreach (var platform in icePlatforms)
            {
                var iceShard = new IceShard(platform.Bounds);
                iceShard.SetPixelTexture(_pixelTexture);
                _iceShards.Add(iceShard);
            }

            System.Diagnostics.Debug.WriteLine($"Created {_iceShards.Count} animated ice shards");
        }
    }
}
