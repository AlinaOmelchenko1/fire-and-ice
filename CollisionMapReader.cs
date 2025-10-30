using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace fire_and_ice
{
    public class CollisionMapReader
    {
        private Texture2D _collisionMap;
        private Color[] _collisionData;
        private int _width;
        private int _height;

        // Define the collision color (green in your map)
        private readonly Color COLLISION_COLOR = new Color(0, 255, 0); // Pure green
        private readonly int COLOR_TOLERANCE = 50; // Tolerance for color matching

        public CollisionMapReader(Texture2D collisionMap)
        {
            _collisionMap = collisionMap;
            _width = collisionMap.Width;
            _height = collisionMap.Height;

            // Extract pixel data from the collision map
            _collisionData = new Color[_width * _height];
            collisionMap.GetData(_collisionData);
        }

        // Check if a specific pixel is a collision pixel (green)
        public bool IsCollisionPixel(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            Color pixel = _collisionData[y * _width + x];

            // Check if pixel is green (with tolerance)
            return pixel.G > 200 && pixel.R < COLOR_TOLERANCE && pixel.B < COLOR_TOLERANCE;
        }

        // Extract all collision rectangles from the map as InteractableObjects
        public List<InteractableObject> ExtractCollisionRectangles()
        {
            List<InteractableObject> platforms = new List<InteractableObject>();
            bool[,] processed = new bool[_width, _height];

            // Scan the entire image for green pixels
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (IsCollisionPixel(x, y) && !processed[x, y])
                    {
                        // Found a new platform, extract its bounds
                        Rectangle platform = ExtractPlatformBounds(x, y, processed);
                        if (platform.Width > 5 && platform.Height > 5) // Ignore tiny platforms
                        {
                            // Default to Solid type for image-based collision
                            platforms.Add(new InteractableObject(platform, SurfaceType.Solid));
                        }
                    }
                }
            }

            return platforms;
        }

        // Extract bounds of a platform starting from a green pixel
        private Rectangle ExtractPlatformBounds(int startX, int startY, bool[,] processed)
        {
            int minX = startX;
            int maxX = startX;
            int minY = startY;
            int maxY = startY;

            // Use flood fill to find all connected green pixels
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(startX, startY));
            processed[startX, startY] = true;

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();

                // Update bounds
                minX = System.Math.Min(minX, current.X);
                maxX = System.Math.Max(maxX, current.X);
                minY = System.Math.Min(minY, current.Y);
                maxY = System.Math.Max(maxY, current.Y);

                // Check adjacent pixels (4-directional)
                Point[] neighbors = new Point[]
                {
                    new Point(current.X + 1, current.Y),
                    new Point(current.X - 1, current.Y),
                    new Point(current.X, current.Y + 1),
                    new Point(current.X, current.Y - 1)
                };

                foreach (Point neighbor in neighbors)
                {
                    if (neighbor.X >= 0 && neighbor.X < _width &&
                        neighbor.Y >= 0 && neighbor.Y < _height &&
                        !processed[neighbor.X, neighbor.Y] &&
                        IsCollisionPixel(neighbor.X, neighbor.Y))
                    {
                        processed[neighbor.X, neighbor.Y] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        // Check if a rectangle collides with any green pixels
        public bool CheckCollision(Rectangle hitbox)
        {
            int startX = System.Math.Max(0, hitbox.Left);
            int endX = System.Math.Min(_width - 1, hitbox.Right);
            int startY = System.Math.Max(0, hitbox.Top);
            int endY = System.Math.Min(_height - 1, hitbox.Bottom);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (IsCollisionPixel(x, y))
                        return true;
                }
            }

            return false;
        }
    }
}