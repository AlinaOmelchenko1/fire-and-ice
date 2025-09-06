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
        private PlatformProtagonist _player; // Changed to platform protagonist
        private PlatformCaveBackground _caveBackground; // Changed to platform cave background
        private Texture2D _pixelTexture; // For drawing debug hitboxes
        private bool _showHitboxes = false; // Toggle for debugging
        private KeyboardState _previousKeyboardState;

        public Game1() //create instance of graphic manager and point to content (where to get images , from imported by monogame)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() //imported by monogame initialise the game 
        {
            // Add initialisation logic here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _caveBackground = new PlatformCaveBackground(GraphicsDevice); // Use platform cave background

            // Create pixel texture for hitbox debugging
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            Texture2D heroTexture = Content.Load<Texture2D>("hero_walk");

            // The sprite sheet appears to have 4 frames horizontally
            // Calculate the actual frame dimensions
            int frameCount = 4;
            int frameWidth = heroTexture.Width / frameCount;  // Total width divided by 4
            int frameHeight = heroTexture.Height;  // Full height of the image


            // Start position - on the bottom floor
            Vector2 startPosition = new Vector2(
                GraphicsDevice.Viewport.Width / 2 - frameWidth / 2, // Center horizontally
                GraphicsDevice.Viewport.Height - 120 - frameHeight  // On bottom platform
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
            _player.Update(gameTime, _caveBackground.PlatformHitboxes);

            _previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Draw platform cave background
            _caveBackground.Draw(_spriteBatch);

            // Draw player
            _player.Draw(_spriteBatch);

            // Debug: Draw hitboxes if enabled
            if (_showHitboxes)
            {
                _caveBackground.DrawPlatformHitboxes(_spriteBatch);
                _player.DrawHitbox(_spriteBatch, _pixelTexture);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}