using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class CollisionProtagonist
    {
        // Initialise attributes
        private Texture2D _texture;
        private Vector2 _position;
        private int _frameWidth, _frameHeight, _frameCount;
        private int _currentFrame;
        private double _timer;
        private double _interval = 0.15; // seconds per frame from tutorial??
        private bool _isMoving; // Track if character is moving from tutorial??
        private float _speed = 200f; // Movement speed in pixels per second from tutorial??

        // Physics and collision
        private Vector2 _velocity;
        private float _gravity = 600f; // Gravity strength
        private float _jumpPower = 350f; // Jump strength
        private bool _isOnGround; //to check if on the ground for collision 
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


//CHANGRE TO GET METHOD 
        public Vector2 Position => _position;
        public Rectangle Hitbox => _hitbox;
        public bool IsOnGround => _isOnGround;


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

            // Horizontal movement input
            Vector2 movement = Vector2.Zero;
            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                movement.X += 1f;
            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                movement.X -= 1f;

            // Jump input only if on ground and key just pressed
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

            
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
        }

        // Method to draw coloured hitbox for debugging dads advice dont delete 
        public void DrawHitbox(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            spriteBatch.Draw(pixelTexture, _hitbox, Color.Red * 0.4f);
        }

        
    }
}
