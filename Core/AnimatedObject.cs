using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Core
{
    /// <summary>
    /// Base class for animated game objects that have visual effects
    /// Provides common animation timing and intensity management
    /// </summary>
    public abstract class AnimatedObject : IGameObject
    {
        /// <summary>
        /// The bounding rectangle for this animated object
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Primary animation timer for effects
        /// </summary>
        protected float AnimationTimer { get; set; }

        /// <summary>
        /// Secondary animation timer for additional effects
        /// </summary>
        protected float SecondaryTimer { get; set; }

        /// <summary>
        /// Current intensity/brightness of the animation (typically 0.0 to 1.0)
        /// </summary>
        protected float CurrentIntensity { get; set; }

        /// <summary>
        /// Random number generator for animation variation
        /// </summary>
        protected Random Random { get; private set; }

        /// <summary>
        /// Creates a new animated object with the specified bounds
        /// </summary>
        /// <param name="bounds">The bounding rectangle for this object</param>
        protected AnimatedObject(Rectangle bounds)
        {
            Bounds = bounds;
            Random = new Random();
            CurrentIntensity = 1.0f;
            AnimationTimer = 0f;
            SecondaryTimer = 0f;

            // Randomize initial animation phase to prevent synchronized animations
            AnimationTimer = (float)(Random.NextDouble() * Math.PI * 2);
        }

        /// <summary>
        /// Updates the animated object's state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public virtual void Update(GameTime gameTime)
        {
            UpdateAnimation(gameTime);
        }

        /// <summary>
        /// Updates animation timers and intensity.
        /// Override this to implement specific animation behavior.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected abstract void UpdateAnimation(GameTime gameTime);

        /// <summary>
        /// Draws the animated object to the screen
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Helper method to calculate a sine wave value for smooth animation
        /// </summary>
        /// <param name="timer">The current timer value</param>
        /// <param name="frequency">How fast the wave oscillates</param>
        /// <returns>A value between -1 and 1</returns>
        protected float CalculateSineWave(float timer, float frequency = 1.0f)
        {
            return (float)Math.Sin(timer * frequency);
        }

        /// <summary>
        /// Helper method to calculate an intensity value that oscillates smoothly
        /// </summary>
        /// <param name="baseIntensity">The base intensity value</param>
        /// <param name="variation">How much the intensity varies (0.0 to 1.0)</param>
        /// <param name="waveValue">A sine wave value (-1 to 1)</param>
        /// <returns>The calculated intensity</returns>
        protected float CalculateIntensity(float baseIntensity, float variation, float waveValue)
        {
            return baseIntensity + (waveValue * variation);
        }

        /// <summary>
        /// Helper method to clamp a value between min and max
        /// </summary>
        protected float Clamp(float value, float min, float max)
        {
            return MathHelper.Clamp(value, min, max);
        }
    }
}
