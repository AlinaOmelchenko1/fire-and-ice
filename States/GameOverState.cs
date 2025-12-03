using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice.States
{
    /// <summary>
    /// Represents the game over state displayed when a player dies.
    /// Shows a red modal window with "GAME OVER" text and allows restart after a delay.
    /// </summary>
    public class GameOverState : BaseGameState
    {
        private readonly Texture2D _levelTexture;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _font;
        private readonly Player _player;
        private readonly Player _player2;
        private readonly Action _restartGame;

        private float _gameOverTimer;

        /// <summary>
        /// Creates a new game over state
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        /// <param name="levelTexture">Background texture for the level</param>
        /// <param name="pixelTexture">1x1 white pixel texture for UI elements</param>
        /// <param name="font">Font for text rendering</param>
        /// <param name="player">First player instance</param>
        /// <param name="player2">Second player instance</param>
        /// <param name="restartGame">Callback to restart the game</param>
        public GameOverState(
            Game1 game,
            SpriteBatch spriteBatch,
            Texture2D levelTexture,
            Texture2D pixelTexture,
            SpriteFont font,
            Player player,
            Player player2,
            Action restartGame) : base(game, spriteBatch)
        {
            _levelTexture = levelTexture;
            _pixelTexture = pixelTexture;
            _font = font;
            _player = player;
            _player2 = player2;
            _restartGame = restartGame;
            _gameOverTimer = 0f;
        }

        /// <summary>
        /// Called when entering the game over state.
        /// Resets the timer to 0.
        /// </summary>
        public override void Enter()
        {
            _gameOverTimer = 0f;
            System.Diagnostics.Debug.WriteLine("Entered GameOver state");
        }

        /// <summary>
        /// Updates the game over timer
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            _gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            System.Diagnostics.Debug.WriteLine($"GameOver Update - Timer: {_gameOverTimer:F2}s");
        }

        /// <summary>
        /// Processes keyboard input to allow restart after delay
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        public override void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            // Allow restart after delay
            if (_gameOverTimer >= GameConstants.GameState.GameOverDelay)
            {
                if (IsKeyPressed(Keys.Enter, keyboardState, previousKeyboardState) ||
                    IsKeyPressed(Keys.Space, keyboardState, previousKeyboardState))
                {
                    System.Diagnostics.Debug.WriteLine("Restart key pressed - Restarting game");
                    _restartGame?.Invoke();
                }
            }
        }

        /// <summary>
        /// Renders the game over screen with greyed-out game world and modal window
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                // Draw the game world (greyed out)
                spriteBatch.Draw(_levelTexture,
                    new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                    Color.Gray * GameConstants.Opacity.Medium);

                _player.Draw(spriteBatch);
                _player2.Draw(spriteBatch);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing game world in GameOver: {ex.Message}");
            }

            // Draw semi-transparent overlay
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.Black * GameConstants.Opacity.High);

            // Draw red modal window
            Rectangle modalWindow = new Rectangle(
                ScreenCenterX - GameConstants.UI.ModalWidth / 2,
                ScreenCenterY - GameConstants.UI.ModalHeight / 2,
                GameConstants.UI.ModalWidth,
                GameConstants.UI.ModalHeight
            );
            DrawFilledRectangle(spriteBatch, _pixelTexture, modalWindow, Color.Red * GameConstants.Opacity.VeryHigh);

            // Draw GAME OVER text if font available
            if (_font != null)
            {
                DrawGameOverText(spriteBatch, modalWindow);
                DrawRestartPrompt(spriteBatch, modalWindow);
            }
        }

        /// <summary>
        /// Draws the "GAME OVER" text centered in the modal window
        /// </summary>
        private void DrawGameOverText(SpriteBatch spriteBatch, Rectangle modalWindow)
        {
            string gameOverText = "GAME OVER";
            Vector2 textSize = _font.MeasureString(gameOverText);
            float textScale = GameConstants.UI.GameOverTextScale;

            // Center text within the red modal window
            Vector2 textPosition = new Vector2(
                modalWindow.X + (modalWindow.Width - textSize.X * textScale) / 2,
                modalWindow.Y + (modalWindow.Height - textSize.Y * textScale) / 2
            );

            // Draw black text centered in modal window
            spriteBatch.DrawString(_font, gameOverText,
                textPosition,
                Color.Black, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the restart prompt after the delay period
        /// </summary>
        private void DrawRestartPrompt(SpriteBatch spriteBatch, Rectangle modalWindow)
        {
            // Draw restart prompt after delay (below the modal window)
            if (_gameOverTimer >= GameConstants.GameState.GameOverDelay)
            {
                string restartText = "Press ENTER or SPACE to Restart";
                DrawCenteredText(spriteBatch, _font, restartText,
                    modalWindow.Bottom + GameConstants.UI.ModalMargin,
                    Color.White);
            }
        }
    }
}
