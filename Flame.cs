using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace fire_and_ice
{
    /// <summary>
    /// Represents an animated flame effect for fire hazards
    /// </summary>
    public class Flame
    {
        public Rectangle Bounds { get; set; }
        private float _animationTimer = 0f;
        private float _flickerTimer = 0f;
        private float _currentIntensity = 1f;
        private Random _random = new Random();

        // Flame animation parameters
        private const float FLICKER_SPEED = 8f;
        private const float PULSE_SPEED = 3f;

        public Flame(Rectangle bounds)
        {
            Bounds = bounds;
            _animationTimer = (float)_random.NextDouble() * MathF.PI * 2; // Random start phase
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _animationTimer += deltaTime * PULSE_SPEED;
            _flickerTimer += deltaTime * FLICKER_SPEED;

            // Create flickering effect by combining sine waves
            float pulse = (float)Math.Sin(_animationTimer);
            float flicker = (float)Math.Sin(_flickerTimer) * 0.3f;
            float randomFlicker = ((float)_random.NextDouble() - 0.5f) * 0.2f;

            _currentIntensity = 0.7f + (pulse * 0.2f) + flicker + randomFlicker;
            _currentIntensity = MathHelper.Clamp(_currentIntensity, 0.5f, 1.0f);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            int centerX = Bounds.X + Bounds.Width / 2;
            int bottomY = Bounds.Y + Bounds.Height;

            // Calculate animation offsets
            float wave1 = (float)Math.Sin(_animationTimer * 2) * 2;
            float wave2 = (float)Math.Sin(_animationTimer * 3 + 1) * 1.5f;
            float wave3 = (float)Math.Sin(_flickerTimer) * 1;

            // Draw outer red/orange base
            DrawFlameLayer(spriteBatch, pixelTexture, centerX, bottomY,
                Bounds.Width, Bounds.Height,
                Color.Red, _currentIntensity * 0.7f, wave1);

            // Draw middle orange layer
            DrawFlameLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.75f), (int)(Bounds.Height * 0.85f),
                Color.OrangeRed, _currentIntensity * 0.85f, wave2);

            // Draw inner orange-yellow layer
            DrawFlameLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.6f), (int)(Bounds.Height * 0.7f),
                Color.Orange, _currentIntensity * 0.9f, wave3);

            // Draw bright yellow core
            DrawFlameLayer(spriteBatch, pixelTexture, centerX, bottomY,
                (int)(Bounds.Width * 0.4f), (int)(Bounds.Height * 0.55f),
                Color.Yellow, _currentIntensity, wave1 * 0.5f);

            // Draw white hot center at bottom
            int coreSize = (int)(Bounds.Width * 0.25f);
            Rectangle whiteCore = new Rectangle(
                centerX - coreSize / 2,
                bottomY - coreSize,
                coreSize,
                coreSize
            );
            spriteBatch.Draw(pixelTexture, whiteCore,
                Color.White * (_currentIntensity * 0.8f));
        }

        private void DrawFlameLayer(SpriteBatch spriteBatch, Texture2D pixelTexture,
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
                    3
                );

                // Fade out towards the top
                float segmentAlpha = alpha * (1.0f - progress * 0.3f);

                spriteBatch.Draw(pixelTexture, segment, color * segmentAlpha);
            }

            // Add flame tips (pointed top)
            int tipCount = 2;
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
                        spriteBatch.Draw(pixelTexture, tip, color * (alpha * (1.0f - tipProgress * 0.5f)));
                    }
                }
            }
        }
    }
}
