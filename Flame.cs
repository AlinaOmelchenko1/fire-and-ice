using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using fire_and_ice.Core;

namespace fire_and_ice
{
    /// <summary>
    /// Represents an animated flame effect for fire hazards.
    /// Features multi-layered rendering with flickering animation and wave motion.
    /// </summary>
    public class Flame : AnimatedObject
    {
        private Texture2D _pixelTexture;

        // Flame animation parameters
        private const float FLICKER_SPEED = 8f;
        private const float PULSE_SPEED = 3f;

        /// <summary>
        /// Creates a new flame effect at the specified bounds
        /// </summary>
        /// <param name="bounds">The area where the flame should be displayed</param>
        public Flame(Rectangle bounds) : base(bounds)
        {
            // Base class already initializes AnimationTimer with random phase
        }

        /// <summary>
        /// Sets the pixel texture used for rendering the flame
        /// </summary>
        /// <param name="pixelTexture">A 1x1 white pixel texture for drawing shapes</param>
        public void SetPixelTexture(Texture2D pixelTexture)
        {
            _pixelTexture = pixelTexture;
        }

        /// <summary>
        /// Updates the flame animation timers and intensity.
        /// Creates a flickering effect by combining multiple sine waves.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void UpdateAnimation(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update primary and secondary timers
            AnimationTimer += deltaTime * PULSE_SPEED;
            SecondaryTimer += deltaTime * FLICKER_SPEED;

            // Create flickering effect by combining sine waves
            float pulse = CalculateSineWave(AnimationTimer);
            float flicker = CalculateSineWave(SecondaryTimer) * GameConstants.Flame.FlickerIntensity;
            float randomFlicker = ((float)Random.NextDouble() - 0.5f) * GameConstants.Flame.RandomFlickerIntensity;

            // Calculate intensity using base class helper
            CurrentIntensity = CalculateIntensity(
                GameConstants.Flame.BaseIntensity,
                GameConstants.Flame.PulseIntensity,
                pulse
            ) + flicker + randomFlicker;

            // Clamp to valid range
            CurrentIntensity = Clamp(CurrentIntensity, GameConstants.Flame.MinIntensity, GameConstants.Flame.MaxIntensity);
        }

        /// <summary>
        /// Draws the flame effect to the screen.
        /// Renders multiple colored layers with wave motion for realistic flame appearance.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_pixelTexture == null)
                return;

            int centerX = Bounds.X + Bounds.Width / 2;
            int bottomY = Bounds.Y + Bounds.Height;

            // Calculate animation offsets using sine waves
            float wave1 = CalculateSineWave(AnimationTimer, 2f) * 2;
            float wave2 = CalculateSineWave(AnimationTimer * 3 + 1) * 1.5f;
            float wave3 = CalculateSineWave(SecondaryTimer) * 1;

            // Draw outer red/orange base
            DrawFlameLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.Flame.OuterLayerWidth),
                (int)(Bounds.Height * GameConstants.Flame.OuterLayerHeight),
                Color.Red, CurrentIntensity * GameConstants.Opacity.High, wave1);

            // Draw middle orange layer
            DrawFlameLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.Flame.MiddleLayerWidth),
                (int)(Bounds.Height * GameConstants.Flame.MiddleLayerHeight),
                Color.OrangeRed, CurrentIntensity * 0.85f, wave2);

            // Draw inner orange-yellow layer
            DrawFlameLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.Flame.InnerLayerWidth),
                (int)(Bounds.Height * GameConstants.Flame.InnerLayerHeight),
                Color.Orange, CurrentIntensity * 0.9f, wave3);

            // Draw bright yellow core
            DrawFlameLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.Flame.CoreLayerWidth),
                (int)(Bounds.Height * GameConstants.Flame.CoreLayerHeight),
                Color.Yellow, CurrentIntensity, wave1 * 0.5f);

            // Draw white hot center at bottom
            int coreSize = (int)(Bounds.Width * GameConstants.Flame.WhiteCoreSize);
            Rectangle whiteCore = new Rectangle(
                centerX - coreSize / 2,
                bottomY - coreSize,
                coreSize,
                coreSize
            );
            spriteBatch.Draw(_pixelTexture, whiteCore,
                Color.White * (CurrentIntensity * GameConstants.Opacity.VeryHigh));
        }

        /// <summary>
        /// Draws a single layer of the flame with wave motion
        /// </summary>
        private void DrawFlameLayer(SpriteBatch spriteBatch,
            int centerX, int bottomY, int width, int height, Color color, float alpha, float wave)
        {
            // Draw flame body with curved shape
            int steps = height / 2;
            for (int y = 0; y < steps; y++)
            {
                float progress = (float)y / steps;

                // Create flame shape - wider at bottom, narrower and wavy at top
                float widthMultiplier = 1.0f - (progress * progress * 0.7f);

                // Add wave motion
                float xOffset = wave * progress * 2;

                int currentWidth = (int)(width * widthMultiplier);
                int currentY = bottomY - (y * 2);

                Rectangle segment = new Rectangle(
                    (int)(centerX - currentWidth / 2 + xOffset),
                    currentY,
                    currentWidth,
                    GameConstants.Flame.SegmentHeight
                );

                // Fade out towards the top
                float segmentAlpha = alpha * (1.0f - progress * GameConstants.Opacity.VeryLight);

                spriteBatch.Draw(_pixelTexture, segment, color * segmentAlpha);
            }

            // Add flame tips (pointed top)
            int tipCount = GameConstants.Flame.TipCount;
            for (int i = 0; i < tipCount; i++)
            {
                float tipAngle = (i - 0.5f) * 0.4f;
                int tipHeight = (int)(height * 0.3f);
                int tipWidth = (int)(width * 0.15f);

                for (int y = 0; y < tipHeight; y++)
                {
                    float tipProgress = (float)y / tipHeight;
                    int tipX = (int)(centerX + tipAngle * width + wave * tipProgress);
                    int tipY = bottomY - height + y;
                    int tipW = (int)(tipWidth * (1.0f - tipProgress));

                    if (tipW > 0)
                    {
                        Rectangle tip = new Rectangle(tipX - tipW / 2, tipY, tipW, 2);
                        spriteBatch.Draw(_pixelTexture, tip, color * (alpha * (1.0f - tipProgress * GameConstants.Opacity.Medium)));
                    }
                }
            }
        }
    }
}
