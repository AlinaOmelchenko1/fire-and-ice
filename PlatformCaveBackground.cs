using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class PlatformCaveBackground
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _pixel;
        private int _screenWidth;
        private int _screenHeight;

        // Platform collision hitboxes
        private List<Rectangle> _platformHitboxes;

        public List<Rectangle> PlatformHitboxes => _platformHitboxes;

        public PlatformCaveBackground(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            // Create a single pixel texture for drawing rectangles
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Initialize platform hitboxes
            CreatePlatforms();
        }

        private void CreatePlatforms()
        {
            _platformHitboxes = new List<Rectangle>();

            // Bottom floor (full width)
            _platformHitboxes.Add(new Rectangle(0, _screenHeight - 60, _screenWidth, 60));

            // Second level platforms
            int level2Y = _screenHeight - 180;
            _platformHitboxes.Add(new Rectangle(0, level2Y, 200, 20));  // Left platform
            _platformHitboxes.Add(new Rectangle(300, level2Y, 180, 20)); // Middle platform
            _platformHitboxes.Add(new Rectangle(550, level2Y, 250, 20)); // Right platform

            // Third level platforms
            int level3Y = _screenHeight - 300;
            _platformHitboxes.Add(new Rectangle(80, level3Y, 150, 20));   // Left-center platform
            _platformHitboxes.Add(new Rectangle(280, level3Y, 120, 20));  // Center platform
            _platformHitboxes.Add(new Rectangle(450, level3Y, 200, 20));  // Right platform
            _platformHitboxes.Add(new Rectangle(700, level3Y, 100, 20));  // Far right platform

            // Fourth level platforms
            int level4Y = _screenHeight - 420;
            _platformHitboxes.Add(new Rectangle(50, level4Y, 120, 20));   // Left platform
            _platformHitboxes.Add(new Rectangle(220, level4Y, 160, 20));  // Center-left platform
            _platformHitboxes.Add(new Rectangle(430, level4Y, 140, 20));  // Center-right platform
            _platformHitboxes.Add(new Rectangle(620, level4Y, 180, 20));  // Right platform

            // Top level platforms
            int level5Y = _screenHeight - 540;
            _platformHitboxes.Add(new Rectangle(100, level5Y, 180, 20));  // Left platform
            _platformHitboxes.Add(new Rectangle(350, level5Y, 200, 20));  // Center platform
            _platformHitboxes.Add(new Rectangle(600, level5Y, 150, 20));  // Right platform
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Dark cave atmosphere
            DrawCaveBackground(spriteBatch);

            // Draw stalactites (ceiling decorations)
            DrawStalactites(spriteBatch);

            // Draw cave walls
            DrawCaveWalls(spriteBatch);

            // Draw all platforms
            DrawPlatforms(spriteBatch);

            // Add atmospheric effects
            DrawAtmosphericEffects(spriteBatch);
        }

        public void DrawPlatformHitboxes(SpriteBatch spriteBatch)
        {
            foreach (Rectangle platform in _platformHitboxes)
            {
                spriteBatch.Draw(_pixel, platform, Color.Green * 0.3f);
            }
        }

        private void DrawCaveBackground(SpriteBatch spriteBatch)
        {
            // Dark gradient background (very dark at top, slightly lighter at bottom)
            for (int y = 0; y < _screenHeight; y++)
            {
                float ratio = (float)y / _screenHeight;
                Color bgColor = Color.Lerp(
                    new Color(15, 15, 25),    // Very dark blue-gray
                    new Color(35, 25, 40),    // Slightly lighter purple-gray
                    ratio
                );
                spriteBatch.Draw(_pixel, new Rectangle(0, y, _screenWidth, 1), bgColor);
            }
        }

        private void DrawStalactites(SpriteBatch spriteBatch)
        {
            Color stalactiteColor = new Color(45, 40, 50);
            Color stalactiteShadow = new Color(30, 25, 35);

            // Draw stalactites hanging from ceiling at various positions
            DrawInvertedTriangle(spriteBatch, new Vector2(120, 0), 25, 60, stalactiteColor);
            DrawInvertedTriangle(spriteBatch, new Vector2(125, 0), 20, 55, stalactiteShadow);

            DrawInvertedTriangle(spriteBatch, new Vector2(300, 0), 30, 80, stalactiteColor);
            DrawInvertedTriangle(spriteBatch, new Vector2(305, 0), 25, 75, stalactiteShadow);

            DrawInvertedTriangle(spriteBatch, new Vector2(500, 0), 20, 50, stalactiteColor);
            DrawInvertedTriangle(spriteBatch, new Vector2(650, 0), 35, 70, stalactiteColor);
            DrawInvertedTriangle(spriteBatch, new Vector2(655, 0), 30, 65, stalactiteShadow);
        }

        private void DrawCaveWalls(SpriteBatch spriteBatch)
        {
            Color wallColor = new Color(40, 35, 45);
            Color wallShadow = new Color(25, 20, 30);

            // Left wall
            spriteBatch.Draw(_pixel, new Rectangle(0, 0, 30, _screenHeight), wallColor);

            // Right wall
            spriteBatch.Draw(_pixel, new Rectangle(_screenWidth - 30, 0, 30, _screenHeight), wallColor);

            // Add wall texture with horizontal lines
            for (int y = 0; y < _screenHeight; y += 40)
            {
                spriteBatch.Draw(_pixel, new Rectangle(0, y, 30, 2), wallShadow);
                spriteBatch.Draw(_pixel, new Rectangle(_screenWidth - 30, y, 30, 2), wallShadow);
            }
        }

        private void DrawPlatforms(SpriteBatch spriteBatch)
        {
            Color platformColor = new Color(50, 45, 55);
            Color platformShadow = new Color(30, 25, 35);
            Color platformHighlight = new Color(70, 60, 75);
            Color edgeColor = new Color(35, 30, 40);

            foreach (Rectangle platform in _platformHitboxes)
            {
                // Main platform body
                spriteBatch.Draw(_pixel, platform, platformColor);

                // Platform top highlight
                spriteBatch.Draw(_pixel,
                    new Rectangle(platform.X, platform.Y, platform.Width, 3),
                    platformHighlight);

                // Platform shadow/depth
                spriteBatch.Draw(_pixel,
                    new Rectangle(platform.X + 2, platform.Y + 3, platform.Width - 2, platform.Height - 3),
                    platformShadow);

                // Platform edges
                spriteBatch.Draw(_pixel,
                    new Rectangle(platform.X, platform.Y, 2, platform.Height),
                    edgeColor);
                spriteBatch.Draw(_pixel,
                    new Rectangle(platform.Right - 2, platform.Y, 2, platform.Height),
                    edgeColor);

                // Add some rock texture to larger platforms
                if (platform.Width > 150)
                {
                    for (int x = platform.X + 20; x < platform.Right - 20; x += 40)
                    {
                        // Small rock details
                        spriteBatch.Draw(_pixel,
                            new Rectangle(x, platform.Y + 5, 8, 4),
                            platformShadow);
                        spriteBatch.Draw(_pixel,
                            new Rectangle(x + 15, platform.Y + 8, 6, 3),
                            platformShadow);
                    }
                }
            }
        }

        private void DrawInvertedTriangle(SpriteBatch spriteBatch, Vector2 peak,
            int baseWidth, int height, Color color)
        {
            // Draw inverted triangle (stalactite) by stacking horizontal lines
            for (int y = 0; y < height; y++)
            {
                float ratio = (float)y / height;
                int lineWidth = (int)(baseWidth * (1.0f - ratio));
                int x = (int)(peak.X - lineWidth / 2);

                if (lineWidth > 0)
                {
                    spriteBatch.Draw(_pixel,
                        new Rectangle(x, (int)peak.Y + y, lineWidth, 1),
                        color);
                }
            }
        }

        private void DrawAtmosphericEffects(SpriteBatch spriteBatch)
        {
            // Add some mysterious glowing spots
            Color crystalGlow = new Color(100, 150, 255, 60); // Blue crystal glow
            Color torchGlow = new Color(255, 180, 100, 70);   // Warm torch glow

            // Crystal glows on various platforms
            DrawGlow(spriteBatch, 150, _screenHeight - 200, 15, crystalGlow);
            DrawGlow(spriteBatch, 400, _screenHeight - 320, 18, crystalGlow);
            DrawGlow(spriteBatch, 500, _screenHeight - 440, 12, crystalGlow);
            DrawGlow(spriteBatch, 250, _screenHeight - 560, 20, crystalGlow);

            // Torch-like glows
            DrawGlow(spriteBatch, 350, _screenHeight - 200, 25, torchGlow);
            DrawGlow(spriteBatch, 120, _screenHeight - 320, 22, torchGlow);
            DrawGlow(spriteBatch, 650, _screenHeight - 440, 28, torchGlow);

            // Add some floating particles effect
            DrawFloatingParticles(spriteBatch);
        }

        private void DrawGlow(SpriteBatch spriteBatch, int centerX, int centerY, int radius, Color glowColor)
        {
            // Draw a simple circular glow effect
            for (int r = radius; r > 0; r -= 3)
            {
                Color currentGlow = Color.Lerp(Color.Transparent, glowColor, (float)r / radius * 0.3f);
                spriteBatch.Draw(_pixel,
                    new Rectangle(centerX - r, centerY - r, r * 2, r * 2),
                    currentGlow);
            }
        }

        private void DrawFloatingParticles(SpriteBatch spriteBatch)
        {
            Color particleColor = new Color(200, 200, 255, 40);

            // Static floating particles for atmosphere
            for (int i = 0; i < 15; i++)
            {
                int x = (i * 53 + 100) % _screenWidth;
                int y = (i * 37 + 80) % (_screenHeight - 100);

                spriteBatch.Draw(_pixel, new Rectangle(x, y, 2, 2), particleColor);
                spriteBatch.Draw(_pixel, new Rectangle(x + 200, y + 150, 1, 1), particleColor);
            }
        }
    }
}