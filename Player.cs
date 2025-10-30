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

        // Health system
        public float Health { get; private set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public bool IsAlive => Health > 0f;
        private float _damageCooldown = 0f;
        private const float DAMAGE_COOLDOWN_TIME = 0.5f; // Half second invincibility after hit
        public bool IsInvincible => _damageCooldown > 0f;

        // Surface interaction modifiers
        private float _currentFrictionMultiplier = 1f;
        private SurfaceType _currentSurfaceType = SurfaceType.Empty;
        private float _bounceCooldown = 0f;
        private const float BOUNCE_COOLDOWN_TIME = 0.3f; // Prevent immediate re-bounce
        private const float MIN_BOUNCE_VELOCITY = 50f; // Minimum falling speed required to trigger bounce

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
            // --- DAMAGE COOLDOWN ---
            if (_damageCooldown > 0f)
                _damageCooldown -= deltaTime;

            // --- BOUNCE COOLDOWN ---
            if (_bounceCooldown > 0f)
                _bounceCooldown -= deltaTime;

            // --- COYOTE TIME HANDLING ---
            if (IsOnGround)
                coyoteTimer = COYOTE_TIME;
            else
                coyoteTimer -= deltaTime;

            // --- HORIZONTAL MOVEMENT (acceleration / deceleration with surface friction) ---
            float effectiveAcceleration = ACCELERATION * _currentFrictionMultiplier;
            float effectiveDeceleration = DECELERATION * _currentFrictionMultiplier;

            if (inputMoveX != 0)
            {
                velocity.X += inputMoveX * effectiveAcceleration * deltaTime;
                if (Math.Abs(velocity.X) > MAX_RUN_SPEED)
                    velocity.X = Math.Sign(velocity.X) * MAX_RUN_SPEED;
            }
            else
            {
                if (velocity.X > 0)
                {
                    velocity.X -= effectiveDeceleration * deltaTime;
                    if (velocity.X < 0) velocity.X = 0;
                }
                else if (velocity.X < 0)
                {
                    velocity.X += effectiveDeceleration * deltaTime;
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
                System.Diagnostics.Debug.WriteLine($"JUMP! Position: {position.Y:F2}, Velocity: {velocity.Y:F2}");
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

            // Update movement flag for animation
            isMoving = (Math.Abs(velocity.X) > 1f) && IsOnGround;
        }

        public void CheckCollisions(List<InteractableObject> objects)
        {
            IsOnGround = false;
            _currentFrictionMultiplier = 1f;
            _currentSurfaceType = SurfaceType.Empty;

            Rectangle hitbox = GetHitbox();
            Vector2 correction = Vector2.Zero;
            bool shouldBounce = false;
            float bounceForce = 0f;

            foreach (InteractableObject obj in objects)
            {
                if (hitbox.Intersects(obj.Bounds))
                {
                    // Calculate interaction result based on object type
                    InteractionResult interaction = CalculateInteraction(obj);

                    // Handle damage if applicable
                    if (interaction.DamageTaken > 0f && !IsInvincible)
                    {
                        TakeDamage(interaction.DamageTaken);
                    }

                    // Apply velocity modifiers (but NOT bounce yet)
                    velocity += interaction.VelocityModifier;

                    // Only apply collision if specified
                    if (interaction.ShouldApplyCollision)
                    {
                        float overlapLeft = hitbox.Right - obj.Bounds.Left;
                        float overlapRight = obj.Bounds.Right - hitbox.Left;
                        float overlapTop = hitbox.Bottom - obj.Bounds.Top;
                        float overlapBottom = obj.Bounds.Bottom - hitbox.Top;
                        float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight),
                                                    Math.Min(overlapTop, overlapBottom));

                        if (minOverlap == overlapTop && velocity.Y >= 0)
                        {
                            // Store the landing velocity before zeroing it
                            float landingVelocity = velocity.Y;

                            // Landed on platform
                            correction.Y = -overlapTop;
                            velocity.Y = 0;
                            IsOnGround = true;
                            _currentSurfaceType = obj.Type;
                            _currentFrictionMultiplier = interaction.FrictionMultiplier;

                            // Check if we should bounce (apply AFTER collision resolution)
                            // Only bounce if: 1) cooldown expired, 2) was actually falling with sufficient speed
                            if (interaction.BounceForce > 0f &&
                                _bounceCooldown <= 0f &&
                                landingVelocity >= MIN_BOUNCE_VELOCITY)
                            {
                                shouldBounce = true;
                                bounceForce = interaction.BounceForce;
                            }
                        }
                        else if (minOverlap == overlapBottom && velocity.Y < 0 && !obj.IsOneWay)
                        {
                            // Hit ceiling (not for one-way platforms)
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
            }

            // Apply bounce force AFTER all collision resolution
            if (shouldBounce)
            {
                velocity.Y = -bounceForce;
                IsOnGround = false; // Player is launching, not on ground anymore
                _bounceCooldown = BOUNCE_COOLDOWN_TIME; // Set cooldown to prevent immediate re-bounce
            }

            // Ground stabilization: prevent micro-shaking
            if (IsOnGround)
            {
                velocity.Y = 0;
                position.Y = (float)Math.Round(position.Y);
            }
        }

        /// <summary>
        /// Calculate interaction effects based on object type
        /// </summary>
        private InteractionResult CalculateInteraction(InteractableObject obj)
        {
            switch (obj.Type)
            {
                case SurfaceType.Solid:
                    return InteractionResult.Normal;

                case SurfaceType.Platform:
                    return InteractionResult.Normal;

                case SurfaceType.Ice:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = true,
                        DamageTaken = 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = 0.2f, // Very slippery
                        BounceForce = 0f
                    };

                case SurfaceType.Sticky:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = true,
                        DamageTaken = 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = 3f, // Hard to move
                        BounceForce = 0f
                    };

                case SurfaceType.Bouncy:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = true,
                        DamageTaken = 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = 1f,
                        BounceForce = 500f // Bounce up
                    };

                case SurfaceType.Fire:
                case SurfaceType.Lava:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false, // Can walk through
                        DamageTaken = obj.DamageAmount > 0 ? obj.DamageAmount : 10f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = 1f,
                        BounceForce = 0f
                    };

                case SurfaceType.Spike:
                case SurfaceType.Hazard:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false,
                        DamageTaken = obj.DamageAmount > 0 ? obj.DamageAmount : 25f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = 1f,
                        BounceForce = 0f
                    };

                case SurfaceType.Water:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false,
                        DamageTaken = 0f,
                        VelocityModifier = new Vector2(velocity.X * -0.5f, velocity.Y * -0.3f), // Slow down
                        FrictionMultiplier = 0.5f,
                        BounceForce = 0f
                    };

                default:
                    return InteractionResult.None;
            }
        }

        /// <summary>
        /// Apply damage to player
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsInvincible || !IsAlive)
                return;

            Health -= amount;
            if (Health < 0f)
                Health = 0f;

            _damageCooldown = DAMAGE_COOLDOWN_TIME;

            System.Diagnostics.Debug.WriteLine($"Player took {amount} damage! Health: {Health}/{MaxHealth}");
        }

        /// <summary>
        /// Heal player
        /// </summary>
        public void Heal(float amount)
        {
            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        /// <summary>
        /// Reset player health
        /// </summary>
        public void ResetHealth()
        {
            Health = MaxHealth;
            _damageCooldown = 0f;
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

            // Flash red when taking damage
            Color drawColor = IsInvincible ? Color.Red : Color.White;

            // Flash effect - alternate visibility when invincible
            if (IsInvincible && ((int)(_damageCooldown * 20) % 2 == 0))
                drawColor = Color.White * 0.5f;

            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor);
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
