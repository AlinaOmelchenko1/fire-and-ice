using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice
{
    public class Game1 : Game
    {
        //attributes for graphics and collision map 
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _levelTexture;
        private Texture2D _heroTexture;
        private Texture2D _pixelTexture;
        private Texture2D _collisionMapTexture;  
        private Color[] _collisionMapData;        
        
        // Simple player variables
        private Vector2 _playerPosition;
        private Vector2 _playerVelocity;
        private int _frameWidth, _frameHeight;
        private float _gravity = 800f;
        private float _jumpPower = 400f;
        private float _moveSpeed = 200f;
        private bool _isOnGround = false;
        private bool _wasJumpPressed = false;

        // Animation
        private int _currentFrame = 0;
        private double _animationTimer = 0;
        private double _animationInterval = 0.15;

        // Debugging 
        private bool _showHitboxes = false;
        private bool _showCollisionMap = false;
        private KeyboardState _previousKeyboardState;

        // Fixed ground level where chracter stands 
        private const float GROUND_Y = 420f;

        // Platform rectangles for collision 
        private Rectangle[] _platforms;

        //i dont remmber what this does got from tutorial 
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        //initialise the game from framework dont delete 
        protected override void Initialize()
        {
            base.Initialize();
        }

        //loads images of level and sprite
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load files
            _levelTexture = Content.Load<Texture2D>("first_level");
            _heroTexture = Content.Load<Texture2D>("hero_walk");

            //Frame dimensions
            _frameWidth = _heroTexture.Width / 4;  // 4 frames
            _frameHeight = _heroTexture.Height;

            // Create coloured hitbox and floor level for debug 
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Set player starting position DIRECTLY on the ground MAKE A NORE OF THIS IN NEA
            _playerPosition = new Vector2(100, GROUND_Y - _frameHeight);
            _playerVelocity = Vector2.Zero;
            _isOnGround = true;
        }

        protected override void Update(GameTime gameTime)
        {
            //get keyboard state
            KeyboardState currentKeyboardState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Show hitbox with H for debugging
            if (currentKeyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H))
            {
                _showHitboxes = !_showHitboxes;
            }

            // horizontal movement 
            float moveX = 0;
            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D))
                moveX = 1;
            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A))
                moveX = -1;

            _playerVelocity.X = moveX * _moveSpeed;

            // Jumping
            bool jumpPressed = currentKeyboardState.IsKeyDown(Keys.Space) ||
                              currentKeyboardState.IsKeyDown(Keys.Up) ||
                              currentKeyboardState.IsKeyDown(Keys.W);

            if (jumpPressed && !_wasJumpPressed && _isOnGround)
            {
                _playerVelocity.Y = -_jumpPower;
                _isOnGround = false;
            }
            _wasJumpPressed = jumpPressed;

            // gravity
            if (!_isOnGround)
            {
                _playerVelocity.Y += _gravity * deltaTime;
                if (_playerVelocity.Y > 1000)
                    _playerVelocity.Y = 1000; // Terminal velocity so does not fall too fast 
            }

            // movement 
            _playerPosition.X += _playerVelocity.X * deltaTime;
            _playerPosition.Y += _playerVelocity.Y * deltaTime;

            // collison with ground
            float playerBottom = _playerPosition.Y + _frameHeight;

            if (playerBottom >= GROUND_Y)
            {
                // Hit the ground
                _playerPosition.Y = GROUND_Y - _frameHeight;
                _playerVelocity.Y = 0;
                _isOnGround = true;
            }
            else if (playerBottom >= GROUND_Y - 5 && _playerVelocity.Y >= 0)
            {
                // Close enough to ground and not jumping
                _isOnGround = true;
            }
            else
            {
                // In the air
                _isOnGround = false;
            }

            // keeps player on the screen
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0,
                GraphicsDevice.Viewport.Width - _frameWidth);

            // Prevent going above screen
            if (_playerPosition.Y < 0)
            {
                _playerPosition.Y = 0;
                _playerVelocity.Y = 0;
            } 

            _previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw level background
            Rectangle levelDestRect = new Rectangle(0, 0,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_levelTexture, levelDestRect, Color.White);

            // Draw player
            Rectangle sourceRect = new Rectangle(
                _currentFrame * _frameWidth, 0,
                _frameWidth, _frameHeight);
            _spriteBatch.Draw(_heroTexture, _playerPosition, sourceRect, Color.White);

            // Debug hitbox
            if (_showHitboxes)
            {
                // Player hitbox
                Rectangle playerRect = new Rectangle(
                    (int)_playerPosition.X, (int)_playerPosition.Y,
                    _frameWidth, _frameHeight);
                _spriteBatch.Draw(_pixelTexture, playerRect, Color.Red * 0.3f);

                // Coloured ground line
                Rectangle groundLine = new Rectangle(
                    0, (int)GROUND_Y,
                    GraphicsDevice.Viewport.Width, 2);
                _spriteBatch.Draw(_pixelTexture, groundLine, Color.Yellow);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}