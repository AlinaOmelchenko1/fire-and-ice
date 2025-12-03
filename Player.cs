using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using fire_and_ice.Interfaces;

namespace fire_and_ice
{
    /// <summary>
    /// Defines the player character type (Fire or Ice)
    /// </summary>
    public enum PlayerType
    {
        Fire,
        Ice
    }

    /// <summary>
    /// Represents a player character with physics, animation, input handling, and collision detection.
    /// Supports different player types (Fire/Ice) with type-specific hazard interactions.
    /// </summary>
    public class Player : IGameObject, ICollidable
    {
        // Player type (Fire or Ice)
        public PlayerType Type { get; set; } = PlayerType.Fire;

        // Textures / animation
        private Texture2D texture;
        private int frameWidth;
        private int frameHeight;
        private int frameCount = GameConstants.Player.DefaultFrameCount;
        private int currentFrame;
        private double animationTimer;
        private double animationInterval = GameConstants.Player.AnimationInterval;

        // Player color
        public Color PlayerColor { get; set; } = Color.White;

        // Position / physics
        private Vector2 position;
        private Vector2 velocity;

        // Input
        private bool wasJumpPressed;
        private float inputMoveX;
        private bool inputJump;

        // Control keys (configurable)
        public Keys MoveLeftKey { get; set; } = Keys.A;
        public Keys MoveRightKey { get; set; } = Keys.D;
        public Keys JumpKey1 { get; set; } = Keys.Space;
        public Keys JumpKey2 { get; set; } = Keys.W;
        public Keys JumpKey3 { get; set; } = Keys.Up;

        // Public tunables
        public float Gravity { get; set; } = GameConstants.Physics.Gravity;
        public float JumpPower { get; set; } = GameConstants.Physics.JumpPower;
        public float MoveSpeed { get; set; } = GameConstants.Physics.MaxRunSpeed; // kept for compatibility
        public bool IsOnGround { get; set; }
        public int HitboxOffsetX { get; set; } = GameConstants.Player.HitboxOffsetX;
        public int HitboxOffsetY { get; set; } = GameConstants.Player.HitboxOffsetY;

        // Health system
        public float Health { get; private set; } = GameConstants.Player.DefaultHealth;
        public float MaxHealth { get; set; } = GameConstants.Player.DefaultHealth;
        public bool IsAlive => Health > 0f;
        private float _damageCooldown = 0f;
        public bool IsInvincible => _damageCooldown > 0f;

        // Surface interaction modifiers
        private float _currentFrictionMultiplier = 1f;
        private SurfaceType _currentSurfaceType = SurfaceType.Empty;
        private float _bounceCooldown = 0f;

        // Movement smoothing / coyote
        private float coyoteTimer = 0f;

        // Animation helper
        private bool isMoving;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Player(Texture2D playerTexture, Vector2 startPosition, int animationFrameCount = 4)
        {
            texture = playerTexture ?? throw new ArgumentNullException(nameof(playerTexture));
            position = startPosition;
            velocity = Vector2.Zero;

            frameCount = animationFrameCount;
            frameWidth = playerTexture.Width / frameCount;
            frameHeight = playerTexture.Height;

            System.Diagnostics.Debug.WriteLine($"Player created: texture={playerTexture.Width}x{playerTexture.Height}, frames={frameCount}, frameSize={frameWidth}x{frameHeight}");

            // Defaults already set via properties/fields
            currentFrame = 0;
            animationTimer = 0;
            isMoving = false;
            IsOnGround = false;
            wasJumpPressed = false;
            inputMoveX = 0;
            inputJump = false;
        }

        /// <summary>
        /// Gets the player's collision hitbox (smaller than sprite bounds)
        /// </summary>
        public Rectangle GetHitbox()
        {
            return new Rectangle(
                (int)position.X + HitboxOffsetX,
                (int)position.Y + HitboxOffsetY,
                frameWidth - (HitboxOffsetX * 2),
                frameHeight - (HitboxOffsetY * 2)
            );
        }

        /// <summary>
        /// Gets the bounds for collision detection (ICollidable interface implementation)
        /// </summary>
        public Rectangle GetBounds()
        {
            return GetHitbox();
        }

        /// <summary>
        /// Checks collision with another collidable object (ICollidable interface implementation)
        /// </summary>
        public bool CheckCollision(ICollidable other)
        {
            return GetBounds().Intersects(other.GetBounds());
        }

        /// <summary>
        /// Main update method (IGameObject interface implementation)
        /// Note: For Player, input and physics are separated and called externally by Game1
        /// to support different control schemes and collision systems
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // For Player class, Update, Physics, and Animation are kept separate
            // to allow Game1 to control the update order and collision system
            // This method exists to satisfy IGameObject but delegates to specialized methods
            UpdateAnimation(gameTime);
        }

        /// <summary>
        /// Processes keyboard input for player movement and jumping
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        public void ProcessInput(KeyboardState keyboardState)
        {
            inputMoveX = 0;
            if (keyboardState.IsKeyDown(MoveRightKey))
                inputMoveX = 1;
            if (keyboardState.IsKeyDown(MoveLeftKey))
                inputMoveX = -1;

            bool jumpPressed = keyboardState.IsKeyDown(JumpKey1) ||
                              keyboardState.IsKeyDown(JumpKey2) ||
                              keyboardState.IsKeyDown(JumpKey3);

            // edge-trigger jump detection
            if (jumpPressed && !wasJumpPressed)
                inputJump = true;
            else
                inputJump = false;

            wasJumpPressed = jumpPressed;
        }

        /// <summary>
        /// Updates player physics including movement, gravity, jumping, and collisions
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update in seconds</param>
        /// <param name="screenWidth">Width of the game screen for boundary clamping</param>
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
                coyoteTimer = GameConstants.Physics.CoyoteTime;
            else
                coyoteTimer -= deltaTime;

            // --- HORIZONTAL MOVEMENT (acceleration / deceleration with surface friction) ---
            float effectiveAcceleration = GameConstants.Physics.Acceleration * _currentFrictionMultiplier;
            float effectiveDeceleration = GameConstants.Physics.Deceleration * _currentFrictionMultiplier;

            if (inputMoveX != 0)
            {
                velocity.X += inputMoveX * effectiveAcceleration * deltaTime;
                if (Math.Abs(velocity.X) > GameConstants.Physics.MaxRunSpeed)
                    velocity.X = Math.Sign(velocity.X) * GameConstants.Physics.MaxRunSpeed;
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
            }

            // --- GRAVITY ---
            if (!IsOnGround)
            {
                velocity.Y += Gravity * deltaTime;
                if (velocity.Y > GameConstants.Physics.MaxFallSpeed)
                    velocity.Y = GameConstants.Physics.MaxFallSpeed;
            }

            // --- POSITION UPDATE ---
            position += velocity * deltaTime;

            // Clamp horizontally inside screen
            position.X = MathHelper.Clamp(position.X, 0, screenWidth - frameWidth);

            // Update movement flag for animation
            isMoving = (Math.Abs(velocity.X) > 1f) && IsOnGround;
        }

        /// <summary>
        /// Checks and resolves collisions with interactable objects (platforms, hazards, etc.)
        /// </summary>
        /// <param name="objects">List of interactable objects to check against</param>
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
                                landingVelocity >= GameConstants.Surface.MinBounceVelocity)
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
                _bounceCooldown = GameConstants.Surface.BounceCooldownTime; // Set cooldown to prevent immediate re-bounce
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
                        FrictionMultiplier = GameConstants.Surface.IceFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.Sticky:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = true,
                        DamageTaken = 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.StickyFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.Bouncy:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = true,
                        DamageTaken = 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.NormalFriction,
                        BounceForce = GameConstants.Surface.BouncyForce
                    };

                case SurfaceType.Fire:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false, // Can walk through
                        DamageTaken = obj.DamageAmount > 0 ? obj.DamageAmount : GameConstants.Damage.FireHazard,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.NormalFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.Lava:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false, // Can walk through
                        DamageTaken = obj.DamageAmount > 0 ? obj.DamageAmount : GameConstants.Damage.Lava,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.NormalFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.IceHazard:
                    // Only damages Fire character, Ice character can walk through safely
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false, // Can walk through
                        DamageTaken = (Type == PlayerType.Fire) ? (obj.DamageAmount > 0 ? obj.DamageAmount : GameConstants.Damage.IceHazard) : 0f,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.NormalFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.Spike:
                case SurfaceType.Hazard:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false,
                        DamageTaken = obj.DamageAmount > 0 ? obj.DamageAmount : GameConstants.Damage.Spike,
                        VelocityModifier = Vector2.Zero,
                        FrictionMultiplier = GameConstants.Surface.NormalFriction,
                        BounceForce = 0f
                    };

                case SurfaceType.Water:
                    return new InteractionResult
                    {
                        ShouldApplyCollision = false,
                        DamageTaken = 0f,
                        VelocityModifier = new Vector2(velocity.X * GameConstants.Water.HorizontalDrag, velocity.Y * GameConstants.Water.VerticalDrag),
                        FrictionMultiplier = GameConstants.Water.FrictionMultiplier,
                        BounceForce = 0f
                    };

                default:
                    return InteractionResult.None;
            }
        }

        /// <summary>
        /// Apply damage to player with invincibility period
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        public void TakeDamage(float amount)
        {
            if (IsInvincible || !IsAlive)
                return;

            Health -= amount;
            if (Health < 0f)
                Health = 0f;

            _damageCooldown = GameConstants.Player.DamageCooldownTime;

            System.Diagnostics.Debug.WriteLine($"Player took {amount} damage! Health: {Health}/{MaxHealth}");
        }

        /// <summary>
        /// Heals the player by the specified amount
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        /// <summary>
        /// Resets player health to maximum and clears damage cooldown
        /// </summary>
        public void ResetHealth()
        {
            Health = MaxHealth;
            _damageCooldown = 0f;
        }

        /// <summary>
        /// Resets all player state to initial values (position, velocity, health, cooldowns)
        /// </summary>
        /// <param name="spawnPosition">Position to spawn the player at</param>
        public void Reset(Vector2 spawnPosition)
        {
            position = spawnPosition;
            velocity = Vector2.Zero;
            Health = MaxHealth;
            _damageCooldown = 0f;
            _bounceCooldown = 0f;
            coyoteTimer = 0f;
            IsOnGround = false;
            inputJump = false;
            wasJumpPressed = false;
            currentFrame = 0;
            animationTimer = 0;
        }

        /// <summary>
        /// Updates the player's walking animation based on movement
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
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

        /// <summary>
        /// Draws the player to the screen with animation and damage effects
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            Vector2 drawPos = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));

            // Flash red when taking damage, otherwise use player color
            Color drawColor = IsInvincible ? Color.Red : PlayerColor;

            // Flash effect - alternate visibility when invincible
            if (IsInvincible && ((int)(_damageCooldown * GameConstants.Player.InvincibilityFlashRate) % 2 == 0))
                drawColor = PlayerColor * GameConstants.Opacity.Medium;

            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor);
        }

        /// <summary>
        /// Draws debug information (hitbox, sprite bounds, ground indicator)
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="pixel">A 1x1 white pixel texture for drawing shapes</param>
        public void DrawDebug(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Draw hitbox in red
            Rectangle hitbox = GetHitbox();
            spriteBatch.Draw(pixel, hitbox, Color.Red * GameConstants.Opacity.Medium);

            // Draw sprite bounds in yellow
            Rectangle spriteBounds = new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight);
            int thickness = GameConstants.Debug.HitboxBorderThickness;
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Y, spriteBounds.Width, thickness), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Bottom - thickness, spriteBounds.Width, thickness), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.X, spriteBounds.Y, thickness, spriteBounds.Height), Color.Yellow);
            spriteBatch.Draw(pixel, new Rectangle(spriteBounds.Right - thickness, spriteBounds.Y, thickness, spriteBounds.Height), Color.Yellow);

            // Draw ground indicator (green when on ground, red when in air)
            Color groundColor = IsOnGround ? Color.Lime : Color.Red;
            Rectangle groundIndicator = new Rectangle(
                (int)position.X + frameWidth / 2 + GameConstants.Debug.GroundIndicatorOffsetX,
                (int)position.Y + GameConstants.Debug.GroundIndicatorOffsetY,
                GameConstants.Debug.GroundIndicatorSize,
                GameConstants.Debug.GroundIndicatorSize
            );
            spriteBatch.Draw(pixel, groundIndicator, groundColor);
        }

        // Optionally expose currentFrame etc if used elsewhere
        public int HitboxOffsetYPublic => HitboxOffsetY;
    }
}
