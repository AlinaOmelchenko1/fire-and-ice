using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace fire_and_ice
{
    /// <summary>
    /// Represents a collectable key in the game
    /// </summary>
    public class Key
    {
        public Vector2 Position { get; set; }
        public bool IsCollected { get; set; }
        public int PlayerOwner { get; set; } // 0 for no owner, 1 or 2 for player
        private float _animationTimer = 0f;
        private float _bobOffset = 0f;

        public Key(Vector2 position)
        {
            Position = position;
            IsCollected = false;
            PlayerOwner = 0;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X,
                (int)(Position.Y + _bobOffset),
                GameConstants.Key.Size,
                GameConstants.Key.Size
            );
        }

        public void Update(GameTime gameTime)
        {
            if (!IsCollected)
            {
                // Bob up and down animation
                _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _bobOffset = (float)System.Math.Sin(_animationTimer * GameConstants.Key.BobSpeed) * GameConstants.Key.BobAmount;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (!IsCollected)
            {
                Rectangle bounds = GetBounds();

                // Draw key body (yellow/gold)
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX, bounds.Y + GameConstants.Key.BodyOffsetY,
                        GameConstants.Key.BodyWidth, GameConstants.Key.BodyHeight),
                    Color.Gold);

                // Draw key head (circle)
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.HeadOffsetX, bounds.Y + GameConstants.Key.HeadOffsetY,
                        GameConstants.Key.HeadSize, GameConstants.Key.HeadSize),
                    Color.Gold);

                // Draw key teeth
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX, bounds.Y + GameConstants.Key.TeethOffsetY,
                        GameConstants.Key.TeethSize, GameConstants.Key.TeethSize),
                    Color.Gold);
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + GameConstants.Key.BodyOffsetX + GameConstants.Key.TeethSpacing,
                        bounds.Y + GameConstants.Key.TeethOffsetY, GameConstants.Key.TeethSize, GameConstants.Key.TeethSize),
                    Color.Gold);

                // Draw glow effect
                spriteBatch.Draw(pixelTexture, bounds, Color.Yellow * 0.3f);
            }
        }

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
