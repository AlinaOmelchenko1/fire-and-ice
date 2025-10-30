using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace fire_and_ice
{
    public enum SurfaceType
    {
        Empty = 0,        // Walkable air/transparent
        Solid = 1,        // Solid ground - normal walking
        Platform = 2,     // Jump-through platform (one-way collision)
        Hazard = 3,       // Generic hazard - damages player
        Water = 4,        // Water - slows movement, potential drowning
        Lava = 5,         // Lava - high damage fire hazard
        Ice = 6,          // Ice - slippery surface
        Bouncy = 7,       // Bouncy - bounce pads
        Sticky = 8,       // Sticky - slows horizontal movement
        Spike = 9,        // Spike - instant death or high damage
        Fire = 10         // Fire - damage over time
    }

    /// <summary>
    /// Represents an interactive object in the game world with a type and behavior
    /// </summary>
    public class InteractableObject
    {
        public Rectangle Bounds { get; set; }
        public SurfaceType Type { get; set; }
        public float DamageAmount { get; set; }
        public float InteractionCooldown { get; set; } // Time between interactions
        public bool IsOneWay { get; set; } // For platforms you can jump through

        public InteractableObject(Rectangle bounds, SurfaceType type, float damageAmount = 0f, float cooldown = 0f)
        {
            Bounds = bounds;
            Type = type;
            DamageAmount = damageAmount;
            InteractionCooldown = cooldown;
            IsOneWay = (type == SurfaceType.Platform);
        }

        /// <summary>
        /// Get the color for debug visualization
        /// </summary>
        public Color GetDebugColor()
        {
            return Type switch
            {
                SurfaceType.Solid => Color.Cyan * 0.4f,
                SurfaceType.Platform => Color.Green * 0.4f,
                SurfaceType.Hazard => Color.Red * 0.5f,
                SurfaceType.Water => Color.Blue * 0.5f,
                SurfaceType.Lava => Color.OrangeRed * 0.6f,
                SurfaceType.Ice => Color.LightBlue * 0.4f,
                SurfaceType.Bouncy => Color.Purple * 0.5f,
                SurfaceType.Sticky => Color.Brown * 0.5f,
                SurfaceType.Spike => Color.DarkRed * 0.7f,
                SurfaceType.Fire => Color.Yellow * 0.6f,
                _ => Color.Gray * 0.3f
            };
        }
    }

    /// <summary>
    /// Result of an interaction between player and object
    /// </summary>
    public struct InteractionResult
    {
        public bool ShouldApplyCollision { get; set; }
        public float DamageTaken { get; set; }
        public Vector2 VelocityModifier { get; set; }
        public float FrictionMultiplier { get; set; }
        public float BounceForce { get; set; }

        public static InteractionResult None => new InteractionResult
        {
            ShouldApplyCollision = false,
            DamageTaken = 0f,
            VelocityModifier = Vector2.Zero,
            FrictionMultiplier = 1f,
            BounceForce = 0f
        };

        public static InteractionResult Normal => new InteractionResult
        {
            ShouldApplyCollision = true,
            DamageTaken = 0f,
            VelocityModifier = Vector2.Zero,
            FrictionMultiplier = 1f,
            BounceForce = 0f
        };
    }

}