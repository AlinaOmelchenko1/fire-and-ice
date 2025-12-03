using System;
using System.Collections.Generic;

namespace fire_and_ice.Levels
{
    /// <summary>
    /// Factory for creating level instances.
    /// Centralizes level creation logic and provides extensibility for adding new levels.
    /// </summary>
    public class LevelFactory
    {
        private readonly Dictionary<string, Func<ILevel>> _levelCreators;

        /// <summary>
        /// Gets the number of registered level types
        /// </summary>
        public int RegisteredLevelCount => _levelCreators.Count;

        /// <summary>
        /// Creates a new level factory with default level registrations
        /// </summary>
        public LevelFactory()
        {
            _levelCreators = new Dictionary<string, Func<ILevel>>();

            // Register default levels
            RegisterDefaultLevels();
        }

        /// <summary>
        /// Registers the default levels that come with the game
        /// </summary>
        private void RegisterDefaultLevels()
        {
            // Register Level 1
            RegisterLevel("level1", () => new Level1());

            System.Diagnostics.Debug.WriteLine($"Registered {_levelCreators.Count} default levels");
        }

        /// <summary>
        /// Registers a level creator function
        /// </summary>
        /// <param name="levelId">The unique ID for the level</param>
        /// <param name="creator">Function that creates a new instance of the level</param>
        public void RegisterLevel(string levelId, Func<ILevel> creator)
        {
            if (string.IsNullOrEmpty(levelId))
                throw new ArgumentException("Level ID cannot be null or empty", nameof(levelId));

            if (creator == null)
                throw new ArgumentNullException(nameof(creator));

            if (_levelCreators.ContainsKey(levelId))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Level '{levelId}' is already registered. Replacing creator.");
            }

            _levelCreators[levelId] = creator;
            System.Diagnostics.Debug.WriteLine($"Registered level creator: {levelId}");
        }

        /// <summary>
        /// Unregisters a level creator
        /// </summary>
        /// <param name="levelId">The ID of the level to unregister</param>
        /// <returns>True if the level was unregistered, false if it wasn't found</returns>
        public bool UnregisterLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return false;

            bool removed = _levelCreators.Remove(levelId);
            if (removed)
            {
                System.Diagnostics.Debug.WriteLine($"Unregistered level creator: {levelId}");
            }

            return removed;
        }

        /// <summary>
        /// Checks if a level creator is registered
        /// </summary>
        /// <param name="levelId">The ID of the level to check</param>
        /// <returns>True if the level creator is registered</returns>
        public bool IsLevelRegistered(string levelId)
        {
            return !string.IsNullOrEmpty(levelId) && _levelCreators.ContainsKey(levelId);
        }

        /// <summary>
        /// Creates a new instance of a level by ID
        /// </summary>
        /// <param name="levelId">The ID of the level to create</param>
        /// <returns>A new level instance, or null if the level ID is not registered</returns>
        public ILevel CreateLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                System.Diagnostics.Debug.WriteLine("Cannot create level: levelId is null or empty");
                return null;
            }

            if (!_levelCreators.TryGetValue(levelId, out var creator))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create level: '{levelId}' is not registered");
                return null;
            }

            try
            {
                var level = creator();
                System.Diagnostics.Debug.WriteLine($"Created level: {levelId} ({level.LevelName})");
                return level;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating level '{levelId}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates all registered levels
        /// </summary>
        /// <returns>List of all level instances</returns>
        public List<ILevel> CreateAllLevels()
        {
            var levels = new List<ILevel>();

            foreach (var levelId in _levelCreators.Keys)
            {
                var level = CreateLevel(levelId);
                if (level != null)
                {
                    levels.Add(level);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Created {levels.Count} levels");
            return levels;
        }

        /// <summary>
        /// Gets all registered level IDs
        /// </summary>
        /// <returns>List of level IDs</returns>
        public List<string> GetRegisteredLevelIds()
        {
            return new List<string>(_levelCreators.Keys);
        }

        /// <summary>
        /// Creates a level by numeric index (1-based)
        /// </summary>
        /// <param name="levelNumber">The level number (1 for level1, 2 for level2, etc.)</param>
        /// <returns>A new level instance, or null if not found</returns>
        public ILevel CreateLevelByNumber(int levelNumber)
        {
            if (levelNumber < 1)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create level: level number must be positive (got {levelNumber})");
                return null;
            }

            string levelId = $"level{levelNumber}";
            return CreateLevel(levelId);
        }

        /// <summary>
        /// Gets diagnostic information about the factory
        /// </summary>
        /// <returns>Diagnostic string</returns>
        public string GetDiagnostics()
        {
            return $"LevelFactory: {RegisteredLevelCount} level types registered";
        }

        /// <summary>
        /// Validates all registered level creators by attempting to create instances
        /// </summary>
        /// <returns>True if all levels can be created successfully</returns>
        public bool ValidateAllLevels()
        {
            System.Diagnostics.Debug.WriteLine("Validating all registered levels...");
            bool allValid = true;

            foreach (var levelId in _levelCreators.Keys)
            {
                try
                {
                    var level = CreateLevel(levelId);
                    if (level == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  × Level '{levelId}' returned null");
                        allValid = false;
                    }
                    else if (string.IsNullOrEmpty(level.LevelId))
                    {
                        System.Diagnostics.Debug.WriteLine($"  × Level '{levelId}' has invalid LevelId");
                        allValid = false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✓ Level '{levelId}' is valid ({level.LevelName})");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  × Level '{levelId}' threw exception: {ex.Message}");
                    allValid = false;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Validation complete: {(allValid ? "All valid" : "Some levels invalid")}");
            return allValid;
        }

        /// <summary>
        /// Creates a default factory with all standard levels registered
        /// </summary>
        /// <returns>A new level factory instance</returns>
        public static LevelFactory CreateDefault()
        {
            return new LevelFactory();
        }
    }
}
