using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class ColorCollisionProtagonist
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
        private float _gravity = 600f; // Gravity strength
        private float _jumpPower = 350f; // Jump strength
        private bool _isOnGround;
        private bool _wasJumpPressed; // To prevent continuous jumping

        // Hitbox
        private Rectangle _hitbox;
        private int _hitboxOffsetX = 5; // Offset from sprite edge
        private int _hitboxOffsetY = 0;
        private int _hitboxWidth;
        private int _hitboxHeight;

        // Color collision system
        private ColorCollisionSystem _collisionSystem;

        // Screen bounds
        private int _screenWidth;
        private int _screenHeight;

        // Special effects
        private bool _isInvulnerable = false;
        private double _invulnerabilityTimer = 0;
        private double _invulnerabilityDuration = 1.0;

        public Vector2 Position => _position;
        public Rectangle Hitbox => _hitbox;
        public bool IsOnGround => _isOnGround;
        public bool IsInvulnerable => _isInvulnerable;

        public ColorCollisionProtagonist(Texture2D texture, Vector2 position, int frameWidth, int frameHeight, int frameCount, ColorCollisionSystem collisionSystem)
        {
            _texture = texture;
            _position = position;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _frameCount = frameCount;
            _collisionSystem = collisionSystem;
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

        public void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 oldPosition = _position;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update invulnerability
            if (_isInvulnerable)
            {
                _invulnerabilityTimer += deltaTime;
                if (_invulnerabilityTimer >= _invulnerabilityDuration)
                {
                    _isInvulnerable = false;
                    _invulnerabilityTimer = 0;
                }
            }

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

            // Apply horizontal movement first and check collision
            Vector2 newPosition = _position;
            newPosition.X += _velocity.X * deltaTime;

            // Check horizontal collision
            Rectangle horizontalHitbox = new Rectangle(
                (int)newPosition.X + _hitboxOffsetX,
                (int)_position.Y + _hitboxOffsetY,
                _hitboxWidth,
                _hitboxHeight
            );

            if (!_collisionSystem.IsAreaSolid(horizontalHitbox))
            {
                _position.X = newPosition.X;
            }
            else
            {
                _velocity.X = 0; // Stop horizontal movement if hitting wall
            }

            // Apply vertical movement and check collision
            newPosition = _position;
            newPosition.Y += _velocity.Y * deltaTime;

            Rectangle verticalHitbox = new Rectangle(
                (int)_position.X + _hitboxOffsetX,
                (int)newPosition.Y + _hitboxOffsetY,
                _hitboxWidth,
                _hitboxHeight
            );

            // Check ground collision
            CheckGroundCollision(newPosition, verticalHitbox);

            // Keep character within screen bounds
            _position.X = MathHelper.Clamp(_position.X, 0, _screenWidth - _frameWidth);
            _position.Y = MathHelper.Clamp(_position.Y, 0, _screenHeight - _frameHeight);

            // Update hitbox position
            UpdateHitbox();

            // Check for special surface interactions
            CheckSurfaceInteractions();

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

        private void CheckGroundCollision(Vector2 newPosition, Rectangle verticalHitbox)
        {
            _isOnGround = false;

            // Check if landing on solid ground
            if (_collisionSystem.IsAreaSolid(verticalHitbox) && _velocity.Y >= 0)
            {
                // Find exact ground level
                int groundY = _collisionSystem.FindGroundLevel((int)(_position.X + _frameWidth / 2));
                if (groundY < _screenHeight)
                {
                    _position.Y = groundY - _frameHeight;
                    _velocity.Y = 0;
                    _isOnGround = true;
                }
            }
            // Check if landing on platform
            else if (_collisionSystem.IsOnPlatform(_hitbox, _velocity.Y))
            {
                if (_velocity.Y > 0)
                {
                    int platformY = _collisionSystem.FindGroundLevel((int)(_position.X + _frameWidth / 2));
                    _position.Y = platformY - _frameHeight;
                    _velocity.Y = 0;
                    _isOnGround = true;
                }
            }
            else if (_velocity.Y >= 0)
            {
                // No collision, apply vertical movement
                _position.Y = newPosition.Y;
            }
            else
            {
                // Moving upward, check for ceiling collision
                if (_collisionSystem.IsAreaSolid(verticalHitbox))
                {
                    _velocity.Y = 0; // Hit ceiling
                }
                else
                {
                    _position.Y = newPosition.Y;
                }
            }
        }

        private void CheckSurfaceInteractions()
        {
            List<SurfaceType> surfaceTypes = _collisionSystem.GetSurfaceTypesInArea(_hitbox);

            foreach (SurfaceType surface in surfaceTypes)
            {
                switch (surface)
                {
                    case SurfaceType.Hazard:
                        if (!_isInvulnerable)
                        {
                            TakeDamage();
                        }
                        break;

                    case SurfaceType.Water:
                        // Reduce movement speed in water
                        _velocity.X *= 0.7f;
                        _velocity.Y *= 0.8f; // Buoyancy effect
                        break;

                    case SurfaceType.Ice:
                        // Slippery movement on ice
                        _velocity.X *= 1.05f; // Less friction
                        break;

                    case SurfaceType.Lava:
                        if (!_isInvulnerable)
                        {
                            TakeDamage();
                        }
                        break;
                }
            }
        }

        private void TakeDamage()
        {
            System.Diagnostics.Debug.WriteLine("Player took damage!");
            _isInvulnerable = true;
            _invulnerabilityTimer = 0;

            // Knockback effect
            _velocity.Y = -100; // Small jump back
            _isOnGround = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);

            // Flash when invulnerable
            Color drawColor = _isInvulnerable && ((int)(_invulnerabilityTimer * 10) % 2 == 0) ?
                Color.Red : Color.White;

            spriteBatch.Draw(_texture, _position, sourceRect, drawColor);
        }

        // Method to draw hitbox for debugging
        public void DrawHitbox(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            spriteBatch.Draw(pixelTexture, _hitbox, Color.Red * 0.4f);
        }

        // Debug method to check colors under the character
        public void DebugColors()
        {
            Color bottomLeft = _collisionSystem.GetColorAt(_hitbox.Left, _hitbox.Bottom);
            Color bottomRight = _collisionSystem.GetColorAt(_hitbox.Right, _hitbox.Bottom);
            Color bottomCenter = _collisionSystem.GetColorAt(_hitbox.Center.X, _hitbox.Bottom);

            System.Diagnostics.Debug.WriteLine($"Colors under character - Left: {bottomLeft}, Center: {bottomCenter}, Right: {bottomRight}");
        }
    }
}
