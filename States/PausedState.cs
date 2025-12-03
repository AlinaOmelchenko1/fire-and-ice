using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice.States
{
    /// <summary>
    /// Represents the paused state displayed when the player pauses the game.
    /// Shows the frozen game world with a menu overlay offering Resume and Exit options.
    /// </summary>
    public class PausedState : BaseGameState
    {
        // Resources
        private readonly Texture2D _levelTexture;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _font;

        // Game entities (for background rendering)
        private readonly Player _player;
        private readonly Player _player2;
        private readonly Door _door1;
        private readonly Door _door2;
        private readonly Key _key1;
        private readonly Key _key2;
        private readonly List<Flame> _flames;
        private readonly List<IceShard> _iceShards;

        // State management
        private readonly Action<GameState> _changeState;
        private readonly Action _exitGame;

        private int _selectedPauseOption;
        private readonly string[] _pauseOptions = { "RESUME", "EXIT" };

        /// <summary>
        /// Creates a new paused state with all required game entities and resources
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        /// <param name="levelTexture">Background texture for the level</param>
        /// <param name="pixelTexture">1x1 white pixel texture for UI elements</param>
        /// <param name="font">Font for menu text</param>
        /// <param name="player">First player instance</param>
        /// <param name="player2">Second player instance</param>
        /// <param name="door1">First door instance</param>
        /// <param name="door2">Second door instance</param>
        /// <param name="key1">First key instance</param>
        /// <param name="key2">Second key instance</param>
        /// <param name="flames">List of flame hazards</param>
        /// <param name="iceShards">List of ice shard hazards</param>
        /// <param name="changeState">Callback to change game state</param>
        /// <param name="exitGame">Callback to exit the game</param>
        public PausedState(
            Game1 game,
            SpriteBatch spriteBatch,
            Texture2D levelTexture,
            Texture2D pixelTexture,
            SpriteFont font,
            Player player,
            Player player2,
            Door door1,
            Door door2,
            Key key1,
            Key key2,
            List<Flame> flames,
            List<IceShard> iceShards,
            Action<GameState> changeState,
            Action exitGame) : base(game, spriteBatch)
        {
            _levelTexture = levelTexture;
            _pixelTexture = pixelTexture;
            _font = font;
            _player = player;
            _player2 = player2;
            _door1 = door1;
            _door2 = door2;
            _key1 = key1;
            _key2 = key2;
            _flames = flames;
            _iceShards = iceShards;
            _changeState = changeState;
            _exitGame = exitGame;

            _selectedPauseOption = 0;
        }

        /// <summary>
        /// Called when entering the paused state.
        /// Resets the selected menu option to Resume.
        /// </summary>
        public override void Enter()
        {
            _selectedPauseOption = 0; // Reset to Resume
            System.Diagnostics.Debug.WriteLine("Entered Paused state");
        }

        /// <summary>
        /// Updates the paused state (currently no time-based updates needed)
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            // No time-based updates needed for pause menu
            // All logic is handled in HandleInput
        }

        /// <summary>
        /// Processes keyboard input for menu navigation and selection
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        public override void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            // Navigate menu down with Down or S keys
            if (IsKeyPressed(Keys.Down, keyboardState, previousKeyboardState) ||
                IsKeyPressed(Keys.S, keyboardState, previousKeyboardState))
            {
                _selectedPauseOption = (_selectedPauseOption + 1) % GameConstants.Menu.PauseMenuOptionsCount;
            }

            // Navigate menu up with Up or W keys
            if (IsKeyPressed(Keys.Up, keyboardState, previousKeyboardState) ||
                IsKeyPressed(Keys.W, keyboardState, previousKeyboardState))
            {
                _selectedPauseOption = (_selectedPauseOption - 1 + GameConstants.Menu.PauseMenuOptionsCount) % GameConstants.Menu.PauseMenuOptionsCount;
            }

            // Select option with Enter or Space
            if (IsKeyPressed(Keys.Enter, keyboardState, previousKeyboardState) ||
                IsKeyPressed(Keys.Space, keyboardState, previousKeyboardState))
            {
                SelectCurrentOption();
            }

            // Allow Escape to resume (toggle pause)
            if (IsKeyPressed(Keys.Escape, keyboardState, previousKeyboardState))
            {
                System.Diagnostics.Debug.WriteLine("Resuming game with ESC key");
                _changeState?.Invoke(GameState.Playing);
            }
        }

        /// <summary>
        /// Executes the action for the currently selected menu option
        /// </summary>
        private void SelectCurrentOption()
        {
            switch (_selectedPauseOption)
            {
                case 0: // RESUME
                    System.Diagnostics.Debug.WriteLine("Resuming game from pause menu");
                    _changeState?.Invoke(GameState.Playing);
                    break;

                case 1: // EXIT
                    _exitGame?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Renders the paused state with frozen game world and menu overlay
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the game world in the background (slightly dimmed)
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.White * GameConstants.Opacity.MediumHigh);

            // Draw game entities (frozen in place)
            DrawGameWorld(spriteBatch);

            // Draw semi-transparent overlay
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.Black * GameConstants.Opacity.Medium);

            // Draw pause menu
            DrawPauseMenu(spriteBatch);
        }

        /// <summary>
        /// Draws the frozen game world (all entities in their paused positions)
        /// </summary>
        private void DrawGameWorld(SpriteBatch spriteBatch)
        {
            // Draw doors (behind players)
            _door1.Draw(spriteBatch);
            _door2.Draw(spriteBatch);

            // Draw animated flames
            foreach (var flame in _flames)
            {
                flame.Draw(spriteBatch);
            }

            // Draw animated ice shards
            foreach (var iceShard in _iceShards)
            {
                iceShard.Draw(spriteBatch);
            }

            // Draw keys
            _key1.Draw(spriteBatch);
            _key2.Draw(spriteBatch);

            // Draw players
            _player.Draw(spriteBatch);
            _player2.Draw(spriteBatch);
        }

        /// <summary>
        /// Draws the pause menu with title, options, and controls hint
        /// </summary>
        private void DrawPauseMenu(SpriteBatch spriteBatch)
        {
            // Pause menu dimensions and positioning
            int menuWidth = GameConstants.Menu.MenuWidth;
            int menuHeight = GameConstants.Menu.MenuHeight;
            int menuX = (ScreenWidth - menuWidth) / 2;
            int menuY = (ScreenHeight - menuHeight) / 2;

            // Draw semi-transparent menu background
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Black * GameConstants.Opacity.VeryHigh);

            // Draw menu border
            DrawRectangleBorder(spriteBatch, _pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Cyan,
                GameConstants.Menu.BorderThickness);

            // Draw menu content if font is available
            if (_font != null)
            {
                DrawTitle(spriteBatch, menuX, menuY, menuWidth);
                DrawMenuOptions(spriteBatch, menuX, menuY, menuWidth);
                DrawControlsHint(spriteBatch);
            }
            else
            {
                // Fallback: Draw simple colored blocks if no font
                DrawFallbackMenu(spriteBatch, menuX, menuY, menuWidth);
            }
        }

        /// <summary>
        /// Draws the "PAUSED" title
        /// </summary>
        private void DrawTitle(SpriteBatch spriteBatch, int menuX, int menuY, int menuWidth)
        {
            string title = "PAUSED";
            Vector2 titleSize = _font.MeasureString(title);
            float titleScale = 2.5f;
            Vector2 titlePosition = new Vector2(
                menuX + (menuWidth - titleSize.X * titleScale) / 2,
                menuY + 20
            );

            // Draw title shadow
            spriteBatch.DrawString(_font, title,
                titlePosition + new Vector2(2, 2),
                Color.Black, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

            // Draw title
            spriteBatch.DrawString(_font, title,
                titlePosition,
                Color.Cyan, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the menu options with selection highlighting
        /// </summary>
        private void DrawMenuOptions(SpriteBatch spriteBatch, int menuX, int menuY, int menuWidth)
        {
            int firstOptionY = menuY + GameConstants.Menu.FirstOptionY;
            float optionScale = GameConstants.UI.MenuOptionScale;

            for (int i = 0; i < _pauseOptions.Length; i++)
            {
                string option = _pauseOptions[i];
                Vector2 optionSize = _font.MeasureString(option);
                Vector2 optionPosition = new Vector2(
                    menuX + (menuWidth - optionSize.X * optionScale) / 2,
                    firstOptionY + i * GameConstants.Menu.OptionSpacing
                );

                // Determine color based on selection
                Color optionColor = (_selectedPauseOption == i) ? Color.Yellow : Color.White;

                // Draw selection indicator
                if (_selectedPauseOption == i)
                {
                    // Draw highlight background
                    DrawFilledRectangle(spriteBatch, _pixelTexture,
                        new Rectangle((int)optionPosition.X - 10, (int)optionPosition.Y - 5,
                                     (int)(optionSize.X * optionScale) + 20, (int)(optionSize.Y * optionScale) + 10),
                        Color.Cyan * GameConstants.Opacity.VeryLight);

                    // Draw arrow indicator
                    string arrow = ">";
                    spriteBatch.DrawString(_font, arrow,
                        new Vector2(optionPosition.X - 30, optionPosition.Y),
                        Color.Yellow, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                }

                // Draw option shadow
                spriteBatch.DrawString(_font, option,
                    optionPosition + new Vector2(2, 2),
                    Color.Black, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);

                // Draw option text
                spriteBatch.DrawString(_font, option,
                    optionPosition,
                    optionColor, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the controls hint at the bottom of the screen
        /// </summary>
        private void DrawControlsHint(SpriteBatch spriteBatch)
        {
            string controlsHint = "W/S or Arrows to navigate | Enter/Space to select | ESC to resume";
            Vector2 hintSize = _font.MeasureString(controlsHint);
            Vector2 hintPosition = new Vector2(
                (ScreenWidth - hintSize.X) / 2,
                ScreenHeight - 30
            );

            spriteBatch.DrawString(_font, controlsHint,
                hintPosition,
                Color.White * GameConstants.Opacity.High, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a fallback menu using colored blocks when no font is available
        /// </summary>
        private void DrawFallbackMenu(SpriteBatch spriteBatch, int menuX, int menuY, int menuWidth)
        {
            int blockHeight = 40;
            int blockWidth = 200;
            int blockX = menuX + (menuWidth - blockWidth) / 2;
            int firstBlockY = menuY + 80;
            int blockSpacing = 60;

            // Resume option
            Color resumeColor = (_selectedPauseOption == 0) ? Color.Green : Color.DarkGreen;
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(blockX, firstBlockY, blockWidth, blockHeight),
                resumeColor);

            // Exit option
            Color exitColor = (_selectedPauseOption == 1) ? Color.Red : Color.DarkRed;
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(blockX, firstBlockY + blockSpacing, blockWidth, blockHeight),
                exitColor);
        }
    }
}
