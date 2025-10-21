using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class Player
    {
        // Textures / animation
        private Texture2D texture;
        private int frameWidth;
        private int frameHeight;
        private int frameCount = 4;
        private int currentFrame;
        private double animationTimer;
        private double animationInterval = 0.15;

        // Position / physics
        private Vector2 position;
        private Vector2 velocity;

        // Input
        private bool wasJumpPressed;
        private float inputMoveX;
        private bool inputJump;

        // Public tunables
        public float Gravity { get; set; } = 800f;
        public float JumpPower { get; set; } = 400f;
        public float MoveSpeed { get; set; } = 200f; // kept for compatibility
        public bool IsOnGround { get; set; }
        public int HitboxOffsetX { get; set; } = 10;
        public int HitboxOffsetY { get; set; } = 5;

        // Movement smoothing / coyote
        private float coyoteTimer = 0f;
        private const float COYOTE_TIME = 0.1f; // 100 ms forgiveness
        private const float ACCELERATION = 2000f;
        private const float DECELERATION = 2500f;
        private const float MAX_RUN_SPEED = 200f;

        // Animation helper
        private bool isMoving;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Player(Texture2D playerTexture, Vector2 startPosition)
        {
            texture = playerTexture ?? throw new ArgumentNullException(nameof(playerTexture));
            position = startPosition;
            velocity = Vector2.Zero;

            frameCount = 4;
            frameWidth = playerTexture.Width / frameCount;
            frameHeight = playerTexture.Height;

            // Defaults already set via properties/fields
            currentFrame = 0;
            animationTimer = 0;
            isMoving = false;
            IsOnGround = false;
            wasJumpPressed = false;
            inputMoveX = 0;
            inputJump = false;
        }

        public Rectangle GetHitbox()
        {
            return new Rectangle(
                (int)position.X + HitboxOffsetX,
                (int)position.Y + HitboxOffsetY,
                frameWidth - (HitboxOffsetX * 2),
                frameHeight - (HitboxOffsetY * 2)
            );
        }

        public void ProcessInput(KeyboardState keyboardState)
        {
            inputMoveX = 0;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                inputMoveX = 1;
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                inputMoveX = -1;

            bool jumpPressed = keyboardState.IsKeyDown(Keys.Space) ||
                              keyboardState.IsKeyDown(Keys.Up) ||
                              keyboardState.IsKeyDown(Keys.W);

            // edge-trigger jump detection
            if (jumpPressed && !wasJumpPressed)
                inputJump = true;
            else
                inputJump = false;

            wasJumpPressed = jumpPressed;
        }

        public void UpdatePhysics(float deltaTime, int screenWidth)
        {
            // --- COYOTE TIME HANDLING ---
            if (IsOnGround)
                coyoteTimer = COYOTE_TIME;
            else
                coyoteTimer -= deltaTime;

            // --- HORIZONTAL MOVEMENT (acceleration / deceleration) ---
            if (inputMoveX != 0)
            {
                velocity.X += inputMoveX * ACCELERATION * deltaTime;
                if (Math.Abs(velocity.X) > MAX_RUN_SPEED)
                    velocity.X = Math.Sign(velocity.X) * MAX_RUN_SPEED;
            }
            else
            {
                if (velocity.X > 0)
                {
                    velocity.X -= DECELERATION * deltaTime;
                    if (velocity.X < 0) velocity.X = 0;
                }
                else if (velocity.X < 0)
                {
                    velocity.X += DECELERATION * deltaTime;
                    if (velocity.X > 0) velocity.X = 0;
                }
            }

            // --- JUMPING (instant when on ground or within coyote time) ---
            if (inputJump && coyoteTimer > 0f)
            {
                velocity.Y = -JumpPower;
                IsOnGround = false;
                coyoteTimer = 0f;
                inputJump = false;
            }

            // --- GRAVITY ---
            if (!IsOnGround)
            {
                velocity.Y += Gravity * deltaTime;
                if (velocity.Y > 1000f)
                    velocity.Y = 1000f;
            }

            // --- POSITION UPDATE ---
            position += velocity * deltaTime;

            // Clamp horizontally inside screen
            position.X = MathHelper.Clamp(position.X, 0, screenWidth - frameWidth);

            // Snap to pixel grid to reduce subpixel jitter
            position.Y = (float)Math.Round(position.Y, 2);

            // Update movement flag for animation
            isMoving = (Math.Abs(velocity.X) > 1f) && IsOnGround;
        }

        public void CheckCollisions(List<Rectangle> platforms)
        {
            // We'll assume false and set true when we land
            IsOnGround = false;

            Rectangle hitbox = GetHitbox();
            Vector2 correction = Vector2.Zero;

            foreach (Rectangle platform in platforms)
            {
                if (hitbox.Intersects(platform))
                {
                    float overlapLeft = hitbox.Right - platform.Left;
                    float overlapRight = platform.Right - hitbox.Left;
                    float overlapTop = hitbox.Bottom - platform.Top;
                    float overlapBottom = platform.Bottom - hitbox.Top;
                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight),
                                                Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapTop && velocity.Y >= 0)
                    {
                        // Landed on platform
                        correction.Y = -overlapTop;
                        velocity.Y = 0;
                        IsOnGround = true;
                    }
                    else if (minOverlap == overlapBottom && velocity.Y < 0)
                    {
                        // Hit ceiling
                        correction.Y = overlapBottom;
                        velocity.Y = 0;
                    }
                    else if (minOverlap == overlapLeft && velocity.X > 0)
                    {
                        // Hit right side
                        correction.X = -overlapLeft;
                        velocity.X = 0;
                    }
                    else if (minOverlap == overlapRight && velocity.X < 0)
                    {
                        // Hit left side
                        correction.X = overlapRight;
                        velocity.X = 0;
                    }

                    // Apply correction immediately and refresh hitbox for stacked collisions
                    position += correction;
                    hitbox = GetHitbox();
                    correction = Vector2.Zero;
                }
            }

            // Ground stabilization: prevent micro-shaking
            if (IsOnGround)
            {
                velocity.Y = 0;
                position.Y = (float)Math.Round(position.Y + 0.5f);
            }
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            if (isMoving)
            {
                animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (animationTimer > animationInterval)
                {
                    currentFrame++;
                    if (currentFrame >= frameCount)
                        currentFrame = 0;
                    animationTimer = 0;
                }
            }
            else
            {
                currentFrame = 0;
                animationTimer = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            Vector2 drawPos = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));
            spriteBatch.Draw(texture, drawPos, sourceRect, Color.White);

        }

        public void DrawDebug(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Rectangle hitbox = GetHitbox();
            spriteBatch.Draw(pixel, hitbox, Color.Red * 0.5f);

            Rectangle spriteBounds = new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight);
            int t = 2;
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Y, spriteBounds.Width, t), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Bottom - t, spriteBounds.Width, t), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Y, t, spriteBounds.Height), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.Right - t, spriteBounds.Y, t, spriteBounds.Height), Color.Yellow);

            Color groundColor = IsOnGround ? Color.Lime : Color.Red;
            Rectangle groundIndicator = new Rectangle((int)position.X + frameWidth / 2 - 5, (int)position.Y - 10, 10, 10);
            spriteBatch.Draw(pixel, groundIndicator, groundColor);
        }

        // Optionally expose currentFrame etc if used elsewhere
        public int HitboxOffsetYPublic => HitboxOffsetY;
    }
}
