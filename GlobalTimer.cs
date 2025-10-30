using Microsoft.Xna.Framework;
using System;

namespace fire_and_ice
{
    public class GlobalTimer
    {
        // Fixed timestep for collision checks
        private const double FIXED_TIMESTEP = 1.0 / 24.0; // ~0.04167 seconds

        // Accumulator for time between updates
        private double _accumulator = 0.0;

        // Track total elapsed time
        private double _totalElapsedTime = 0.0;

        // Counter for number of fixed updates that have occurred
        private int _fixedUpdateCount = 0;

        // Maximum number of fixed updates per frame (prevents spiral of death)
        private const int MAX_FIXED_UPDATES_PER_FRAME = 5;

        /// Gets the fixed timestep duration in seconds
        /// </summary>
        public double FixedTimestep => FIXED_TIMESTEP;

        /// <summary>
        /// Gets the total elapsed time since the timer started
        /// </summary>
        public double TotalElapsedTime => _totalElapsedTime;

        /// Gets the number of fixed updates that have occurred
        public int FixedUpdateCount => _fixedUpdateCount;

        /// Gets how far between fixed updates we currently are (0.0 to 1.0)
        /// Useful for interpolation
        /// </summary>
        public double Alpha => _accumulator / FIXED_TIMESTEP;

        /// Update the timer and determine how many fixed timestep updates should occur
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
        /// Get diagnostic information about the timer state
        /// </summary>
        /// <returns>String with timer diagnostics for debugging</returns>
        public string GetDiagnostics()
        {
            return $"Fixed Timestep: {FIXED_TIMESTEP:F5}s ({1.0 / FIXED_TIMESTEP:F1} FPS)\n" +
                   $"Accumulator: {_accumulator:F5}s\n" +
                   $"Alpha: {Alpha:F3}\n" +
                   $"Fixed Updates: {_fixedUpdateCount}\n" +
                   $"Total Time: {_totalElapsedTime:F2}s";
        }

    }

}