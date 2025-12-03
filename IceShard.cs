using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using fire_and_ice.Core;

namespace fire_and_ice
{
    /// <summary>
    /// Represents an animated ice shard effect for ice hazards.
    /// Features crystalline appearance with glowing and shimmering effects.
    /// </summary>
    public class IceShard : AnimatedObject
    {
        private Texture2D _pixelTexture;

        // Ice animation parameters
        private const float GLOW_SPEED = 2f;
        private const float SHIMMER_SPEED = 5f;

        /// <summary>
        /// Creates a new ice shard effect at the specified bounds
        /// </summary>
        /// <param name="bounds">The area where the ice shard should be displayed</param>
        public IceShard(Rectangle bounds) : base(bounds)
        {
            // Base class already initializes AnimationTimer with random phase
        }

        /// <summary>
        /// Sets the pixel texture used for rendering the ice shard
        /// </summary>
        /// <param name="pixelTexture">A 1x1 white pixel texture for drawing shapes</param>
        public void SetPixelTexture(Texture2D pixelTexture)
        {
            _pixelTexture = pixelTexture;
        }

        /// <summary>
        /// Updates the ice shard animation timers and intensity.
        /// Creates a glowing/shimmering effect by combining sine waves.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void UpdateAnimation(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update primary and secondary timers
            AnimationTimer += deltaTime * GLOW_SPEED;
            SecondaryTimer += deltaTime * SHIMMER_SPEED;

            // Create subtle glowing/shimmering effect
            float glow = CalculateSineWave(AnimationTimer);
            float shimmer = CalculateSineWave(SecondaryTimer) * GameConstants.IceShard.ShimmerIntensity;

            // Calculate intensity using base class helper
            CurrentIntensity = CalculateIntensity(
                GameConstants.IceShard.BaseIntensity,
                GameConstants.IceShard.GlowIntensity,
                glow
            ) + shimmer;

            // Clamp to valid range
            CurrentIntensity = Clamp(CurrentIntensity, GameConstants.IceShard.MinIntensity, GameConstants.IceShard.MaxIntensity);
        }

        /// <summary>
        /// Draws the ice shard effect to the screen.
        /// Renders multiple crystalline layers with glowing and shimmering effects.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_pixelTexture == null)
                return;

            int centerX = Bounds.X + Bounds.Width / 2;
            int bottomY = Bounds.Y + Bounds.Height;

            // Draw ice shard as a crystalline spike pointing upward
            // Base layer - outer dark cyan
            DrawShardLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.IceShard.OuterLayerWidth),
                (int)(Bounds.Height * GameConstants.IceShard.OuterLayerHeight),
                new Color(0, 100, 150), CurrentIntensity * GameConstants.Opacity.High);

            // Middle layer - cyan
            DrawShardLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.IceShard.MiddleLayerWidth),
                (int)(Bounds.Height * GameConstants.IceShard.MiddleLayerHeight),
                Color.Cyan, CurrentIntensity * 0.85f);

            // Inner layer - light cyan
            DrawShardLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.IceShard.InnerLayerWidth),
                (int)(Bounds.Height * GameConstants.IceShard.InnerLayerHeight),
                Color.LightCyan, CurrentIntensity * 0.95f);

            // Core - bright white
            DrawShardLayer(spriteBatch, centerX, bottomY,
                (int)(Bounds.Width * GameConstants.IceShard.CoreLayerWidth),
                (int)(Bounds.Height * GameConstants.IceShard.CoreLayerHeight),
                Color.White, CurrentIntensity);

            // Add sparkle effect at tip
            float sparkle = CalculateSineWave(SecondaryTimer, 2f) * 0.5f + 0.5f;
            if (sparkle > GameConstants.IceShard.SparkleThreshold)
            {
                Rectangle sparkleTip = new Rectangle(
                    centerX - GameConstants.IceShard.SparkleSize / 2,
                    Bounds.Y + (int)(Bounds.Height * GameConstants.IceShard.SparkleTipOffset),
                    GameConstants.IceShard.SparkleSize,
                    GameConstants.IceShard.SparkleSize
                );
                spriteBatch.Draw(_pixelTexture, sparkleTip, Color.White * sparkle);
            }

            // Add shimmer lines on sides
            DrawShimmerLine(spriteBatch, centerX - (int)(Bounds.Width * GameConstants.IceShard.ShimmerSideOffset), Bounds.Y, Bounds.Height);
            DrawShimmerLine(spriteBatch, centerX + (int)(Bounds.Width * GameConstants.IceShard.ShimmerSideOffset), Bounds.Y, Bounds.Height);
        }

        /// <summary>
        /// Draws a single layer of the ice shard with crystalline taper
        /// </summary>
        private void DrawShardLayer(SpriteBatch spriteBatch,
            int centerX, int bottomY, int width, int height, Color color, float alpha)
        {
            // Draw crystalline spike - wide at bottom, narrow at top
            int steps = height / GameConstants.IceShard.SegmentHeight;
            for (int y = 0; y < steps; y++)
            {
                float progress = (float)y / steps;

                // Create spike shape - linear taper from bottom to top
                float widthMultiplier = 1.0f - progress;

                int currentWidth = (int)(width * widthMultiplier);
                int currentY = bottomY - (y * GameConstants.IceShard.SegmentHeight);

                if (currentWidth > 0)
                {
                    Rectangle segment = new Rectangle(
                        centerX - currentWidth / 2,
                        currentY,
                        currentWidth,
                        GameConstants.IceShard.SegmentHeight
                    );

                    // Consistent opacity throughout
                    float segmentAlpha = alpha * GameConstants.Opacity.AlmostOpaque;

                    spriteBatch.Draw(_pixelTexture, segment, color * segmentAlpha);
                }
            }

            // Add sharp pointed tip
            int tipHeight = (int)(height * GameConstants.IceShard.TipHeightMultiplier);
            for (int y = 0; y < tipHeight; y++)
            {
                float tipProgress = (float)y / tipHeight;
                int tipY = Bounds.Y + (int)(height * GameConstants.IceShard.TipHeightMultiplier) + y;
                int tipWidth = Math.Max(1, (int)(width * GameConstants.IceShard.TipWidthMultiplier * (1.0f - tipProgress)));

                Rectangle tip = new Rectangle(centerX - tipWidth / 2, tipY, tipWidth, 1);
                spriteBatch.Draw(_pixelTexture, tip, color * (alpha * GameConstants.Opacity.VeryHigh));
            }
        }

        /// <summary>
        /// Draws a shimmering line on the side of the ice shard
        /// </summary>
        private void DrawShimmerLine(SpriteBatch spriteBatch, int x, int startY, int height)
        {
            float shimmer = CalculateSineWave(SecondaryTimer + x * 0.1f) * 0.3f + 0.4f;
            int lineHeight = (int)(height * GameConstants.IceShard.ShimmerLineHeight);

            Rectangle line = new Rectangle(x, startY + height - lineHeight, 1, lineHeight);
            spriteBatch.Draw(_pixelTexture, line, Color.White * (shimmer * CurrentIntensity * GameConstants.Opacity.MediumHigh));
        }
    }
}
