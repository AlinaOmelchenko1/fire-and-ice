using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace fire_and_ice
{
    public class ImageLevelBackground
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _backgroundTexture;
        private int _screenWidth;
        private int _screenHeight;

        // Camera for scrolling (if needed)
        private Vector2 _cameraPosition;
        private bool _enableScrolling;

        // Color collision system
        private ColorCollisionSystem _collisionSystem;

        public Vector2 CameraPosition => _cameraPosition;
        public int LevelWidth => _backgroundTexture?.Width ?? _screenWidth;
        public int LevelHeight => _backgroundTexture?.Height ?? _screenHeight;
        public ColorCollisionSystem CollisionSystem => _collisionSystem;

        public ImageLevelBackground(GraphicsDevice graphicsDevice, ContentManager content, bool enableScrolling = false)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            _enableScrolling = enableScrolling;
            _cameraPosition = Vector2.Zero;

            // Load the background image
            _backgroundTexture = content.Load<Texture2D>("first_level");

            // Create color collision system
            _collisionSystem = new ColorCollisionSystem(_backgroundTexture);
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

        // Debug method to sample colors in an area
        public void SampleColorsInArea(Rectangle area)
        {
            var colorSamples = _collisionSystem.SampleColors(area);
            System.Diagnostics.Debug.WriteLine("Color samples in area:");
            foreach (var sample in colorSamples)
            {
                System.Diagnostics.Debug.WriteLine($"Color: R{sample.Key.R} G{sample.Key.G} B{sample.Key.B} - Count: {sample.Value}");
            }
        }
    }
}