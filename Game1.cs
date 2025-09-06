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
        private ColorCollisionProtagonist _player; // Changed to color collision protagonist
        private ImageLevelBackground _levelBackground; // Background with color collision
        private Texture2D _pixelTexture; // For drawing debug hitboxes
        private bool _showHitboxes = false; // Toggle for debugging
        private bool _showColorDebug = false; // Toggle for color debugging
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

            // Create image-based level background with color collision
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

            // Start position - character will fall to ground based on colors
            Vector2 startPosition = new Vector2(
                0, // X position
               GraphicsDevice.Viewport.Height - 20   // Y position - start high so character falls to ground
            );
            System.Diagnostics.Debug.WriteLine($"Setting start position to: {startPosition}");
            _player = new ColorCollisionProtagonist(heroTexture, startPosition, frameWidth, frameHeight, frameCount, _levelBackground.CollisionSystem);

            // Set the screen bounds
            _player.SetBounds(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // Debug: Sample colors from the bottom of the screen to help setup
            Rectangle sampleArea = new Rectangle(0, GraphicsDevice.Viewport.Height - 200, GraphicsDevice.Viewport.Width, 200);
            _levelBackground.SampleColorsInArea(sampleArea);
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

            // Toggle color debugging (press C - only once per press)
            if (currentKeyboardState.IsKeyDown(Keys.C) && !_previousKeyboardState.IsKeyDown(Keys.C))
            {
                _showColorDebug = !_showColorDebug;
                if (_showColorDebug)
                    _player.DebugColors(); // Print colors under character
            }

            // Update player with color collision
            _player.Update(gameTime);

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

            // Debug: Draw character hitbox if enabled
            if (_showHitboxes)
            {
                _player.DrawHitbox(_spriteBatch, _pixelTexture);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}