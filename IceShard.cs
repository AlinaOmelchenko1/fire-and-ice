using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace fire_and_ice
{
    /// <summary>
    /// Represents an animated ice shard effect for ice hazards
    /// </summary>
    public class IceShard
    {
        public Rectangle Bounds { get; set; }
        private float _animationTimer = 0f;
        private float _glowTimer = 0f;
        private float _currentIntensity = 1f;
        private Random _random = new Random();

        // Ice animation parameters
        private const float GLOW_SPEED = 2f;
        private const float SHIMMER_SPEED = 5f;

        public IceShard(Rectangle bounds)
        {
            Bounds = bounds;
            _animationTimer = (float)_random.NextDouble() * MathF.PI * 2; // Random start phase
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _animationTimer += deltaTime * GLOW_SPEED;
            _glowTimer += deltaTime * SHIMMER_SPEED;

            // Create subtle glowing/shimmering effect
            float glow = (float)Math.Sin(_animationTimer);
            float shimmer = (float)Math.Sin(_glowTimer) * 0.2f;

            _currentIntensity = 0.8f + (glow * 0.15f) + shimmer;
            _currentIntensity = MathHelper.Clamp(_currentIntensity, 0.7f, 1.0f);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            int centerX = Bounds.X + Bounds.Width / 2;
            int bottomY = Bounds.Y + Bounds.Height;

            // Draw ice shard as a crystalline spike pointing upward
            // Base layer - outer dark cyan
            DrawShardLayer(spriteBatch, pixelTexture, centerX, bottomY,
                Bounds.Width, Bounds.Height,
                new Color(0, 100, 150), _currentIntensity * 0.7f);

            // Middle layer - cyan
            DrawShardLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.7f), (int)(Bounds.Height * 0.9f),
                Color.Cyan, _currentIntensity * 0.85f);

            // Inner layer - light cyan
            DrawShardLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.5f), (int)(Bounds.Height * 0.75f),
                Color.LightCyan, _currentIntensity * 0.95f);

            // Core - bright white
            DrawShardLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.3f), (int)(Bounds.Height * 0.6f),
                Color.White, _currentIntensity);

            // Add sparkle effect at tip
            float sparkle = (float)Math.Sin(_glowTimer * 2) * 0.5f + 0.5f;
            if (sparkle > 0.7f)
            {
                int sparkleSize = 4;
                Rectangle sparkleTip = new Rectangle(
                    centerX - sparkleSize / 2,
                    Bounds.Y + (int)(Bounds.Height * 0.2f),
                    sparkleSize,
                    sparkleSize
                );
                spriteBatch.Draw(pixelTexture, sparkleTip, Color.White * sparkle);
            }

            // Add shimmer lines on sides
            DrawShimmerLine(spriteBatch, pixelTexture, centerX - (int)(Bounds.Width * 0.3f), Bounds.Y, Bounds.Height);
            DrawShimmerLine(spriteBatch, pixelTexture, centerX + (int)(Bounds.Width * 0.3f), Bounds.Y, Bounds.Height);
        }

        private void DrawShardLayer(SpriteBatch spriteBatch, Texture2D pixelTexture,
            int centerX, int bottomY, int width, int height, Color color, float alpha)
        {
            // Draw crystalline spike - wide at bottom, narrow at top
            int steps = height / 2;
            for (int y = 0; y < steps; y++)
            {
                float progress = (float)y / steps;

                // Create spike shape - linear taper from bottom to top
                float widthMultiplier = 1.0f - progress;

                int currentWidth = (int)(width * widthMultiplier);
                int currentY = bottomY - (y * 2);

                if (currentWidth > 0)
                {
                    Rectangle segment = new Rectangle(
                        centerX - currentWidth / 2,
                        currentY,
                        currentWidth,
                        2
                    );

                    // Consistent opacity throughout
                    float segmentAlpha = alpha * 0.95f;

                    spriteBatch.Draw(pixelTexture, segment, color * segmentAlpha);
                }
            }

            // Add sharp pointed tip
            int tipHeight = (int)(height * 0.15f);
            for (int y = 0; y < tipHeight; y++)
            {
                float tipProgress = (float)y / tipHeight;
                int tipY = Bounds.Y + (int)(height * 0.15f) + y;
                int tipWidth = Math.Max(1, (int)(width * 0.2f * (1.0f - tipProgress)));

                Rectangle tip = new Rectangle(centerX - tipWidth / 2, tipY, tipWidth, 1);
                spriteBatch.Draw(pixelTexture, tip, color * (alpha * 0.9f));
            }
        }

        private void DrawShimmerLine(SpriteBatch spriteBatch, Texture2D pixelTexture,
            int x, int startY, int height)
        {
            float shimmer = (float)Math.Sin(_glowTimer + x * 0.1f) * 0.3f + 0.4f;
            int lineHeight = (int)(height * 0.6f);

            Rectangle line = new Rectangle(x, startY + height - lineHeight, 1, lineHeight);
            spriteBatch.Draw(pixelTexture, line, Color.White * (shimmer * _currentIntensity * 0.6f));
        }
    }
}
