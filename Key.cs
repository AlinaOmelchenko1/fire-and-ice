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
        private const int KEY_SIZE = 20;
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
                KEY_SIZE,
                KEY_SIZE
            );
        }

        public void Update(GameTime gameTime)
        {
            if (!IsCollected)
            {
                // Bob up and down animation
                _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _bobOffset = (float)System.Math.Sin(_animationTimer * 3f) * 5f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            if (!IsCollected)
            {
                Rectangle bounds = GetBounds();

                // Draw key body (yellow/gold)
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + 5, bounds.Y + 5, 10, 12),
                    Color.Gold);

                // Draw key head (circle)
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + 7, bounds.Y + 2, 6, 6),
                    Color.Gold);

                // Draw key teeth
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + 5, bounds.Y + 15, 3, 3),
                    Color.Gold);
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(bounds.X + 10, bounds.Y + 15, 3, 3),
                    Color.Gold);

                // Draw glow effect
                spriteBatch.Draw(pixelTexture, bounds, Color.Yellow * 0.3f);
            }
        }

        public void DrawIcon(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 position, float scale = 0.7f)
        {
            int iconSize = (int)(KEY_SIZE * scale);

            // Draw key body
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(5 * scale), (int)position.Y + (int)(5 * scale), (int)(10 * scale), (int)(12 * scale)),
                Color.Gold);

            // Draw key head
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(7 * scale), (int)position.Y + (int)(2 * scale), (int)(6 * scale), (int)(6 * scale)),
                Color.Gold);

            // Draw key teeth
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(5 * scale), (int)position.Y + (int)(15 * scale), (int)(3 * scale), (int)(3 * scale)),
                Color.Gold);
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)position.X + (int)(10 * scale), (int)position.Y + (int)(15 * scale), (int)(3 * scale), (int)(3 * scale)),
                Color.Gold);
        }
    }
}
