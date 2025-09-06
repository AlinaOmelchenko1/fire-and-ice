using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice
{
    public class Game1 : Game //inheritance 
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlatformProtagonist _player;
        private ImageLevelBackground _levelBackground; // Changed to image-based background
        private Texture2D _pixelTexture; // For drawing debug hitboxes
        private bool _showHitboxes = false; // Toggle for debugging
        private KeyboardState _previousKeyboardState;

        public Game1() //create instance of graphic manager and point to content (where to get images , from imported by monogame)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // You can adjust window size to match your level image dimensions if needed
            // _graphics.PreferredBackBufferWidth = 1024;
            // _graphics.PreferredBackBufferHeight = 768;
        }

        protected override void Initialize() //imported by monogame initialise the game 
        {
            // Add initialisation logic here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create image-based level background
            // Set second parameter to true if you want scrolling camera
            _levelBackground = new ImageLevelBackground(GraphicsDevice, Content, false);

            // Create pixel texture for hitbox debugging
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            Texture2D heroTexture = Content.Load<Texture2D>("hero_walk");

            // The sprite sheet appears to have 4 frames horizontally
            // Calculate the actual frame dimensions
            int frameCount = 4;
            int frameWidth = heroTexture.Width / frameCount;  // Total width divided by 4
            int frameHeight = heroTexture.Height;  // Full height of the image

            // Start position - adjust these coordinates to match your level image
            Vector2 startPosition = new Vector2(
                100, // X position - adjust based on your level
                GraphicsDevice.Viewport.Height - 160 - frameHeight  // Y position - above bottom platform
            );

            _player = new PlatformProtagonist(heroTexture, startPosition, frameWidth, frameHeight, frameCount);

            // Set the screen bounds
            _player.SetBounds(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle hitbox visibility for debugging (press H - only once per press)
            if (currentKeyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H))
                _showHitboxes = !_showHitboxes;

            // Update player with platform collision
            _player.Update(gameTime, _levelBackground.PlatformHitboxes);

            // Update camera if scrolling is enabled
            _levelBackground.UpdateCamera(_player.Position);

            _previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Draw the image-based level background
            _levelBackground.Draw(_spriteBatch);

            // Draw player
            _player.Draw(_spriteBatch);

            // Debug: Draw hitboxes if enabled
            if (_showHitboxes)
            {
                _levelBackground.DrawPlatformHitboxes(_spriteBatch);
                _player.DrawHitbox(_spriteBatch, _pixelTexture);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}