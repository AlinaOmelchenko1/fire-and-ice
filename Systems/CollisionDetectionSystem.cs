using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Systems
{
    /// <summary>
    /// Represents a collision event between two collidable objects
    /// </summary>
    public class CollisionEventArgs : EventArgs
    {
        /// <summary>
        /// The first collidable object in the collision
        /// </summary>
        public ICollidable Object1 { get; set; }

        /// <summary>
        /// The second collidable object in the collision
        /// </summary>
        public ICollidable Object2 { get; set; }

        /// <summary>
        /// The time at which the collision occurred
        /// </summary>
        public float Time { get; set; }
    }

    /// <summary>
    /// Enhanced collision detection system with spatial partitioning for performance.
    /// Uses a grid-based broad-phase to reduce collision checks, followed by narrow-phase AABB testing.
    /// </summary>
    public class CollisionDetectionSystem
    {
        private readonly List<ICollidable> _collidables;
        private readonly Dictionary<Point, List<ICollidable>> _spatialGrid;
        private readonly HashSet<(ICollidable, ICollidable)> _checkedPairs;
        private int _cellSize;
        private Rectangle _worldBounds;

        /// <summary>
        /// Event raised when two objects collide
        /// </summary>
        public event EventHandler<CollisionEventArgs> CollisionDetected;

        /// <summary>
        /// Gets or sets the size of each spatial grid cell (in pixels)
        /// </summary>
        public int CellSize
        {
            get => _cellSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Cell size must be positive", nameof(value));
                _cellSize = value;
                RebuildSpatialGrid();
            }
        }

        /// <summary>
        /// Gets or sets the world bounds for spatial partitioning
        /// </summary>
        public Rectangle WorldBounds
        {
            get => _worldBounds;
            set
            {
                _worldBounds = value;
                RebuildSpatialGrid();
            }
        }

        /// <summary>
        /// Gets the number of registered collidable objects
        /// </summary>
        public int CollidableCount => _collidables.Count;

        /// <summary>
        /// Gets or sets whether spatial partitioning is enabled (default: true)
        /// </summary>
        public bool UseSpatialPartitioning { get; set; }

        /// <summary>
        /// Gets statistics about the last collision check
        /// </summary>
        public CollisionStats LastStats { get; private set; }

        /// <summary>
        /// Creates a new collision detection system
        /// </summary>
        /// <param name="cellSize">Size of spatial grid cells in pixels (default 128)</param>
        /// <param name="worldBounds">Bounds of the game world (default 800x600)</param>
        public CollisionDetectionSystem(int cellSize = 128, Rectangle? worldBounds = null)
        {
            _collidables = new List<ICollidable>();
            _spatialGrid = new Dictionary<Point, List<ICollidable>>();
            _checkedPairs = new HashSet<(ICollidable, ICollidable)>();
            _cellSize = cellSize;
            _worldBounds = worldBounds ?? new Rectangle(0, 0, 800, 600);
            UseSpatialPartitioning = true;
            LastStats = new CollisionStats();
        }

        /// <summary>
        /// Registers a collidable object
        /// </summary>
        /// <param name="collidable">The collidable object to register</param>
        public void Register(ICollidable collidable)
        {
            if (collidable == null)
                throw new ArgumentNullException(nameof(collidable));

            if (!_collidables.Contains(collidable))
            {
                _collidables.Add(collidable);
            }
        }

        /// <summary>
        /// Unregisters a collidable object
        /// </summary>
        /// <param name="collidable">The collidable object to unregister</param>
        public void Unregister(ICollidable collidable)
        {
            if (collidable != null)
            {
                _collidables.Remove(collidable);
            }
        }

        /// <summary>
        /// Clears all registered collidables
        /// </summary>
        public void Clear()
        {
            _collidables.Clear();
            _spatialGrid.Clear();
        }

        /// <summary>
        /// Performs collision detection for all registered objects
        /// </summary>
        /// <returns>List of collision pairs</returns>
        public List<(ICollidable, ICollidable)> DetectCollisions()
        {
            var collisions = new List<(ICollidable, ICollidable)>();
            _checkedPairs.Clear();

            int broadPhaseChecks = 0;
            int narrowPhaseChecks = 0;
            int collisionsFound = 0;

            if (UseSpatialPartitioning)
            {
                // Rebuild spatial grid
                RebuildSpatialGrid();

                // Check collisions within each cell
                foreach (var cell in _spatialGrid.Values)
                {
                    for (int i = 0; i < cell.Count; i++)
                    {
                        for (int j = i + 1; j < cell.Count; j++)
                        {
                            var obj1 = cell[i];
                            var obj2 = cell[j];

                            // Create ordered pair to avoid duplicate checks
                            var pair = obj1.GetHashCode() < obj2.GetHashCode()
                                ? (obj1, obj2)
                                : (obj2, obj1);

                            broadPhaseChecks++;

                            if (!_checkedPairs.Contains(pair))
                            {
                                _checkedPairs.Add(pair);
                                narrowPhaseChecks++;

                                if (obj1.CheckCollision(obj2))
                                {
                                    collisions.Add((obj1, obj2));
                                    collisionsFound++;
                                    OnCollisionDetected(obj1, obj2);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Brute force: check all pairs (O(n²))
                for (int i = 0; i < _collidables.Count; i++)
                {
                    for (int j = i + 1; j < _collidables.Count; j++)
                    {
                        broadPhaseChecks++;
                        narrowPhaseChecks++;

                        var obj1 = _collidables[i];
                        var obj2 = _collidables[j];

                        if (obj1.CheckCollision(obj2))
                        {
                            collisions.Add((obj1, obj2));
                            collisionsFound++;
                            OnCollisionDetected(obj1, obj2);
                        }
                    }
                }
            }

            LastStats = new CollisionStats
            {
                TotalObjects = _collidables.Count,
                BroadPhaseChecks = broadPhaseChecks,
                NarrowPhaseChecks = narrowPhaseChecks,
                CollisionsFound = collisionsFound,
                SpatialGridCells = _spatialGrid.Count
            };

            return collisions;
        }

        /// <summary>
        /// Finds all collidables within a specified area
        /// </summary>
        /// <param name="area">The area to search</param>
        /// <returns>List of collidables in the area</returns>
        public List<ICollidable> QueryArea(Rectangle area)
        {
            if (!UseSpatialPartitioning)
            {
                // Without spatial partitioning, check all objects
                return _collidables.Where(c => c.GetBounds().Intersects(area)).ToList();
            }

            var results = new HashSet<ICollidable>();

            // Get all cells that overlap with the query area
            int minCellX = area.Left / _cellSize;
            int maxCellX = area.Right / _cellSize;
            int minCellY = area.Top / _cellSize;
            int maxCellY = area.Bottom / _cellSize;

            for (int x = minCellX; x <= maxCellX; x++)
            {
                for (int y = minCellY; y <= maxCellY; y++)
                {
                    var cellKey = new Point(x, y);
                    if (_spatialGrid.TryGetValue(cellKey, out var cellObjects))
                    {
                        foreach (var obj in cellObjects)
                        {
                            if (obj.GetBounds().Intersects(area))
                            {
                                results.Add(obj);
                            }
                        }
                    }
                }
            }

            return results.ToList();
        }

        /// <summary>
        /// Finds the closest collidable to a given point
        /// </summary>
        /// <param name="point">The point to search from</param>
        /// <param name="maxDistance">Maximum search distance (default: float.MaxValue)</param>
        /// <returns>The closest collidable, or null if none found</returns>
        public ICollidable FindClosest(Vector2 point, float maxDistance = float.MaxValue)
        {
            ICollidable closest = null;
            float closestDistSq = maxDistance * maxDistance;

            foreach (var collidable in _collidables)
            {
                var bounds = collidable.GetBounds();
                var center = new Vector2(
                    bounds.X + bounds.Width / 2f,
                    bounds.Y + bounds.Height / 2f);

                float distSq = Vector2.DistanceSquared(point, center);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = collidable;
                }
            }

            return closest;
        }

        /// <summary>
        /// Performs a raycast to find the first collidable intersecting the ray
        /// </summary>
        /// <param name="origin">Ray origin</param>
        /// <param name="direction">Ray direction (should be normalized)</param>
        /// <param name="maxDistance">Maximum ray distance</param>
        /// <returns>The first collidable hit, or null if none</returns>
        public ICollidable Raycast(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue)
        {
            direction.Normalize();
            ICollidable closest = null;
            float closestDist = maxDistance;

            foreach (var collidable in _collidables)
            {
                var bounds = collidable.GetBounds();
                if (RayIntersectsRect(origin, direction, bounds, out float distance))
                {
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        closest = collidable;
                    }
                }
            }

            return closest;
        }

        /// <summary>
        /// Rebuilds the spatial grid based on current collidable positions
        /// </summary>
        private void RebuildSpatialGrid()
        {
            _spatialGrid.Clear();

            foreach (var collidable in _collidables)
            {
                var bounds = collidable.GetBounds();

                // Calculate which cells this object occupies
                int minCellX = bounds.Left / _cellSize;
                int maxCellX = bounds.Right / _cellSize;
                int minCellY = bounds.Top / _cellSize;
                int maxCellY = bounds.Bottom / _cellSize;

                // Add to all cells it overlaps
                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int y = minCellY; y <= maxCellY; y++)
                    {
                        var cellKey = new Point(x, y);
                        if (!_spatialGrid.ContainsKey(cellKey))
                        {
                            _spatialGrid[cellKey] = new List<ICollidable>();
                        }
                        _spatialGrid[cellKey].Add(collidable);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a ray intersects a rectangle
        /// </summary>
        private bool RayIntersectsRect(Vector2 origin, Vector2 direction, Rectangle rect, out float distance)
        {
            distance = float.MaxValue;

            // Convert rectangle to min/max points
            Vector2 min = new Vector2(rect.Left, rect.Top);
            Vector2 max = new Vector2(rect.Right, rect.Bottom);

            // Perform slab test
            float tmin = 0f;
            float tmax = float.MaxValue;

            // Check X slab
            if (Math.Abs(direction.X) > 0.0001f)
            {
                float t1 = (min.X - origin.X) / direction.X;
                float t2 = (max.X - origin.X) / direction.X;
                tmin = Math.Max(tmin, Math.Min(t1, t2));
                tmax = Math.Min(tmax, Math.Max(t1, t2));
            }
            else if (origin.X < min.X || origin.X > max.X)
            {
                return false;
            }

            // Check Y slab
            if (Math.Abs(direction.Y) > 0.0001f)
            {
                float t1 = (min.Y - origin.Y) / direction.Y;
                float t2 = (max.Y - origin.Y) / direction.Y;
                tmin = Math.Max(tmin, Math.Min(t1, t2));
                tmax = Math.Min(tmax, Math.Max(t1, t2));
            }
            else if (origin.Y < min.Y || origin.Y > max.Y)
            {
                return false;
            }

            if (tmax < tmin || tmin < 0)
                return false;

            distance = tmin;
            return true;
        }

        /// <summary>
        /// Raises the CollisionDetected event
        /// </summary>
        private void OnCollisionDetected(ICollidable obj1, ICollidable obj2)
        {
            CollisionDetected?.Invoke(this, new CollisionEventArgs
            {
                Object1 = obj1,
                Object2 = obj2,
                Time = 0f // Can be set by caller if needed
            });
        }

        /// <summary>
        /// Gets diagnostic information about the collision system
        /// </summary>
        public string GetDiagnostics()
        {
            return $"Collidables: {CollidableCount}, Grid Cells: {_spatialGrid.Count}, " +
                   $"Broad Phase: {LastStats.BroadPhaseChecks}, Narrow Phase: {LastStats.NarrowPhaseChecks}, " +
                   $"Collisions: {LastStats.CollisionsFound}";
        }
    }

    /// <summary>
    /// Statistics about collision detection performance
    /// </summary>
    public struct CollisionStats
    {
        /// <summary>
        /// Total number of objects checked
        /// </summary>
        public int TotalObjects { get; set; }

        /// <summary>
        /// Number of broad-phase checks performed
        /// </summary>
        public int BroadPhaseChecks { get; set; }

        /// <summary>
        /// Number of narrow-phase (actual collision) checks performed
        /// </summary>
        public int NarrowPhaseChecks { get; set; }

        /// <summary>
        /// Number of actual collisions found
        /// </summary>
        public int CollisionsFound { get; set; }

        /// <summary>
        /// Number of spatial grid cells used
        /// </summary>
        public int SpatialGridCells { get; set; }
    }
}
