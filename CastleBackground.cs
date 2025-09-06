using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace fire_and_ice
{
    public class ScrollingCastleBackground
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _pixel;
        private int _screenWidth;
        private int _screenHeight;

        // Scrolling properties
        private float _cameraX;
        private float _backgroundWidth;
        private float _parallaxSpeed = 0.5f; // Speed multiplier for background elements

        // Floor collision hitbox (world coordinates)
        private Rectangle _floorHitbox;

        public Rectangle FloorHitbox => new Rectangle(
            (int)(_floorHitbox.X - _cameraX),
            _floorHitbox.Y,
            _floorHitbox.Width,
            _floorHitbox.Height);

        public float CameraX => _cameraX;

        public ScrollingCastleBackground(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            // Make background much wider than screen for scrolling
            _backgroundWidth = _screenWidth * 3; // 3 times wider than screen

            // Create a single pixel texture for drawing rectangles
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Create floor hitbox (in world coordinates)
            int floorY = _screenHeight - 180;
            _floorHitbox = new Rectangle(0, floorY, (int)_backgroundWidth, 180);

            _cameraX = 0;
        }

        public void UpdateCamera(Vector2 playerPosition)
        {
            // Follow the player horizontally
            _cameraX = playerPosition.X - _screenWidth / 2;

            // Clamp camera to background bounds
            _cameraX = MathHelper.Clamp(_cameraX, 0, _backgroundWidth - _screenWidth);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Sky gradient (fixed background)
            DrawSkyGradient(spriteBatch);

            // Draw distant mountains (slow parallax)
            DrawMountains(spriteBatch);

            // Draw castle walls (normal scrolling)
            DrawCastleWalls(spriteBatch);

            // Draw castle towers (normal scrolling)
            DrawTowers(spriteBatch);

            // Draw stone floor (normal scrolling)
            DrawStoneFloor(spriteBatch);
        }

        public void DrawFloorHitbox(SpriteBatch spriteBatch)
        {
            // Draw floor hitbox adjusted for camera position
            Rectangle screenFloorHitbox = new Rectangle(
                (int)(-_cameraX),
                _floorHitbox.Y,
                _floorHitbox.Width,
                _floorHitbox.Height
            );
            spriteBatch.Draw(_pixel, screenFloorHitbox, Color.Green * 0.2f);
        }

        private void DrawSkyGradient(SpriteBatch spriteBatch)
        {
            // Sky doesn't scroll - it's always visible
            for (int y = 0; y < _screenHeight * 0.7f; y++)
            {
                float ratio = y / (_screenHeight * 0.7f);
                Color skyColor = Color.Lerp(
                    new Color(25, 25, 60),    // Dark blue
                    new Color(70, 90, 120),    // Lighter blue
                    ratio
                );
                spriteBatch.Draw(_pixel, new Rectangle(0, y, _screenWidth, 1), skyColor);
            }
        }

        private void DrawMountains(SpriteBatch spriteBatch)
        {
            // Mountains scroll slower (parallax effect)
            Color mountainColor = new Color(40, 45, 55);
            float mountainOffset = _cameraX * _parallaxSpeed;

            // Draw multiple mountain sets for seamless scrolling
            for (int i = -1; i <= 2; i++)
            {
                float baseX = i * _screenWidth - mountainOffset;

                // Mountain 1
                DrawTriangle(spriteBatch,
                    new Vector2(baseX + 100, _screenHeight * 0.7f),
                    200, 150, mountainColor);

                // Mountain 2
                DrawTriangle(spriteBatch,
                    new Vector2(baseX + 300, _screenHeight * 0.7f),
                    250, 180, mountainColor);

                // Mountain 3
                DrawTriangle(spriteBatch,
                    new Vector2(baseX + 550, _screenHeight * 0.7f),
                    200, 140, mountainColor);
            }
        }

        private void DrawCastleWalls(SpriteBatch spriteBatch)
        {
            Color wallColor = new Color(60, 60, 70);
            Color wallShadow = new Color(40, 40, 50);

            int wallHeight = 250;
            int wallY = _screenHeight - 180 - wallHeight;

            // Calculate visible portion of the wall
            int startX = (int)_cameraX;
            int endX = (int)(_cameraX + _screenWidth);

            // Main castle wall (adjusted for camera)
            spriteBatch.Draw(_pixel,
                new Rectangle(-startX, wallY, (int)_backgroundWidth, wallHeight),
                wallColor);

            // Battlements (crenellations)
            int crenelWidth = 40;
            int crenelHeight = 30;
            int crenelSpacing = 30;

            for (int x = 0; x < _backgroundWidth; x += crenelWidth + crenelSpacing)
            {
                // Only draw if visible on screen
                if (x - _cameraX > -crenelWidth && x - _cameraX < _screenWidth + crenelWidth)
                {
                    spriteBatch.Draw(_pixel,
                        new Rectangle((int)(x - _cameraX), wallY - crenelHeight, crenelWidth, crenelHeight),
                        wallColor);
                }
            }

            // Stone texture lines
            for (int y = wallY; y < wallY + wallHeight; y += 40)
            {
                spriteBatch.Draw(_pixel,
                    new Rectangle(-startX, y, (int)_backgroundWidth, 2),
                    wallShadow);
            }

            for (int x = 0; x < _backgroundWidth; x += 60)
            {
                if (x - _cameraX > -2 && x - _cameraX < _screenWidth + 2)
                {
                    spriteBatch.Draw(_pixel,
                        new Rectangle((int)(x - _cameraX), wallY, 2, wallHeight),
                        wallShadow);
                }
            }
        }

        private void DrawTowers(SpriteBatch spriteBatch)
        {
            Color towerColor = new Color(65, 65, 75);
            Color roofColor = new Color(80, 40, 40);
            Color windowColor = new Color(20, 20, 30);

            // Place towers at different positions across the wider background
            DrawTower(spriteBatch, 200, 180, towerColor, roofColor, windowColor);
            DrawTower(spriteBatch, 600, 200, towerColor, roofColor, windowColor);
            DrawTower(spriteBatch, 1000, 180, towerColor, roofColor, windowColor);
            DrawTower(spriteBatch, 1400, 220, towerColor, roofColor, windowColor);
            DrawTower(spriteBatch, 1800, 190, towerColor, roofColor, windowColor);
            DrawTower(spriteBatch, 2200, 180, towerColor, roofColor, windowColor);
        }

        private void DrawTower(SpriteBatch spriteBatch, int worldX, int height,
            Color towerColor, Color roofColor, Color windowColor)
        {
            // Convert world position to screen position
            int screenX = (int)(worldX - _cameraX);
            int towerWidth = 100;

            // Only draw if tower is visible on screen
            if (screenX > -towerWidth && screenX < _screenWidth + towerWidth)
            {
                int towerY = _screenHeight - 180 - height;

                // Tower body
                spriteBatch.Draw(_pixel,
                    new Rectangle(screenX, towerY, towerWidth, height),
                    towerColor);

                // Tower roof (triangle)
                DrawTriangle(spriteBatch,
                    new Vector2(screenX + towerWidth / 2, towerY),
                    towerWidth + 20, 60, roofColor);

                // Windows
                int windowWidth = 20;
                int windowHeight = 30;

                // Top window
                spriteBatch.Draw(_pixel,
                    new Rectangle(screenX + towerWidth / 2 - windowWidth / 2,
                        towerY + 20, windowWidth, windowHeight),
                    windowColor);

                // Middle window
                spriteBatch.Draw(_pixel,
                    new Rectangle(screenX + towerWidth / 2 - windowWidth / 2,
                        towerY + 70, windowWidth, windowHeight),
                    windowColor);

                // Window glow effect
                Color glowColor = new Color(255, 200, 100, 50);
                spriteBatch.Draw(_pixel,
                    new Rectangle(screenX + towerWidth / 2 - windowWidth / 2 - 5,
                        towerY + 20 - 5, windowWidth + 10, windowHeight + 10),
                    glowColor);
            }
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 peak,
            int baseWidth, int height, Color color)
        {
            for (int y = 0; y < height; y++)
            {
                float ratio = (float)y / height;
                int lineWidth = (int)(baseWidth * ratio);
                int x = (int)(peak.X - lineWidth / 2);

                spriteBatch.Draw(_pixel,
                    new Rectangle(x, (int)peak.Y - height + y, lineWidth, 1),
                    color);
            }
        }

        private void DrawStoneFloor(SpriteBatch spriteBatch)
        {
            Color floorColor = new Color(45, 45, 50);
            Color floorShadow = new Color(35, 35, 40);

            int floorY = _screenHeight - 180;

            // Main floor (adjusted for camera)
            spriteBatch.Draw(_pixel,
                new Rectangle((int)-_cameraX, floorY, (int)_backgroundWidth, 180),
                floorColor);

            // Stone pattern
            for (int y = floorY; y < _screenHeight; y += 30)
            {
                // Horizontal lines
                spriteBatch.Draw(_pixel,
                    new Rectangle((int)-_cameraX, y, (int)_backgroundWidth, 2),
                    floorShadow);

                for (int x = 0; x < _backgroundWidth; x += 80)
                {
                    // Only draw if visible
                    if (x - _cameraX > -80 && x - _cameraX < _screenWidth + 80)
                    {
                        int offset = ((y - floorY) / 30) % 2 == 0 ? 0 : 40;

                        // Vertical lines
                        spriteBatch.Draw(_pixel,
                            new Rectangle((int)(x + offset - _cameraX), y, 2, 30),
                            floorShadow);
                    }
                }
            }
        }
    }
}