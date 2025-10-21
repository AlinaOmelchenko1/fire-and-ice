using Microsoft.Xna.Framework;
using System;

namespace fire_and_ice
{
    /// <summary>
    /// Global timer that manages fixed timestep updates for physics/collision
    /// Runs collision checks at a fixed rate of 24 times per second
    /// </summary>
    public class GlobalTimer
    {
        // Fixed timestep for collision checks (24 times per second = 1/24 seconds per update)
        private const double FIXED_TIMESTEP = 1.0 / 24.0; // ~0.04167 seconds

        // Accumulator for time between updates
        private double _accumulator = 0.0;

        // Track total elapsed time
        private double _totalElapsedTime = 0.0;

        // Counter for number of fixed updates that have occurred
        private int _fixedUpdateCount = 0;

        // Maximum number of fixed updates per frame (prevents spiral of death)
        private const int MAX_FIXED_UPDATES_PER_FRAME = 5;

        /// <summary>
        /// Gets the fixed timestep duration in seconds
        /// </summary>
        public double FixedTimestep => FIXED_TIMESTEP;

        /// <summary>
        /// Gets the total elapsed time since the timer started
        /// </summary>
        public double TotalElapsedTime => _totalElapsedTime;

        /// <summary>
        /// Gets the number of fixed updates that have occurred
        /// </summary>
        public int FixedUpdateCount => _fixedUpdateCount;

        /// <summary>
        /// Gets how far between fixed updates we currently are (0.0 to 1.0)
        /// Useful for interpolation
        /// </summary>
        public double Alpha => _accumulator / FIXED_TIMESTEP;

        /// <summary>
        /// Update the timer and determine how many fixed timestep updates should occur
        /// </summary>
        /// <param name="gameTime">The game time from MonoGame's Update method</param>
        /// <param name="onFixedUpdate">Action to call for each fixed timestep update</param>
        public void Update(GameTime gameTime, Action<float> onFixedUpdate)
        {
            // Get the elapsed time since last frame
            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            // Add to total elapsed time
            _totalElapsedTime += deltaTime;

            // Add frame time to accumulator
            _accumulator += deltaTime;

            // Perform fixed updates while we have enough accumulated time
            int updatesThisFrame = 0;
            while (_accumulator >= FIXED_TIMESTEP && updatesThisFrame < MAX_FIXED_UPDATES_PER_FRAME)
            {
                // Call the fixed update callback with the fixed timestep
                onFixedUpdate?.Invoke((float)FIXED_TIMESTEP);

                // Subtract the fixed timestep from accumulator
                _accumulator -= FIXED_TIMESTEP;

                // Increment counters
                _fixedUpdateCount++;
                updatesThisFrame++;
            }

            // If we hit the max updates limit, reset accumulator to prevent spiral of death
            if (updatesThisFrame >= MAX_FIXED_UPDATES_PER_FRAME)
            {
                _accumulator = 0.0;
                System.Diagnostics.Debug.WriteLine("Warning: GlobalTimer hit max fixed updates per frame!");
            }
        }

        /// <summary>
        /// Simpler update method that returns true when a fixed update should occur
        /// Call your collision/physics logic when this returns true
        /// </summary>
        /// <param name="gameTime">The game time from MonoGame's Update method</param>
        /// <returns>True if a fixed update should occur this frame</returns>
        public bool ShouldFixedUpdate(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            _totalElapsedTime += deltaTime;
            _accumulator += deltaTime;

            if (_accumulator >= FIXED_TIMESTEP)
            {
                _accumulator -= FIXED_TIMESTEP;
                _fixedUpdateCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset the timer to initial state
        /// </summary>
        public void Reset()
        {
            _accumulator = 0.0;
            _totalElapsedTime = 0.0;
            _fixedUpdateCount = 0;
        }

        /// <summary>
        /// Get diagnostic information about the timer
        /// </summary>
        public string GetDiagnostics()
        {
            return $"Fixed Updates: {_fixedUpdateCount} | " +
                   $"Total Time: {_totalElapsedTime:F2}s | " +
                   $"Accumulator: {_accumulator:F4}s | " +
                   $"Alpha: {Alpha:F2}";
        }
    }

    /// <summary>
    /// Extension class to demonstrate usage patterns
    /// </summary>
    public static class GlobalTimerExample
    {
        /// <summary>
        /// Example usage pattern 1: Using callback
        /// </summary>
        public static void ExampleUsageWithCallback()
        {
            /*
            // In your Game1 class:
            private GlobalTimer _collisionTimer = new GlobalTimer();
            
            protected override void Update(GameTime gameTime)
            {
                // Update with callback - collision logic runs at fixed 24 FPS
                _collisionTimer.Update(gameTime, (fixedDeltaTime) =>
                {
                    // This code runs exactly 24 times per second
                    CheckPlatformCollisions();
                    UpdatePhysics(fixedDeltaTime);
                });
                
                // Other game logic that runs every frame
                UpdateAnimation(gameTime);
                UpdateInput(gameTime);
            }
            */
        }

        /// <summary>
        /// Example usage pattern 2: Using boolean check
        /// </summary>
        public static void ExampleUsageWithBoolean()
        {
            /*
            // In your Game1 class:
            private GlobalTimer _collisionTimer = new GlobalTimer();
            
            protected override void Update(GameTime gameTime)
            {
                // Check if we should run collision this frame
                if (_collisionTimer.ShouldFixedUpdate(gameTime))
                {
                    // This code runs exactly 24 times per second
                    CheckPlatformCollisions();
                    ApplyGravity();
                }
                
                // Other game logic that runs every frame
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                UpdateAnimation(deltaTime);
                UpdateInput(deltaTime);
            }
            */
        }

        /// <summary>
        /// Example usage pattern 3: Multiple updates per frame
        /// </summary>
        public static void ExampleUsageMultipleUpdates()
        {
            /*
            // In your Game1 class:
            private GlobalTimer _collisionTimer = new GlobalTimer();
            
            protected override void Update(GameTime gameTime)
            {
                // This handles multiple fixed updates per frame automatically
                // If game runs slow, it will catch up with multiple collision checks
                _collisionTimer.Update(gameTime, (fixedDeltaTime) =>
                {
                    // Apply physics with fixed timestep
                    _playerVelocity.Y += _gravity * fixedDeltaTime;
                    _playerPosition += _playerVelocity * fixedDeltaTime;
                    CheckPlatformCollisions();
                });
                
                // Visual updates can still run every frame
                UpdateAnimations(gameTime);
            }
            */
        }
    }
}