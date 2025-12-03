using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Levels
{
    /// <summary>
    /// Manages game levels including loading, unloading, and transitions.
    /// Provides centralized access to the current level and its entities.
    /// </summary>
    public class LevelManager
    {
        private readonly Dictionary<string, ILevel> _registeredLevels;
        private ILevel _currentLevel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Gets the currently active level
        /// </summary>
        public ILevel CurrentLevel => _currentLevel;

        /// <summary>
        /// Gets whether a level is currently loaded
        /// </summary>
        public bool HasCurrentLevel => _currentLevel != null;

        /// <summary>
        /// Gets the ID of the current level, or null if no level is loaded
        /// </summary>
        public string CurrentLevelId => _currentLevel?.LevelId;

        /// <summary>
        /// Gets the number of registered levels
        /// </summary>
        public int RegisteredLevelCount => _registeredLevels.Count;

        /// <summary>
        /// Event raised when a level is loaded
        /// </summary>
        public event EventHandler<LevelEventArgs> LevelLoaded;

        /// <summary>
        /// Event raised when a level is unloaded
        /// </summary>
        public event EventHandler<LevelEventArgs> LevelUnloaded;

        /// <summary>
        /// Event raised when transitioning between levels
        /// </summary>
        public event EventHandler<LevelTransitionEventArgs> LevelTransitioning;

        /// <summary>
        /// Creates a new level manager
        /// </summary>
        /// <param name="content">Content manager for loading level assets</param>
        /// <param name="graphicsDevice">Graphics device for level initialization</param>
        public LevelManager(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _registeredLevels = new Dictionary<string, ILevel>();
        }

        /// <summary>
        /// Registers a level with the manager
        /// </summary>
        /// <param name="level">The level to register</param>
        public void RegisterLevel(ILevel level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            if (string.IsNullOrEmpty(level.LevelId))
                throw new ArgumentException("Level must have a valid LevelId", nameof(level));

            if (_registeredLevels.ContainsKey(level.LevelId))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Level '{level.LevelId}' is already registered. Replacing with new instance.");
            }

            _registeredLevels[level.LevelId] = level;
            System.Diagnostics.Debug.WriteLine($"Registered level: {level.LevelId} ({level.LevelName})");
        }

        /// <summary>
        /// Unregisters a level from the manager
        /// </summary>
        /// <param name="levelId">The ID of the level to unregister</param>
        /// <returns>True if the level was unregistered, false if it wasn't found</returns>
        public bool UnregisterLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return false;

            // If this is the current level, unload it first
            if (_currentLevel?.LevelId == levelId)
            {
                UnloadCurrentLevel();
            }

            bool removed = _registeredLevels.Remove(levelId);
            if (removed)
            {
                System.Diagnostics.Debug.WriteLine($"Unregistered level: {levelId}");
            }

            return removed;
        }

        /// <summary>
        /// Checks if a level is registered
        /// </summary>
        /// <param name="levelId">The ID of the level to check</param>
        /// <returns>True if the level is registered</returns>
        public bool IsLevelRegistered(string levelId)
        {
            return !string.IsNullOrEmpty(levelId) && _registeredLevels.ContainsKey(levelId);
        }

        /// <summary>
        /// Gets a registered level by ID without loading it
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        /// <returns>The level, or null if not found</returns>
        public ILevel GetLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return null;

            _registeredLevels.TryGetValue(levelId, out var level);
            return level;
        }

        /// <summary>
        /// Gets all registered level IDs
        /// </summary>
        /// <returns>List of level IDs</returns>
        public List<string> GetRegisteredLevelIds()
        {
            return new List<string>(_registeredLevels.Keys);
        }

        /// <summary>
        /// Loads a level by ID, unloading the current level if necessary
        /// </summary>
        /// <param name="levelId">The ID of the level to load</param>
        /// <returns>True if the level was loaded successfully</returns>
        public bool LoadLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                System.Diagnostics.Debug.WriteLine("Cannot load level: levelId is null or empty");
                return false;
            }

            if (!_registeredLevels.TryGetValue(levelId, out var level))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot load level: '{levelId}' is not registered");
                return false;
            }

            // If this is already the current level, just reload it
            if (_currentLevel?.LevelId == levelId)
            {
                System.Diagnostics.Debug.WriteLine($"Reloading current level: {levelId}");
                ReloadCurrentLevel();
                return true;
            }

            // Unload the current level first
            UnloadCurrentLevel();

            // Load the new level
            System.Diagnostics.Debug.WriteLine($"Loading level: {levelId} ({level.LevelName})");
            level.Load(_content, _graphicsDevice);
            _currentLevel = level;

            // Raise event
            OnLevelLoaded(level);

            System.Diagnostics.Debug.WriteLine($"Level loaded successfully: {levelId}");
            return true;
        }

        /// <summary>
        /// Unloads the current level
        /// </summary>
        public void UnloadCurrentLevel()
        {
            if (_currentLevel != null)
            {
                System.Diagnostics.Debug.WriteLine($"Unloading level: {_currentLevel.LevelId}");

                // Raise event before unloading
                OnLevelUnloaded(_currentLevel);

                _currentLevel.Unload();
                _currentLevel = null;

                System.Diagnostics.Debug.WriteLine("Level unloaded successfully");
            }
        }

        /// <summary>
        /// Reloads the current level (unload and load again)
        /// </summary>
        public void ReloadCurrentLevel()
        {
            if (_currentLevel == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot reload: no level is currently loaded");
                return;
            }

            string levelId = _currentLevel.LevelId;
            System.Diagnostics.Debug.WriteLine($"Reloading level: {levelId}");

            _currentLevel.Unload();
            _currentLevel.Load(_content, _graphicsDevice);

            System.Diagnostics.Debug.WriteLine($"Level reloaded successfully: {levelId}");
        }

        /// <summary>
        /// Resets the current level to its initial state
        /// </summary>
        public void ResetCurrentLevel()
        {
            if (_currentLevel == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot reset: no level is currently loaded");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Resetting level: {_currentLevel.LevelId}");
            _currentLevel.Reset();
        }

        /// <summary>
        /// Transitions from the current level to a new level
        /// </summary>
        /// <param name="targetLevelId">The ID of the target level</param>
        /// <returns>True if the transition was successful</returns>
        public bool TransitionToLevel(string targetLevelId)
        {
            if (string.IsNullOrEmpty(targetLevelId))
            {
                System.Diagnostics.Debug.WriteLine("Cannot transition: targetLevelId is null or empty");
                return false;
            }

            if (!_registeredLevels.ContainsKey(targetLevelId))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot transition: level '{targetLevelId}' is not registered");
                return false;
            }

            string fromLevelId = _currentLevel?.LevelId;
            System.Diagnostics.Debug.WriteLine($"Transitioning from '{fromLevelId}' to '{targetLevelId}'");

            // Raise transition event
            OnLevelTransitioning(fromLevelId, targetLevelId);

            // Perform the transition
            bool success = LoadLevel(targetLevelId);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"Transition successful: now in '{targetLevelId}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Transition failed: could not load '{targetLevelId}'");
            }

            return success;
        }

        /// <summary>
        /// Gets all entities from the current level
        /// </summary>
        /// <returns>List of entities, or empty list if no level is loaded</returns>
        public List<IGameObject> GetCurrentLevelEntities()
        {
            return _currentLevel?.GetEntities() ?? new List<IGameObject>();
        }

        /// <summary>
        /// Gets all keys from the current level
        /// </summary>
        /// <returns>List of keys, or empty list if no level is loaded</returns>
        public List<Key> GetCurrentLevelKeys()
        {
            return _currentLevel?.GetKeys() ?? new List<Key>();
        }

        /// <summary>
        /// Gets all doors from the current level
        /// </summary>
        /// <returns>List of doors, or empty list if no level is loaded</returns>
        public List<Door> GetCurrentLevelDoors()
        {
            return _currentLevel?.GetDoors() ?? new List<Door>();
        }

        /// <summary>
        /// Gets all flames from the current level
        /// </summary>
        /// <returns>List of flames, or empty list if no level is loaded</returns>
        public List<Flame> GetCurrentLevelFlames()
        {
            return _currentLevel?.GetFlames() ?? new List<Flame>();
        }

        /// <summary>
        /// Gets all ice shards from the current level
        /// </summary>
        /// <returns>List of ice shards, or empty list if no level is loaded</returns>
        public List<IceShard> GetCurrentLevelIceShards()
        {
            return _currentLevel?.GetIceShards() ?? new List<IceShard>();
        }

        /// <summary>
        /// Gets all platforms from the current level
        /// </summary>
        /// <returns>List of platforms, or empty list if no level is loaded</returns>
        public List<InteractableObject> GetCurrentLevelPlatforms()
        {
            return _currentLevel?.Platforms ?? new List<InteractableObject>();
        }

        /// <summary>
        /// Gets the background texture of the current level
        /// </summary>
        /// <returns>Background texture, or null if no level is loaded</returns>
        public Texture2D GetCurrentLevelBackground()
        {
            return _currentLevel?.GetBackgroundTexture();
        }

        /// <summary>
        /// Raises the LevelLoaded event
        /// </summary>
        private void OnLevelLoaded(ILevel level)
        {
            LevelLoaded?.Invoke(this, new LevelEventArgs { Level = level });
        }

        /// <summary>
        /// Raises the LevelUnloaded event
        /// </summary>
        private void OnLevelUnloaded(ILevel level)
        {
            LevelUnloaded?.Invoke(this, new LevelEventArgs { Level = level });
        }

        /// <summary>
        /// Raises the LevelTransitioning event
        /// </summary>
        private void OnLevelTransitioning(string fromLevelId, string toLevelId)
        {
            LevelTransitioning?.Invoke(this, new LevelTransitionEventArgs
            {
                FromLevelId = fromLevelId,
                ToLevelId = toLevelId
            });
        }

        /// <summary>
        /// Gets diagnostic information about the level manager
        /// </summary>
        /// <returns>Diagnostic string</returns>
        public string GetDiagnostics()
        {
            string currentLevelInfo = _currentLevel != null
                ? $"Current: {_currentLevel.LevelId} ({_currentLevel.LevelName})"
                : "No level loaded";

            return $"LevelManager: {currentLevelInfo}, Registered: {RegisteredLevelCount}";
        }
    }

    /// <summary>
    /// Event arguments for level events
    /// </summary>
    public class LevelEventArgs : EventArgs
    {
        /// <summary>
        /// The level involved in the event
        /// </summary>
        public ILevel Level { get; set; }
    }

    /// <summary>
    /// Event arguments for level transition events
    /// </summary>
    public class LevelTransitionEventArgs : EventArgs
    {
        /// <summary>
        /// The ID of the level being transitioned from (may be null)
        /// </summary>
        public string FromLevelId { get; set; }

        /// <summary>
        /// The ID of the level being transitioned to
        /// </summary>
        public string ToLevelId { get; set; }
    }
}
