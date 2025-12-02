using Microsoft.Xna.Framework;

namespace fire_and_ice
{
    /// <summary>
    /// Centralized constants for the Fire and Ice game
    /// </summary>
    public static class GameConstants
    {
        // ===== SCREEN AND DISPLAY =====
        public static class Screen
        {
            // Screen dimensions are determined by GraphicsDevice at runtime
            // These are typical expected dimensions
            public const int ExpectedWidth = 800;
            public const int ExpectedHeight = 600;
        }

        // ===== PLAYER PHYSICS =====
        public static class Physics
        {
            public const float Gravity = 800f;
            public const float JumpPower = 400f;
            public const float MaxRunSpeed = 200f;
            public const float Acceleration = 2000f;
            public const float Deceleration = 2500f;
            public const float MaxFallSpeed = 1000f;
            public const float CoyoteTime = 0.1f; // seconds - grace period for jumping after leaving platform
        }

        // ===== PLAYER ATTRIBUTES =====
        public static class Player
        {
            public const float DefaultHealth = 100f;
            public const float DamageCooldownTime = 0.5f; // seconds of invincibility after taking damage
            public const int DefaultFrameCount = 4;
            public const int HitboxOffsetX = 28;
            public const int HitboxOffsetY = 16;
            public const float AnimationInterval = 0.15f; // seconds per frame
            public const int InvincibilityFlashRate = 20; // flashes per second when invincible
        }

        // ===== PLAYER SPAWN POSITIONS =====
        public static class SpawnPositions
        {
            public static readonly Vector2 Player1Start = new Vector2(85, 270);
            public static readonly Vector2 Player2Start = new Vector2(700, 270);
            public static readonly Vector2 Key1Position = new Vector2(65, 315);
            public static readonly Vector2 Key2Position = new Vector2(625, 305);
            public static readonly Vector2 Door1Position = new Vector2(5, 65);
            public static readonly Vector2 Door2Position = new Vector2(737, 65);
        }

        // ===== SURFACE PHYSICS =====
        public static class Surface
        {
            public const float IceFriction = 0.2f; // Very slippery
            public const float StickyFriction = 3f; // Hard to move
            public const float NormalFriction = 1f;
            public const float BouncyForce = 500f;
            public const float BounceCooldownTime = 0.3f; // Prevent immediate re-bounce
            public const float MinBounceVelocity = 50f; // Minimum falling speed to trigger bounce
        }

        // ===== DAMAGE VALUES =====
        public static class Damage
        {
            public const float FireHazard = 10f;
            public const float IceHazard = 10f;
            public const float Spike = 25f;
            public const float Lava = 10f;
        }

        // ===== WATER PHYSICS =====
        public static class Water
        {
            public const float HorizontalDrag = -0.5f;
            public const float VerticalDrag = -0.3f;
            public const float FrictionMultiplier = 0.5f;
        }

        // ===== KEY OBJECT =====
        public static class Key
        {
            public const int Size = 20;
            public const float BobSpeed = 3f;
            public const float BobAmount = 5f;
            public const float IconScale = 0.7f;

            // Key drawing offsets
            public const int BodyOffsetX = 5;
            public const int BodyOffsetY = 5;
            public const int BodyWidth = 10;
            public const int BodyHeight = 12;
            public const int HeadOffsetX = 7;
            public const int HeadOffsetY = 2;
            public const int HeadSize = 6;
            public const int TeethOffsetY = 15;
            public const int TeethSize = 3;
            public const int TeethSpacing = 5;
        }

        // ===== DOOR OBJECT =====
        public static class Door
        {
            public const int Width = 48;
            public const int Height = 50;
            public const int BarCount = 7;
            public const int BarWidth = 4;
            public const float OpenSpeed = 1.5f; // Speed of opening animation
            public const int HorizontalBarHeight = 4;
            public const int HorizontalBarCount = 3;
        }

        // ===== FLAME ANIMATION =====
        public static class Flame
        {
            public const float FlickerSpeed = 8f;
            public const float PulseSpeed = 3f;

            // Layer size multipliers
            public const float OuterLayerWidth = 1.0f;
            public const float OuterLayerHeight = 1.0f;
            public const float MiddleLayerWidth = 0.75f;
            public const float MiddleLayerHeight = 0.85f;
            public const float InnerLayerWidth = 0.6f;
            public const float InnerLayerHeight = 0.7f;
            public const float CoreLayerWidth = 0.4f;
            public const float CoreLayerHeight = 0.55f;
            public const float WhiteCoreSize = 0.25f;

            // Animation parameters
            public const float BaseIntensity = 0.7f;
            public const float PulseIntensity = 0.2f;
            public const float FlickerIntensity = 0.3f;
            public const float RandomFlickerIntensity = 0.2f;
            public const float MinIntensity = 0.5f;
            public const float MaxIntensity = 1.0f;

            // Drawing
            public const int SegmentHeight = 3;
            public const int TipCount = 2;
        }

        // ===== ICE SHARD ANIMATION =====
        public static class IceShard
        {
            public const float GlowSpeed = 2f;
            public const float ShimmerSpeed = 5f;

            // Layer size multipliers
            public const float OuterLayerWidth = 1.0f;
            public const float OuterLayerHeight = 1.0f;
            public const float MiddleLayerWidth = 0.7f;
            public const float MiddleLayerHeight = 0.9f;
            public const float InnerLayerWidth = 0.5f;
            public const float InnerLayerHeight = 0.75f;
            public const float CoreLayerWidth = 0.3f;
            public const float CoreLayerHeight = 0.6f;

            // Animation parameters
            public const float BaseIntensity = 0.8f;
            public const float GlowIntensity = 0.15f;
            public const float ShimmerIntensity = 0.2f;
            public const float MinIntensity = 0.7f;
            public const float MaxIntensity = 1.0f;

            // Sparkle effect
            public const int SparkleSize = 4;
            public const float SparkleThreshold = 0.7f;
            public const float SparkleTipOffset = 0.2f;

            // Shimmer lines
            public const float ShimmerLineHeight = 0.6f;
            public const float ShimmerSideOffset = 0.3f;

            // Drawing
            public const int SegmentHeight = 2;
            public const float TipHeightMultiplier = 0.15f;
            public const float TipWidthMultiplier = 0.2f;
        }

        // ===== GAME STATE TIMERS =====
        public static class GameState
        {
            public const float GameOverDelay = 2f; // Show game over for 2 seconds before allowing restart
            public const float VictoryDisplayTime = 5f; // Show victory screen for 5 seconds
        }

        // ===== MENU SETTINGS =====
        public static class Menu
        {
            public const int MainMenuOptionsCount = 2;
            public const int PauseMenuOptionsCount = 2;
            public const int MenuWidth = 300;
            public const int MenuHeight = 200;
            public const int BorderThickness = 3;
            public const int OptionSpacing = 50;
            public const int FirstOptionY = 90;

            // Fallback menu (when no font available)
            public const int BlockHeight = 40;
            public const int BlockWidth = 200;
            public const int BlockSpacing = 60;
            public const int FirstBlockY = 80;
        }

        // ===== UI SETTINGS =====
        public static class UI
        {
            public const int HealthBarWidth = 150;
            public const int HealthBarHeight = 20;
            public const int HealthBarMargin = 10;
            public const int Player1HealthY = 10;
            public const int Player2HealthY = 35;
            public const int KeyIconOffset = 30;

            // Text scales
            public const float TitleTextScale = 2.5f;
            public const float GameOverTextScale = 3f;
            public const float MenuOptionScale = 1.5f;
            public const float SubtitleScale = 1.5f;
            public const float NormalTextScale = 1f;

            // Text positions
            public const int ControlsY = 10;
            public const int Player2ControlsY = 30;
            public const int LegendStartY = 70;
            public const int LegendSpacing = 20;
            public const int LegendIconSize = 15;
            public const int LegendTextOffset = 30;
            public const int TimerInfoX = 250;
            public const int TimerInfoY = 10;
            public const int DebugInfoY = 230;
            public const int DebugInfo2Y = 250;
            public const int HintY = 30;

            // Modal window (Game Over)
            public const int ModalWidth = 400;
            public const int ModalHeight = 120;
            public const int ModalMargin = 30;
        }

        // ===== COLOR OPACITY VALUES =====
        public static class Opacity
        {
            public const float Transparent = 0f;
            public const float VeryLight = 0.3f;
            public const float Light = 0.4f;
            public const float Medium = 0.5f;
            public const float MediumHigh = 0.6f;
            public const float High = 0.7f;
            public const float VeryHigh = 0.8f;
            public const float AlmostOpaque = 0.9f;
            public const float Opaque = 1.0f;
        }

        // ===== DEBUG SETTINGS =====
        public static class Debug
        {
            public const int HitboxBorderThickness = 2;
            public const int GroundIndicatorSize = 10;
            public const int GroundIndicatorOffsetY = -10;
            public const int GroundIndicatorOffsetX = 5;
        }
    }
}
