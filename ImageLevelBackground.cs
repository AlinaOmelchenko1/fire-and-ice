using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class ImageLevelBackground
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _backgroundTexture;
        private Texture2D _pixel; // For drawing debug hitboxes
        private int _screenWidth;
        private int _screenHeight;

        // Platform collision hitboxes - you'll define these based on your image
        private List<Rectangle> _platformHitboxes;

        // Camera for scrolling (if needed)
        private Vector2 _cameraPosition;
        private bool _enableScrolling;

        public List<Rectangle> PlatformHitboxes => _platformHitboxes;
        public Vector2 CameraPosition => _cameraPosition;
        public int LevelWidth => _backgroundTexture?.Width ?? _screenWidth;
        public int LevelHeight => _backgroundTexture?.Height ?? _screenHeight;

        public ImageLevelBackground(GraphicsDevice graphicsDevice, ContentManager content, bool enableScrolling = false)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            _enableScrolling = enableScrolling;
            _cameraPosition = Vector2.Zero;

            // Load the background image
            _backgroundTexture = content.Load<Texture2D>("first_level");

            // Create pixel texture for debug drawing
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Create platform hitboxes based on your level design
            CreatePlatformHitboxes();
        }

        private void CreatePlatformHitboxes()
        {
            _platformHitboxes = new List<Rectangle>();

            // You'll need to manually define these based on your first_level.jpeg
            // These are example platforms - adjust them to match your image!

            // Bottom floor (adjust Y position based on your image)
            _platformHitboxes.Add(new Rectangle(0, _screenHeight - 80, _screenWidth, 80));

            // Example platforms - modify these coordinates to match your level image
            _platformHitboxes.Add(new Rectangle(100, _screenHeight - 200, 150, 20));
            _platformHitboxes.Add(new Rectangle(300, _screenHeight - 150, 120, 20));
            _platformHitboxes.Add(new Rectangle(500, _screenHeight - 250, 200, 20));
            _platformHitboxes.Add(new Rectangle(150, _screenHeight - 350, 180, 20));
            _platformHitboxes.Add(new Rectangle(400, _screenHeight - 400, 160, 20));

            // Add more platforms as needed for your specific level design
        }

        public void UpdateCamera(Vector2 playerPosition)
        {
            if (!_enableScrolling) return;

            // Center camera on player
            _cameraPosition.X = playerPosition.X - _screenWidth / 2;
            _cameraPosition.Y = playerPosition.Y - _screenHeight / 2;

            // Clamp camera to level bounds
            _cameraPosition.X = MathHelper.Clamp(_cameraPosition.X, 0, LevelWidth - _screenWidth);
            _cameraPosition.Y = MathHelper.Clamp(_cameraPosition.Y, 0, LevelHeight - _screenHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background image
            if (_enableScrolling)
            {
                // Draw with camera offset for scrolling
                spriteBatch.Draw(_backgroundTexture, -_cameraPosition, Color.White);
            }
            else
            {
                // Draw static background (scaled to fit screen if needed)
                Rectangle destinationRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
                spriteBatch.Draw(_backgroundTexture, destinationRect, Color.White);
            }
        }

        public void DrawPlatformHitboxes(SpriteBatch spriteBatch)
        {
            foreach (Rectangle platform in _platformHitboxes)
            {
                Rectangle drawRect = platform;

                // Adjust for camera if scrolling is enabled
                if (_enableScrolling)
                {
                    drawRect.X -= (int)_cameraPosition.X;
                    drawRect.Y -= (int)_cameraPosition.Y;
                }

                spriteBatch.Draw(_pixel, drawRect, Color.Green * 0.3f);
            }
        }

        // Method to get platforms adjusted for camera position
        public List<Rectangle> GetAdjustedPlatformHitboxes()
        {
            if (!_enableScrolling)
                return _platformHitboxes;

            List<Rectangle> adjustedPlatforms = new List<Rectangle>();
            foreach (Rectangle platform in _platformHitboxes)
            {
                Rectangle adjusted = new Rectangle(
                    platform.X - (int)_cameraPosition.X,
                    platform.Y - (int)_cameraPosition.Y,
                    platform.Width,
                    platform.Height
                );
                adjustedPlatforms.Add(adjusted);
            }
            return adjustedPlatforms;
        }

        // Helper method to add platforms dynamically (useful for level editing)
        public void AddPlatform(Rectangle platform)
        {
            _platformHitboxes.Add(platform);
        }

        // Helper method to clear all platforms (useful for level editing)
        public void ClearPlatforms()
        {
            _platformHitboxes.Clear();
        }
    }
}