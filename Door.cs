using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using fire_and_ice.Interfaces;

namespace fire_and_ice
{
    /// <summary>
    /// Represents a door with animated bars that can open when conditions are met.
    /// The door features vertical and horizontal bars that retract when opened.
    /// Players must collect keys to trigger the door opening.
    /// </summary>
    public class Door : IGameObject, ICollidable
    {
        /// <summary>
        /// Gets or sets the position of the door in the game world
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets whether the door is fully open (bars fully retracted)
        /// </summary>
        public bool IsOpen { get; private set; }

        private float _openProgress = 0f; // 0 = closed, 1 = fully open
        private bool _startOpening = false;
        private Texture2D _pixelTexture;

        /// <summary>
        /// Creates a new door at the specified position
        /// </summary>
        /// <param name="position">The position where the door should be placed</param>
        public Door(Vector2 position)
        {
            Position = position;
            IsOpen = false;
        }

        /// <summary>
        /// Sets the pixel texture used for rendering the door
        /// </summary>
        /// <param name="pixelTexture">A 1x1 white pixel texture for drawing shapes</param>
        public void SetPixelTexture(Texture2D pixelTexture)
        {
            _pixelTexture = pixelTexture;
        }

        /// <summary>
        /// Gets the bounding rectangle for collision detection.
        /// Represents the entire door area including the bars.
        /// </summary>
        /// <returns>A Rectangle representing the door's collision bounds</returns>
        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                GameConstants.Door.Width,
                GameConstants.Door.Height
            );
        }

        /// <summary>
        /// Checks if this door collides with another collidable object.
        /// Only performs collision when the door is not fully open.
        /// </summary>
        /// <param name="other">The other collidable object to check against</param>
        /// <returns>True if the objects are colliding and door is not fully open, false otherwise</returns>
        public bool CheckCollision(ICollidable other)
        {
            if (IsOpen)
                return false;

            return GetBounds().Intersects(other.GetBounds());
        }

        /// <summary>
        /// Triggers the door to start opening.
        /// The door will animate its bars retracting.
        /// </summary>
        public void StartOpening()
        {
            _startOpening = true;
        }

        /// <summary>
        /// Updates the door's opening animation.
        /// When triggered, the door's bars will retract until fully open.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            if (_startOpening && !IsOpen)
            {
                _openProgress += GameConstants.Door.OpenSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_openProgress >= 1f)
                {
                    _openProgress = 1f;
                    IsOpen = true;
                }
            }
        }

        /// <summary>
        /// Draws the door and its bars to the screen.
        /// Renders the door background and animated bars (if not fully open).
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_pixelTexture == null)
                return;

            Rectangle doorBounds = GetBounds();

            // Draw dark background for the entire door area
            spriteBatch.Draw(_pixelTexture,
                doorBounds,
                Color.Black * GameConstants.Opacity.VeryHigh);

            // Draw bars if not fully open
            if (_openProgress < 1f)
            {
                int barSpacing = doorBounds.Width / (GameConstants.Door.BarCount + 1);
                int barWidth = GameConstants.Door.BarWidth;

                // Calculate how far bars have moved
                int barOffset = (int)(_openProgress * doorBounds.Height);

                for (int i = 0; i < GameConstants.Door.BarCount; i++)
                {
                    int barX = doorBounds.X + barSpacing * (i + 1) - barWidth / 2;
                    int barY = doorBounds.Y + barOffset; // Bars move down as they "retract"
                    int barHeight = doorBounds.Height - barOffset;

                    if (barHeight > 0)
                    {
                        // Draw bar (dark iron color)
                        spriteBatch.Draw(_pixelTexture,
                            new Rectangle(barX, barY, barWidth, barHeight),
                            Color.DarkSlateGray);

                        // Draw bar highlight on left side
                        spriteBatch.Draw(_pixelTexture,
                            new Rectangle(barX, barY, 1, barHeight),
                            Color.Gray);

                        // Draw bar shadow on right side
                        spriteBatch.Draw(_pixelTexture,
                            new Rectangle(barX + barWidth - 1, barY, 1, barHeight),
                            Color.Black * GameConstants.Opacity.Medium);
                    }
                }

                // Draw horizontal bars for extra security look
                if (_openProgress < 0.5f) // Only show horizontal bars in first half of animation
                {
                    int horizontalBarHeight = GameConstants.Door.HorizontalBarHeight;
                    int horizontalBarCount = GameConstants.Door.HorizontalBarCount;
                    int horizontalBarSpacing = doorBounds.Height / (horizontalBarCount + 1);

                    for (int i = 0; i < horizontalBarCount; i++)
                    {
                        int barY = doorBounds.Y + horizontalBarSpacing * (i + 1) + barOffset;

                        if (barY < doorBounds.Bottom && barY + horizontalBarHeight <= doorBounds.Bottom)
                        {
                            spriteBatch.Draw(_pixelTexture,
                                new Rectangle(doorBounds.X, barY, doorBounds.Width, horizontalBarHeight),
                                Color.DarkSlateGray);

                            // Highlight on top
                            spriteBatch.Draw(_pixelTexture,
                                new Rectangle(doorBounds.X, barY, doorBounds.Width, 1),
                                Color.Gray);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets the door to its initial closed state.
        /// Bars will be fully visible and the door will be closed.
        /// </summary>
        public void Reset()
        {
            IsOpen = false;
            _openProgress = 0f;
            _startOpening = false;
        }
    }
}
