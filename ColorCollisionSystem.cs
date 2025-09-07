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
        Solid = 1,        // Solid ground - blocks movement
        Platform = 2,     // Jump-through platform
        Hazard = 3,       // Damages player
        Water = 4,        // Special water physics
        Ice = 5,          // Slippery surface
        Lava = 6          // Hot hazard
    }

    public class ColorCollisionSystem
    {
        private Texture2D _backgroundTexture;
        private Color[] _pixelData;
        private int _textureWidth;
        private int _textureHeight;

        // Define what colors represent what surface types
        private Dictionary<Color, SurfaceType> _colorToSurface;

        // Color tolerance for matching (helps with JPEG compression artifacts)
        private int _colorTolerance = 20; // Increased tolerance for better matching

        public ColorCollisionSystem(Texture2D backgroundTexture)
        {
            _backgroundTexture = backgroundTexture;
            _textureWidth = backgroundTexture.Width;
            _textureHeight = backgroundTexture.Height;

            // Extract all pixel color data from the background
            _pixelData = new Color[_textureWidth * _textureHeight];
            _backgroundTexture.GetData(_pixelData);

            // Set up color mappings
            SetupColorMappings();
        }

        private void SetupColorMappings()
        {
            _colorToSurface = new Dictionary<Color, SurfaceType>();

            // CRITICAL: Add the specific dark gray colors from your level (from player position sample)
            // These are the actual platform colors your character is standing on
            _colorToSurface[new Color(29, 34, 38)] = SurfaceType.Solid;  // Most common platform color (count: 8)
            _colorToSurface[new Color(22, 26, 29)] = SurfaceType.Solid;  // Common ground (count: 7)
            _colorToSurface[new Color(28, 33, 37)] = SurfaceType.Solid;  // Platform (count: 6)
            _colorToSurface[new Color(23, 28, 32)] = SurfaceType.Solid;  // Dark stone
            _colorToSurface[new Color(26, 31, 35)] = SurfaceType.Solid;  // Platform edge
            _colorToSurface[new Color(25, 30, 34)] = SurfaceType.Solid;  // Platform variant
            _colorToSurface[new Color(27, 32, 36)] = SurfaceType.Solid;  // Another platform color
            _colorToSurface[new Color(24, 29, 33)] = SurfaceType.Solid;  // Dark platform
            _colorToSurface[new Color(21, 26, 30)] = SurfaceType.Solid;  // Dark ground
            _colorToSurface[new Color(21, 26, 29)] = SurfaceType.Solid;  // Ground variant
            _colorToSurface[new Color(17, 22, 26)] = SurfaceType.Solid;  // Very dark platform
            _colorToSurface[new Color(24, 25, 29)] = SurfaceType.Solid;  // Gray platform
            _colorToSurface[new Color(22, 27, 31)] = SurfaceType.Solid;  // Platform color
            _colorToSurface[new Color(32, 37, 41)] = SurfaceType.Solid;  // Lighter platform

            // Add more dark gray variations that appear in the game
            _colorToSurface[new Color(41, 46, 50)] = SurfaceType.Solid;  // Medium stone (very common)
            _colorToSurface[new Color(33, 38, 42)] = SurfaceType.Solid;  // Stone
            _colorToSurface[new Color(30, 35, 39)] = SurfaceType.Solid;  // Dark platform
            _colorToSurface[new Color(31, 36, 40)] = SurfaceType.Solid;  // Platform variant
            _colorToSurface[new Color(31, 36, 39)] = SurfaceType.Solid;  // Another variant
            _colorToSurface[new Color(20, 25, 29)] = SurfaceType.Solid;  // Very dark
            _colorToSurface[new Color(10, 15, 19)] = SurfaceType.Solid;  // Almost black platform
            _colorToSurface[new Color(11, 16, 20)] = SurfaceType.Solid;  // Dark variant
            _colorToSurface[new Color(14, 19, 23)] = SurfaceType.Solid;  // Dark ground

            // Brown ground colors from the bottom of your level
            _colorToSurface[new Color(110, 103, 49)] = SurfaceType.Solid;  // Most common brown
            _colorToSurface[new Color(112, 102, 49)] = SurfaceType.Solid;
            _colorToSurface[new Color(110, 103, 51)] = SurfaceType.Solid;
            _colorToSurface[new Color(111, 104, 50)] = SurfaceType.Solid;
            _colorToSurface[new Color(112, 103, 48)] = SurfaceType.Solid;

            // Platform/box colors (wooden crates)
            _colorToSurface[new Color(101, 67, 33)] = SurfaceType.Solid;   // Brown wood
            _colorToSurface[new Color(139, 69, 19)] = SurfaceType.Solid;   // Darker wood

            // Mid-tone browns
            _colorToSurface[new Color(69, 50, 43)] = SurfaceType.Solid;
            _colorToSurface[new Color(77, 58, 51)] = SurfaceType.Solid;
            _colorToSurface[new Color(82, 63, 56)] = SurfaceType.Solid;
            _colorToSurface[new Color(87, 68, 61)] = SurfaceType.Solid;

            // Darker browns and earth tones
            _colorToSurface[new Color(52, 33, 27)] = SurfaceType.Solid;
            _colorToSurface[new Color(57, 38, 32)] = SurfaceType.Solid;
            _colorToSurface[new Color(47, 28, 22)] = SurfaceType.Solid;
            _colorToSurface[new Color(50, 31, 25)] = SurfaceType.Solid;

            // Add fire/lava colors if present
            _colorToSurface[new Color(255, 0, 0)] = SurfaceType.Hazard;    // Pure red - fire
            _colorToSurface[new Color(255, 69, 0)] = SurfaceType.Lava;     // Orange red - lava

            // Ice colors if present
            _colorToSurface[new Color(173, 216, 230)] = SurfaceType.Ice;   // Light blue - ice
        }

        // Add a method to dynamically add color mappings
        public void AddColorMapping(Color color, SurfaceType surfaceType)
        {
            _colorToSurface[color] = surfaceType;
        }

        // Get surface type at a specific pixel coordinate
        public SurfaceType GetSurfaceAt(int x, int y)
        {
            // Check bounds
            if (x < 0 || x >= _textureWidth || y < 0 || y >= _textureHeight)
                return SurfaceType.Empty;

            // Get pixel color
            int index = y * _textureWidth + x;
            Color pixelColor = _pixelData[index];

            // IMPORTANT: Check for dark gray platform colors specifically
            // These are the main platform colors in your level
            if (pixelColor.R >= 20 && pixelColor.R <= 45 &&
                pixelColor.G >= 20 && pixelColor.G <= 45 &&
                pixelColor.B >= 20 && pixelColor.B <= 55)
            {
                // This is likely a platform/ground color
                return SurfaceType.Solid;
            }

            // Check for brown tones (common ground color)
            if (IsBrownish(pixelColor))
                return SurfaceType.Solid;

            // Try exact color match first
            if (_colorToSurface.ContainsKey(pixelColor))
                return _colorToSurface[pixelColor];

            // Try fuzzy color matching with tolerance
            foreach (var colorMapping in _colorToSurface)
            {
                if (ColorsAreClose(pixelColor, colorMapping.Key, _colorTolerance))
                    return colorMapping.Value;
            }

            // Default to empty if no match found
            return SurfaceType.Empty;
        }

        // Check if color is in the brownish range
        private bool IsBrownish(Color color)
        {
            // Check if color is in the brownish range (based on your debug output)
            return (color.R > 90 && color.R < 120 &&
                   color.G > 85 && color.G < 110 &&
                   color.B > 40 && color.B < 60) ||
                   // Additional brown ranges
                   (color.R > 45 && color.R < 90 &&
                   color.G > 25 && color.G < 70 &&
                   color.B > 15 && color.B < 45);
        }

        // Check if two colors are close enough (helps with JPEG compression)
        private bool ColorsAreClose(Color color1, Color color2, int tolerance)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance;
        }

        // Check if a rectangle area contains any solid surfaces
        public bool IsAreaSolid(Rectangle area)
        {
            // Sample more densely for better collision detection
            for (int x = area.Left; x < area.Right; x += 1) // Check every pixel for better accuracy
            {
                for (int y = area.Top; y < area.Bottom; y += 1)
                {
                    SurfaceType surface = GetSurfaceAt(x, y);
                    if (surface == SurfaceType.Solid)
                        return true;
                }
            }
            return false;
        }

        // Find the ground level at a specific X coordinate
        public int FindGroundLevel(int x)
        {
            // Start from top and find first solid surface
            for (int y = 0; y < _textureHeight; y++)
            {
                SurfaceType surface = GetSurfaceAt(x, y);
                if (surface == SurfaceType.Solid || surface == SurfaceType.Platform)
                    return y;
            }
            return _textureHeight; // No ground found
        }

        // Check for platform collision (jump-through platforms)
        public bool IsOnPlatform(Rectangle hitbox, float velocityY)
        {
            if (velocityY < 0) return false; // Not falling, can't land on platform

            // Check bottom edge of hitbox
            int checkY = hitbox.Bottom;
            for (int x = hitbox.Left; x < hitbox.Right; x += 2) // Sample more frequently
            {
                SurfaceType surface = GetSurfaceAt(x, checkY);
                if (surface == SurfaceType.Platform || surface == SurfaceType.Solid)
                    return true;
            }
            return false;
        }

        // Get all surface types in an area (for special effects)
        public List<SurfaceType> GetSurfaceTypesInArea(Rectangle area)
        {
            HashSet<SurfaceType> surfaceTypes = new HashSet<SurfaceType>();

            for (int x = area.Left; x < area.Right; x += 2) // Sample more frequently
            {
                for (int y = area.Top; y < area.Bottom; y += 2)
                {
                    SurfaceType surface = GetSurfaceAt(x, y);
                    if (surface != SurfaceType.Empty)
                        surfaceTypes.Add(surface);
                }
            }

            return new List<SurfaceType>(surfaceTypes);
        }

        // Debug method to get the actual color at a position
        public Color GetColorAt(int x, int y)
        {
            if (x < 0 || x >= _textureWidth || y < 0 || y >= _textureHeight)
                return Color.Transparent;

            int index = y * _textureWidth + x;
            return _pixelData[index];
        }

        // Method to sample colors from your image (useful for setup)
        public Dictionary<Color, int> SampleColors(Rectangle area)
        {
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            for (int x = area.Left; x < area.Right; x += 5)
            {
                for (int y = area.Top; y < area.Bottom; y += 5)
                {
                    Color color = GetColorAt(x, y);
                    if (colorCounts.ContainsKey(color))
                        colorCounts[color]++;
                    else
                        colorCounts[color] = 1;
                }
            }

            return colorCounts;
        }
    }
}