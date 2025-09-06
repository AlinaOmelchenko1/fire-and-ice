using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace fire_and_ice
{
    public class PlatformProtagonist
    {
        // Initialize attributes
        private Texture2D _texture;
        private Vector2 _position;
        private int _frameWidth, _frameHeight, _frameCount;
        private int _currentFrame;
        private double _timer;
        private double _interval = 0.15; // seconds per frame
        private bool _isMoving; // Track if character is moving
        private float _speed = 200f; // Movement speed in pixels per second

        // Physics and collision
        private Vector2 _velocity;
        private float _gravity = 800f; // Gravity strength
        private float _jumpPower = 400f; // Jump strength
        private bool _isOnGround;
        private bool _wasJumpPressed; // To prevent continuous jumping

        // Hitbox
        private Rectangle _hitbox;
        private int _hitboxOffsetX = 5; // Offset from sprite edge
        private int _hitboxOffsetY = 0;
        private int _hitboxWidth;
        private int _hitboxHeight;

        // Screen bounds
        private int _screenWidth;
        private int _screenHeight;

        public Vector2 Position => _position;
        public Rectangle Hitbox => _hitbox;
        public bool IsOnGround => _isOnGround;

        public PlatformProtagonist(Texture2D texture, Vector2 position, int frameWidth, int frameHeight, int frameCount)
        {
            _texture = texture;
            _position = position;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _frameCount = frameCount;
            _currentFrame = 0;
            _isMoving = false;
            _velocity = Vector2.Zero;
            _isOnGround = false;

            // Set hitbox size (slightly smaller than sprite for better gameplay)
            _hitboxWidth = frameWidth - (_hitboxOffsetX * 2);
            _hitboxHeight = frameHeight - _hitboxOffsetY;

            UpdateHitbox();
        }

        public void SetBounds(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        private void UpdateHitbox()
        {
            _hitbox = new Rectangle(
                (int)_position.X + _hitboxOffsetX,
                (int)_position.Y + _hitboxOffsetY,
                _hitboxWidth,
                _hitboxHeight
            );
        }

        public void Update(GameTime gameTime, List<Rectangle> platformHitboxes)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 oldPosition = _position;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Horizontal movement input
            Vector2 movement = Vector2.Zero;
            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                movement.X += 1f;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                movement.X -= 1f;

            // Jump input (only if on ground and key just pressed)
            bool jumpPressed = ks.IsKeyDown(Keys.Space) || ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W);
            if (jumpPressed && !_wasJumpPressed && _isOnGround)
            {
                _velocity.Y = -_jumpPower; // Negative Y = upward
                _isOnGround = false;
            }
            _wasJumpPressed = jumpPressed;

            // Apply horizontal movement
            if (movement.X != 0)
            {
                _velocity.X = movement.X * _speed;
            }
            else
            {
                // Apply friction when not moving
                _velocity.X *= 0.85f;
            }

            // Apply gravity
            if (!_isOnGround)
            {
                _velocity.Y += _gravity * deltaTime;
            }

            // Apply horizontal movement first
            _position.X += _velocity.X * deltaTime;

            // Keep character within screen bounds horizontally
            _position.X = MathHelper.Clamp(_position.X, 30, _screenWidth - _frameWidth - 30);

            // Apply vertical movement
            _position.Y += _velocity.Y * deltaTime;

            // Update hitbox position
            UpdateHitbox();

            // Check collision with all platforms
            CheckPlatformCollisions(platformHitboxes);

            // Prevent falling through bottom of screen (safety net)
            if (_position.Y > _screenHeight - _frameHeight)
            {
                _position.Y = _screenHeight - _frameHeight;
                _velocity.Y = 0;
                _isOnGround = true;
                UpdateHitbox();
            }

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Player Y: {_position.Y}, Velocity Y: {_velocity.Y}, On Ground: {_isOnGround}");

            // Check if character is moving for animation
            _isMoving = (oldPosition.X != _position.X) && _isOnGround;

            // Animation
            if (_isMoving)
            {
                _timer += gameTime.ElapsedGameTime.TotalSeconds;
                if (_timer > _interval)
                {
                    _currentFrame++;
                    if (_currentFrame >= _frameCount)
                        _currentFrame = 0;
                    _timer = 0;
                }
            }
            else
            {
                _currentFrame = 0;
                _timer = 0;
            }
        }

        private void CheckPlatformCollisions(List<Rectangle> platformHitboxes)
        {
            _isOnGround = false;

            foreach (Rectangle platform in platformHitboxes)
            {
                // Check if character's hitbox intersects with platform
                if (_hitbox.Intersects(platform))
                {
                    // Check if character is falling onto the platform from above
                    if (_velocity.Y >= 0 && _position.Y < platform.Top)
                    {
                        // Position character on top of the platform
                        _position.Y = platform.Top - _frameHeight;
                        _velocity.Y = 0;
                        _isOnGround = true;
                        UpdateHitbox();
                        break; // Stop checking other platforms once we land on one
                    }
                    // Check for side collisions (optional - prevents walking through platforms)
                    else if (_velocity.Y < 0 && _position.Y + _frameHeight > platform.Bottom)
                    {
                        // Hit platform from below (head bonk)
                        _position.Y = platform.Bottom;
                        _velocity.Y = 0;
                        UpdateHitbox();
                        break;
                    }
                }
            }

            // Additional check: if character is standing on a platform
            if (!_isOnGround)
            {
                Rectangle futureHitbox = new Rectangle(
                    _hitbox.X,
                    _hitbox.Y + 5, // Check slightly below current position
                    _hitbox.Width,
                    _hitbox.Height
                );

                foreach (Rectangle platform in platformHitboxes)
                {
                    if (futureHitbox.Intersects(platform) && _velocity.Y >= 0)
                    {
                        // Character is close enough to a platform to be considered "on ground"
                        float characterBottom = _position.Y + _frameHeight;
                        float platformTop = platform.Top;

                        if (System.Math.Abs(characterBottom - platformTop) <= 5)
                        {
                            _position.Y = platformTop - _frameHeight;
                            _velocity.Y = 0;
                            _isOnGround = true;
                            UpdateHitbox();
                            break;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, _position, sourceRect, Color.White);
        }

        // Method to draw hitbox for debugging
        public void DrawHitbox(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            spriteBatch.Draw(pixelTexture, _hitbox, Color.Red * 0.4f);
        }
    }
}