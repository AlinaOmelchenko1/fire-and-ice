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
        private int _colorTolerance = 15; // Increased tolerance for better matching

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

            // Define your color scheme - adjust these colors based on your actual image

            // Common solid colors (you'll need to adjust these to match your image)
            _colorToSurface[new Color(0, 0, 0)] = SurfaceType.Solid;           // Pure black
            _colorToSurface[new Color(64, 64, 64)] = SurfaceType.Solid;        // Dark gray
            _colorToSurface[new Color(128, 128, 128)] = SurfaceType.Solid;     // Medium gray
            _colorToSurface[new Color(101, 67, 33)] = SurfaceType.Solid;       // Brown/dirt
            _colorToSurface[new Color(139, 69, 19)] = SurfaceType.Solid;       // Saddle brown

            // Platform colors (lighter colors that you can jump through)
            _colorToSurface[new Color(160, 160, 160)] = SurfaceType.Platform;  // Light gray
            _colorToSurface[new Color(205, 133, 63)] = SurfaceType.Platform;   // Peru/wood

            // Special surface types
            _colorToSurface[new Color(255, 0, 0)] = SurfaceType.Hazard;        // Red - spikes/danger
            _colorToSurface[new Color(0, 100, 200)] = SurfaceType.Water;       // Blue - water
            _colorToSurface[new Color(173, 216, 230)] = SurfaceType.Ice;       // Light blue - ice
            _colorToSurface[new Color(255, 69, 0)] = SurfaceType.Lava;         // Orange red - lava
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
            for (int x = area.Left; x < area.Right; x += 2) // Sample every 2 pixels for performance
            {
                for (int y = area.Top; y < area.Bottom; y += 2)
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
            for (int x = hitbox.Left; x < hitbox.Right; x += 3)
            {
                SurfaceType surface = GetSurfaceAt(x, checkY);
                if (surface == SurfaceType.Platform)
                    return true;
            }
            return false;
        }

        // Get all surface types in an area (for special effects)
        public List<SurfaceType> GetSurfaceTypesInArea(Rectangle area)
        {
            HashSet<SurfaceType> surfaceTypes = new HashSet<SurfaceType>();

            for (int x = area.Left; x < area.Right; x += 3)
            {
                for (int y = area.Top; y < area.Bottom; y += 3)
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