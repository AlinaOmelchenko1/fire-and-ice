using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace fire_and_ice
{
    /// <summary>
    /// Represents a door with bars that can open when conditions are met
    /// </summary>
    public class Door
    {
        public Vector2 Position { get; set; }
        public bool IsOpen { get; private set; }
        private float _openProgress = 0f; // 0 = closed, 1 = fully open
        private const float OPEN_SPEED = 1.5f; // Speed of opening animation
        private const int DOOR_WIDTH = 48;
        private const int DOOR_HEIGHT = 50;
        private const int BAR_COUNT = 7;
        private bool _startOpening = false;

        public Door(Vector2 position)
        {
            Position = position;
            IsOpen = false;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                DOOR_WIDTH,
                DOOR_HEIGHT
            );
        }

        public void StartOpening()
        {
            _startOpening = true;
        }

        public void Update(GameTime gameTime)
        {
            if (_startOpening && !IsOpen)
            {
                _openProgress += OPEN_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_openProgress >= 1f)
                {
                    _openProgress = 1f;
                    IsOpen = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            Rectangle doorBounds = GetBounds();

            // Draw dark background for the entire door area
            spriteBatch.Draw(pixelTexture,
                doorBounds,
                Color.Black * 0.8f);

            // Draw bars if not fully open
            if (_openProgress < 1f)
            {
                int barSpacing = doorBounds.Width / (BAR_COUNT + 1);
                int barWidth = 4;

                // Calculate how far bars have moved
                int barOffset = (int)(_openProgress * doorBounds.Height);

                for (int i = 0; i < BAR_COUNT; i++)
                {
                    int barX = doorBounds.X + barSpacing * (i + 1) - barWidth / 2;
                    int barY = doorBounds.Y + barOffset; // Bars move down as they "retract"
                    int barHeight = doorBounds.Height - barOffset;

                    if (barHeight > 0)
                    {
                        // Draw bar (dark iron color)
                        spriteBatch.Draw(pixelTexture,
                            new Rectangle(barX, barY, barWidth, barHeight),
                            Color.DarkSlateGray);

                        // Draw bar highlight on left side
                        spriteBatch.Draw(pixelTexture,
                            new Rectangle(barX, barY, 1, barHeight),
                            Color.Gray);

                        // Draw bar shadow on right side
                        spriteBatch.Draw(pixelTexture,
                            new Rectangle(barX + barWidth - 1, barY, 1, barHeight),
                            Color.Black * 0.5f);
                    }
                }

                // Draw horizontal bars for extra security look
                if (_openProgress < 0.5f) // Only show horizontal bars in first half of animation
                {
                    int horizontalBarHeight = 4;
                    int horizontalBarCount = 3;
                    int horizontalBarSpacing = doorBounds.Height / (horizontalBarCount + 1);

                    for (int i = 0; i < horizontalBarCount; i++)
                    {
                        int barY = doorBounds.Y + horizontalBarSpacing * (i + 1) + barOffset;

                        if (barY < doorBounds.Bottom && barY + horizontalBarHeight <= doorBounds.Bottom)
                        {
                            spriteBatch.Draw(pixelTexture,
                                new Rectangle(doorBounds.X, barY, doorBounds.Width, horizontalBarHeight),
                                Color.DarkSlateGray);

                            // Highlight on top
                            spriteBatch.Draw(pixelTexture,
                                new Rectangle(doorBounds.X, barY, doorBounds.Width, 1),
                                Color.Gray);
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            IsOpen = false;
            _openProgress = 0f;
            _startOpening = false;
        }
    }
}
