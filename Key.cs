using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using fire_and_ice.Interfaces;

namespace fire_and_ice
{
    /// <summary>
    /// Represents a collectable key in the game.
    /// Keys can be collected by players and are required to open doors.
    /// Features a bobbing animation when not collected.
    /// </summary>
    public class Key : IGameObject, ICollidable
    {
        /// <summary>
        /// Gets or sets the position of the key in the game world
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets whether this key has been collected by a player
        /// </summary>
        public bool IsCollected { get; set; }

        /// <summary>
        /// Gets or sets which player owns this key (0 = no owner, 1 = player 1, 2 = player 2)
        /// </summary>
        public int PlayerOwner { get; set; }

        private float _animationTimer = 0f;
        private float _bobOffset = 0f;
        private Texture2D _pixelTexture;

        /// <summary>
        /// Creates a new key at the specified position
        /// </summary>
        /// <param name="position">The position where the key should be placed</param>
        public Key(Vector2 position)
        {
            Position = position;
            IsCollected = false;
            PlayerOwner = 0;
        }

        /// <summary>
        /// Sets the pixel texture used for rendering the key
        /// </summary>
        /// <param name="pixelTexture">A 1x1 white pixel texture for drawing shapes</param>
        public void SetPixelTexture(Texture2D pixelTexture)
        {
            _pixelTexture = pixelTexture;
        }

        /// <summary>
        /// Gets the bounding rectangle for collision detection.
        /// Includes the bobbing offset for accurate collision.
        /// </summary>
        /// <returns>A Rectangle representing the key's collision bounds</returns>
        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X,
                (int)(Position.Y + _bobOffset),
                GameConstants.Key.Size,
                GameConstants.Key.Size
            );
        }

        /// <summary>
        /// Checks if this key collides with another collidable object
        /// </summary>
        /// <param name="other">The other collidable object to check against</param>
        /// <returns>True if the objects are colliding, false otherwise</returns>
        public bool CheckCollision(ICollidable other)
        {
            if (IsCollected)
                return false;

            return GetBounds().Intersects(other.GetBounds());
        }

        /// <summary>
        /// Updates the key's animation state.
        /// When not collected, the key bobs up and down smoothly.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            if (!IsCollected)
            {
                // Bob up and down animation using sine wave
                _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _bobOffset = (float)System.Math.Sin(_animationTimer * GameConstants.Key.BobSpeed) * GameConstants.Key.BobAmount;
            }
        }

        /// <summary>
        /// Draws the key to the screen.
        /// Only draws if the key has not been collected.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsCollected && _pixelTexture != null)
            {
                Rectangle bounds = GetBounds();

                // Draw key body (yellow/gold)
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX, bounds.Y + GameConstants.Key.BodyOffsetY,
                        GameConstants.Key.BodyWidth, GameConstants.Key.BodyHeight),
                    Color.Gold);

                // Draw key head (circle)
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.HeadOffsetX, bounds.Y + GameConstants.Key.HeadOffsetY,
                        GameConstants.Key.HeadSize, GameConstants.Key.HeadSize),
                    Color.Gold);

                // Draw key teeth
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX, bounds.Y + GameConstants.Key.TeethOffsetY,
                        GameConstants.Key.TeethSize, GameConstants.Key.TeethSize),
                    Color.Gold);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX + GameConstants.Key.TeethSpacing,
                        bounds.Y + GameConstants.Key.TeethOffsetY, GameConstants.Key.TeethSize, GameConstants.Key.TeethSize),
                    Color.Gold);

                // Draw glow effect
                spriteBatch.Draw(_pixelTexture, bounds, Color.Yellow * GameConstants.Opacity.VeryLight);
            }
        }

        /// <summary>
        /// Draws a small key icon at the specified position (used for HUD display).
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="pixelTexture">The pixel texture to use for drawing</param>
        /// <param name="position">Where to draw the icon</param>
        /// <param name="scale">The scale of the icon (default is defined in GameConstants)</param>
        public void DrawIcon(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 position, float scale = GameConstants.Key.IconScale)
        {
            int iconSize = (int)(GameConstants.Key.Size * scale);

            // Draw key body
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(GameConstants.Key.BodyOffsetX * scale),
                    (int)position.Y + (int)(GameConstants.Key.BodyOffsetY * scale),
                    (int)(GameConstants.Key.BodyWidth * scale), (int)(GameConstants.Key.BodyHeight * scale)),
                Color.Gold);

            // Draw key head
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(GameConstants.Key.HeadOffsetX * scale),
                    (int)position.Y + (int)(GameConstants.Key.HeadOffsetY * scale),
                    (int)(GameConstants.Key.HeadSize * scale), (int)(GameConstants.Key.HeadSize * scale)),
                Color.Gold);

            // Draw key teeth
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(GameConstants.Key.BodyOffsetX * scale),
                    (int)position.Y + (int)(GameConstants.Key.TeethOffsetY * scale),
                    (int)(GameConstants.Key.TeethSize * scale), (int)(GameConstants.Key.TeethSize * scale)),
                Color.Gold);
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)((GameConstants.Key.BodyOffsetX + GameConstants.Key.TeethSpacing) * scale),
                    (int)position.Y + (int)(GameConstants.Key.TeethOffsetY * scale),
                    (int)(GameConstants.Key.TeethSize * scale), (int)(GameConstants.Key.TeethSize * scale)),
                Color.Gold);
        }
    }
}
