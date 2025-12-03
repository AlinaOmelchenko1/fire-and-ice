using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice.States
{
    /// <summary>
    /// Represents the victory state displayed when players complete the level.
    /// Shows a congratulations message with gold text, then returns to main menu after a timer.
    /// </summary>
    public class VictoryState : BaseGameState
    {
        private readonly Texture2D _levelTexture;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _font;
        private readonly Player _player;
        private readonly Player _player2;
        private readonly Door _door1;
        private readonly Door _door2;
        private readonly Action<GameState> _changeState;
        private readonly Action _restartGame;

        private float _victoryTimer;

        /// <summary>
        /// Creates a new victory state
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        /// <param name="levelTexture">Background texture for the level</param>
        /// <param name="pixelTexture">1x1 white pixel texture for UI elements</param>
        /// <param name="font">Font for text rendering</param>
        /// <param name="player">First player instance</param>
        /// <param name="player2">Second player instance</param>
        /// <param name="door1">First door instance</param>
        /// <param name="door2">Second door instance</param>
        /// <param name="changeState">Callback to change game state</param>
        /// <param name="restartGame">Callback to restart the game</param>
        public VictoryState(
            Game1 game,
            SpriteBatch spriteBatch,
            Texture2D levelTexture,
            Texture2D pixelTexture,
            SpriteFont font,
            Player player,
            Player player2,
            Door door1,
            Door door2,
            Action<GameState> changeState,
            Action restartGame) : base(game, spriteBatch)
        {
            _levelTexture = levelTexture;
            _pixelTexture = pixelTexture;
            _font = font;
            _player = player;
            _player2 = player2;
            _door1 = door1;
            _door2 = door2;
            _changeState = changeState;
            _restartGame = restartGame;
            _victoryTimer = 0f;
        }

        /// <summary>
        /// Called when entering the victory state.
        /// Resets the timer to 0.
        /// </summary>
        public override void Enter()
        {
            _victoryTimer = 0f;
            System.Diagnostics.Debug.WriteLine("Entered Victory state");
        }

        /// <summary>
        /// Updates the victory timer and returns to main menu after display time
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            _victoryTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine($"Victory Update - Timer: {_victoryTimer:F2}s");

            // After display time, return to main menu
            if (_victoryTimer >= GameConstants.GameState.VictoryDisplayTime)
            {
                System.Diagnostics.Debug.WriteLine("Victory timer complete - Returning to main menu");

                // Reset game state for next play
                _restartGame?.Invoke();

                // Go to main menu (RestartGame sets it to Playing, so override)
                _changeState?.Invoke(GameState.MainMenu);
            }
        }

        /// <summary>
        /// Processes keyboard input (currently no input handling needed)
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        public override void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            // No input handling needed - victory screen automatically transitions after timer
        }

        /// <summary>
        /// Renders the victory screen with congratulations message
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the game world in the background
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.White);

            // Draw doors (open)
            _door1.Draw(spriteBatch);
            _door2.Draw(spriteBatch);

            // Draw players (at their victory positions)
            _player.Draw(spriteBatch);
            _player2.Draw(spriteBatch);

            // Draw semi-transparent overlay
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.Black * GameConstants.Opacity.MediumHigh);

            // Draw victory message
            if (_font != null)
            {
                DrawVictoryMessage(spriteBatch);
            }
            else
            {
                DrawFallbackVictory(spriteBatch);
            }
        }

        /// <summary>
        /// Draws the victory message with congratulations text and subtitle
        /// </summary>
        private void DrawVictoryMessage(SpriteBatch spriteBatch)
        {
            // Draw "CONGRATULATIONS!" text
            string victoryText = "CONGRATULATIONS!";
            Vector2 textSize = _font.MeasureString(victoryText);
            float textScale = 2.5f;
            Vector2 textPosition = new Vector2(
                (ScreenWidth - textSize.X * textScale) / 2,
                (ScreenHeight - textSize.Y * textScale) / 2 - 40
            );

            // Draw shadow
            spriteBatch.DrawString(_font, victoryText,
                textPosition + new Vector2(4, 4),
                Color.Black, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

            // Draw main text with gold color
            spriteBatch.DrawString(_font, victoryText,
                textPosition,
                Color.Gold, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

            // Draw "Level Complete!" subtitle
            DrawSubtitle(spriteBatch, textPosition, textSize, textScale);
        }

        /// <summary>
        /// Draws the subtitle text below the main victory message
        /// </summary>
        private void DrawSubtitle(SpriteBatch spriteBatch, Vector2 mainTextPosition, Vector2 mainTextSize, float mainTextScale)
        {
            string subtitleText = "Level Complete!";
            Vector2 subtitleSize = _font.MeasureString(subtitleText);
            float subtitleScale = 1.5f;
            Vector2 subtitlePosition = new Vector2(
                (ScreenWidth - subtitleSize.X * subtitleScale) / 2,
                mainTextPosition.Y + mainTextSize.Y * mainTextScale + 20
            );

            // Draw subtitle shadow
            spriteBatch.DrawString(_font, subtitleText,
                subtitlePosition + new Vector2(2, 2),
                Color.Black, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0f);

            // Draw subtitle text
            spriteBatch.DrawString(_font, subtitleText,
                subtitlePosition,
                Color.White, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a fallback victory indicator when no font is available
        /// </summary>
        private void DrawFallbackVictory(SpriteBatch spriteBatch)
        {
            // Draw colored victory indicator (gold rectangle)
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(ScreenCenterX - 150, ScreenCenterY - 50, 300, 100),
                Color.Gold * GameConstants.Opacity.VeryHigh);
        }
    }
}
