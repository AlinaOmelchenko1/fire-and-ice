using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace fire_and_ice.Systems
{
    /// <summary>
    /// Represents a physical entity that can be affected by physics
    /// </summary>
    public interface IPhysicsEntity
    {
        /// <summary>
        /// Gets or sets the position of the entity
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the entity
        /// </summary>
        Vector2 Velocity { get; set; }

        /// <summary>
        /// Gets or sets whether gravity should be applied to this entity
        /// </summary>
        bool ApplyGravity { get; set; }

        /// <summary>
        /// Gets or sets the mass of the entity (affects physics calculations)
        /// </summary>
        float Mass { get; set; }
    }

    /// <summary>
    /// Manages physics calculations and updates for all physics-enabled entities.
    /// Centralizes gravity, velocity, acceleration, and movement logic.
    /// Supports fixed timestep updates for deterministic physics.
    /// </summary>
    public class PhysicsSystem
    {
        private readonly List<IPhysicsEntity> _entities;
        private float _gravity;
        private float _maxFallSpeed;
        private float _airResistance;

        /// <summary>
        /// Gets or sets the gravity force applied to entities (pixels per second squared)
        /// </summary>
        public float Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        /// <summary>
        /// Gets or sets the maximum fall speed (terminal velocity)
        /// </summary>
        public float MaxFallSpeed
        {
            get => _maxFallSpeed;
            set => _maxFallSpeed = value;
        }

        /// <summary>
        /// Gets or sets the air resistance factor (0 = no resistance, 1 = full resistance)
        /// </summary>
        public float AirResistance
        {
            get => _airResistance;
            set => _airResistance = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Gets the number of registered physics entities
        /// </summary>
        public int EntityCount => _entities.Count;

        /// <summary>
        /// Creates a new physics system with default gravity
        /// </summary>
        public PhysicsSystem()
            : this(GameConstants.Physics.Gravity)
        {
        }

        /// <summary>
        /// Creates a new physics system with specified gravity
        /// </summary>
        /// <param name="gravity">Gravity force in pixels per second squared</param>
        public PhysicsSystem(float gravity)
        {
            _entities = new List<IPhysicsEntity>();
            _gravity = gravity;
            _maxFallSpeed = GameConstants.Physics.MaxFallSpeed;
            _airResistance = 0f; // No air resistance by default
        }

        /// <summary>
        /// Registers an entity to be affected by physics
        /// </summary>
        /// <param name="entity">The physics entity to register</param>
        public void Register(IPhysicsEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }

        /// <summary>
        /// Unregisters an entity from physics updates
        /// </summary>
        /// <param name="entity">The physics entity to unregister</param>
        public void Unregister(IPhysicsEntity entity)
        {
            if (entity != null)
            {
                _entities.Remove(entity);
            }
        }

        /// <summary>
        /// Clears all registered entities
        /// </summary>
        public void Clear()
        {
            _entities.Clear();
        }

        /// <summary>
        /// Updates physics for all registered entities using variable timestep
        /// </summary>
        /// <param name="gameTime">Game time information</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Update(deltaTime);
        }

        /// <summary>
        /// Updates physics for all registered entities using fixed timestep.
        /// This is the preferred method for deterministic physics.
        /// </summary>
        /// <param name="deltaTime">Fixed time step in seconds</param>
        public void Update(float deltaTime)
        {
            foreach (var entity in _entities)
            {
                if (entity != null)
                {
                    UpdateEntity(entity, deltaTime);
                }
            }
        }

        /// <summary>
        /// Updates physics for a single entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="deltaTime">Time step in seconds</param>
        private void UpdateEntity(IPhysicsEntity entity, float deltaTime)
        {
            Vector2 velocity = entity.Velocity;

            // Apply gravity if enabled
            if (entity.ApplyGravity)
            {
                velocity.Y += _gravity * deltaTime;

                // Clamp to max fall speed (terminal velocity)
                if (velocity.Y > _maxFallSpeed)
                {
                    velocity.Y = _maxFallSpeed;
                }
            }

            // Apply air resistance
            if (_airResistance > 0f)
            {
                velocity *= (1f - _airResistance * deltaTime);
            }

            // Update velocity
            entity.Velocity = velocity;

            // Update position based on velocity
            entity.Position += velocity * deltaTime;
        }

        /// <summary>
        /// Applies an impulse force to an entity (instantaneous velocity change)
        /// </summary>
        /// <param name="entity">The entity to apply force to</param>
        /// <param name="impulse">The impulse vector</param>
        public void ApplyImpulse(IPhysicsEntity entity, Vector2 impulse)
        {
            if (entity == null)
                return;

            // F = ma, so impulse / mass = velocity change
            Vector2 velocityChange = impulse / entity.Mass;
            entity.Velocity += velocityChange;
        }

        /// <summary>
        /// Applies a continuous force to an entity over time
        /// </summary>
        /// <param name="entity">The entity to apply force to</param>
        /// <param name="force">The force vector</param>
        /// <param name="deltaTime">Time step in seconds</param>
        public void ApplyForce(IPhysicsEntity entity, Vector2 force, float deltaTime)
        {
            if (entity == null)
                return;

            // F = ma, so a = F / m
            Vector2 acceleration = force / entity.Mass;
            entity.Velocity += acceleration * deltaTime;
        }

        /// <summary>
        /// Sets the velocity of an entity directly
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="velocity">The new velocity</param>
        public void SetVelocity(IPhysicsEntity entity, Vector2 velocity)
        {
            if (entity != null)
            {
                entity.Velocity = velocity;
            }
        }

        /// <summary>
        /// Stops an entity's movement (sets velocity to zero)
        /// </summary>
        /// <param name="entity">The entity to stop</param>
        public void Stop(IPhysicsEntity entity)
        {
            if (entity != null)
            {
                entity.Velocity = Vector2.Zero;
            }
        }

        /// <summary>
        /// Applies friction to an entity's horizontal movement
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="frictionCoefficient">Friction coefficient (0 = no friction, higher = more friction)</param>
        /// <param name="deltaTime">Time step in seconds</param>
        public void ApplyFriction(IPhysicsEntity entity, float frictionCoefficient, float deltaTime)
        {
            if (entity == null || frictionCoefficient <= 0f)
                return;

            Vector2 velocity = entity.Velocity;

            // Apply friction to horizontal movement
            float frictionForce = frictionCoefficient * Math.Abs(velocity.X);
            float frictionDeceleration = frictionForce * deltaTime;

            if (Math.Abs(velocity.X) > frictionDeceleration)
            {
                velocity.X -= Math.Sign(velocity.X) * frictionDeceleration;
            }
            else
            {
                velocity.X = 0f;
            }

            entity.Velocity = velocity;
        }

        /// <summary>
        /// Clamps entity velocity to specified maximum values
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="maxVelocityX">Maximum horizontal velocity (absolute value)</param>
        /// <param name="maxVelocityY">Maximum vertical velocity (absolute value)</param>
        public void ClampVelocity(IPhysicsEntity entity, float maxVelocityX, float maxVelocityY)
        {
            if (entity == null)
                return;

            Vector2 velocity = entity.Velocity;
            velocity.X = MathHelper.Clamp(velocity.X, -maxVelocityX, maxVelocityX);
            velocity.Y = MathHelper.Clamp(velocity.Y, -maxVelocityY, maxVelocityY);
            entity.Velocity = velocity;
        }

        /// <summary>
        /// Calculates the kinetic energy of an entity (0.5 * mass * velocity²)
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>Kinetic energy value</returns>
        public float CalculateKineticEnergy(IPhysicsEntity entity)
        {
            if (entity == null)
                return 0f;

            float velocitySquared = entity.Velocity.LengthSquared();
            return 0.5f * entity.Mass * velocitySquared;
        }

        /// <summary>
        /// Checks if an entity is moving
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <param name="threshold">Minimum velocity magnitude to consider as moving (default 0.1)</param>
        /// <returns>True if entity velocity exceeds threshold</returns>
        public bool IsMoving(IPhysicsEntity entity, float threshold = 0.1f)
        {
            if (entity == null)
                return false;

            return entity.Velocity.LengthSquared() > threshold * threshold;
        }

        /// <summary>
        /// Calculates the distance an entity will travel in the next time step
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="deltaTime">Time step in seconds</param>
        /// <returns>Predicted position delta</returns>
        public Vector2 PredictMovement(IPhysicsEntity entity, float deltaTime)
        {
            if (entity == null)
                return Vector2.Zero;

            return entity.Velocity * deltaTime;
        }

        /// <summary>
        /// Gets all entities currently managed by this physics system
        /// </summary>
        /// <returns>Read-only list of physics entities</returns>
        public IReadOnlyList<IPhysicsEntity> GetEntities()
        {
            return _entities.AsReadOnly();
        }
    }
}
