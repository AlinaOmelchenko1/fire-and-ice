using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice
{
    public class Game1 : Game
    {
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
        // Platform rectangles
        private Rectangle[] _platforms;

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

        //
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load images
            _levelTexture = Content.Load<Texture2D>("first_level");
            _heroTexture = Content.Load<Texture2D>("hero_walk");

            // Calculate frame dimensions
            _frameWidth = _heroTexture.Width / 4;  // 4 frames
            _frameHeight = _heroTexture.Height;

            // Create pixel texture of hitbox and floor level for debug 
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Set player starting position DIRECTLY on the ground
            _playerPosition = new Vector2(100, GROUND_Y - _frameHeight);
            _playerVelocity = Vector2.Zero;
            _isOnGround = true;

        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle hitboxes with H
            if (currentKeyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H))
            {
                _showHitboxes = !_showHitboxes;
            }

            // HORIZONTAL MOVEMENT
            float moveX = 0;
            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D))
                moveX = 1;
            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A))
                moveX = -1;

            _playerVelocity.X = moveX * _moveSpeed;

            // JUMPING
            bool jumpPressed = currentKeyboardState.IsKeyDown(Keys.Space) ||
                              currentKeyboardState.IsKeyDown(Keys.Up) ||
                              currentKeyboardState.IsKeyDown(Keys.W);

            if (jumpPressed && !_wasJumpPressed && _isOnGround)
            {
                _playerVelocity.Y = -_jumpPower;
                _isOnGround = false;
            }
            _wasJumpPressed = jumpPressed;

            // GRAVITY
            if (!_isOnGround)
            {
                _playerVelocity.Y += _gravity * deltaTime;
                if (_playerVelocity.Y > 1000)
                    _playerVelocity.Y = 1000; // Terminal velocity
            }

            // APPLY MOVEMENT
            _playerPosition.X += _playerVelocity.X * deltaTime;
            _playerPosition.Y += _playerVelocity.Y * deltaTime;

            // COLLISION WITH GROUND (SIMPLE)
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

            // KEEP PLAYER ON SCREEN
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0,
                GraphicsDevice.Viewport.Width - _frameWidth);

            // Prevent going above screen
            if (_playerPosition.Y < 0)
            {
                _playerPosition.Y = 0;
                _playerVelocity.Y = 0;
            }

            // ANIMATION
            if (Math.Abs(_playerVelocity.X) > 0.1f && _isOnGround)
            {
                _animationTimer += deltaTime;
                if (_animationTimer > _animationInterval)
                {
                    _currentFrame = (_currentFrame + 1) % 4;
                    _animationTimer = 0;
                }
            }
            else
            {
                _currentFrame = 0;
                _animationTimer = 0;
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

            // Debug drawing
            if (_showHitboxes)
            {
                // Player hitbox
                Rectangle playerRect = new Rectangle(
                    (int)_playerPosition.X, (int)_playerPosition.Y,
                    _frameWidth, _frameHeight);
                _spriteBatch.Draw(_pixelTexture, playerRect, Color.Red * 0.3f);

                // Draw all platforms (only if initialized)
                if (_platforms != null)
                {
                    foreach (Rectangle platform in _platforms)
                    {
                        _spriteBatch.Draw(_pixelTexture, platform, Color.Green * 0.3f);
                    }
                }

                // Ground line
                Rectangle groundLine = new Rectangle(
                    0, (int)GROUND_Y,
                    GraphicsDevice.Viewport.Width, 2);
                _spriteBatch.Draw(_pixelTexture, groundLine, Color.Yellow);
            }

            // Show collision map overlay
            if (_showCollisionMap && _collisionMapTexture != null)
            {
                Rectangle collisionMapRect = new Rectangle(0, 0,
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height);
                _spriteBatch.Draw(_collisionMapTexture, collisionMapRect, Color.White * 0.5f);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}