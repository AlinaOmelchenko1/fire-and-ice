using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace fire_and_ice.States
{
    /// <summary>
    /// Represents the main menu state where players can start the game or exit.
    /// Displays the start page background with menu options.
    /// </summary>
    public class MainMenuState : BaseGameState
    {
        private readonly Texture2D _startPageTexture;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _font;
        private readonly Action<GameState> _changeState;
        private readonly Action _exitGame;

        private int _selectedMenuOption;
        private readonly string[] _menuOptions = { "START", "EXIT" };

        /// <summary>
        /// Creates a new main menu state
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        /// <param name="startPageTexture">Background texture for the menu</param>
        /// <param name="pixelTexture">1x1 white pixel texture for UI elements</param>
        /// <param name="font">Font for menu text</param>
        /// <param name="changeState">Callback to change game state</param>
        /// <param name="exitGame">Callback to exit the game</param>
        public MainMenuState(
            Game1 game,
            SpriteBatch spriteBatch,
            Texture2D startPageTexture,
            Texture2D pixelTexture,
            SpriteFont font,
            Action<GameState> changeState,
            Action exitGame) : base(game, spriteBatch)
        {
            _startPageTexture = startPageTexture;
            _pixelTexture = pixelTexture;
            _font = font;
            _changeState = changeState;
            _exitGame = exitGame;
            _selectedMenuOption = 0;
        }

        /// <summary>
        /// Called when entering the main menu state.
        /// Resets the selected menu option to the first item.
        /// </summary>
        public override void Enter()
        {
            _selectedMenuOption = 0;
            System.Diagnostics.Debug.WriteLine("Entered MainMenu state");
        }

        /// <summary>
        /// Updates the main menu logic (currently no time-based updates needed)
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            // No time-based updates needed for main menu
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
                _selectedMenuOption = (_selectedMenuOption + 1) % GameConstants.Menu.MainMenuOptionsCount;
            }

            // Navigate menu up with Up or W keys
            if (IsKeyPressed(Keys.Up, keyboardState, previousKeyboardState) ||
                IsKeyPressed(Keys.W, keyboardState, previousKeyboardState))
            {
                _selectedMenuOption = (_selectedMenuOption - 1 + GameConstants.Menu.MainMenuOptionsCount) % GameConstants.Menu.MainMenuOptionsCount;
            }

            // Select option with Enter or Space
            if (IsKeyPressed(Keys.Enter, keyboardState, previousKeyboardState) ||
                IsKeyPressed(Keys.Space, keyboardState, previousKeyboardState))
            {
                SelectCurrentOption();
            }

            // Allow Escape to exit from main menu
            if (IsKeyPressed(Keys.Escape, keyboardState, previousKeyboardState))
            {
                _exitGame?.Invoke();
            }
        }

        /// <summary>
        /// Executes the action for the currently selected menu option
        /// </summary>
        private void SelectCurrentOption()
        {
            switch (_selectedMenuOption)
            {
                case 0: // START
                    System.Diagnostics.Debug.WriteLine("Starting game from main menu");
                    _changeState?.Invoke(GameState.Playing);
                    break;

                case 1: // EXIT
                    _exitGame?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Renders the main menu to the screen
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw start page background
            spriteBatch.Draw(_startPageTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.White);

            // Menu dimensions and positioning
            int menuWidth = GameConstants.Menu.MenuWidth;
            int menuHeight = GameConstants.Menu.MenuHeight;
            int menuX = (ScreenWidth - menuWidth) / 2;
            int menuY = (ScreenHeight - menuHeight) / 2;

            // Draw semi-transparent menu background
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Black * GameConstants.Opacity.High);

            // Draw menu border
            DrawRectangleBorder(spriteBatch, _pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Gold,
                GameConstants.Menu.BorderThickness);

            // Draw menu options if font is available
            if (_font != null)
            {
                DrawMenuOptions(spriteBatch, menuX, menuY, menuWidth);
                DrawControlsHint(spriteBatch);
            }
        }

        /// <summary>
        /// Draws the menu options with selection highlighting
        /// </summary>
        private void DrawMenuOptions(SpriteBatch spriteBatch, int menuX, int menuY, int menuWidth)
        {
            int firstOptionY = menuY + GameConstants.Menu.FirstOptionY;
            float optionScale = GameConstants.UI.MenuOptionScale;

            for (int i = 0; i < _menuOptions.Length; i++)
            {
                string option = _menuOptions[i];
                Vector2 optionSize = _font.MeasureString(option);
                Vector2 optionPosition = new Vector2(
                    menuX + (menuWidth - optionSize.X * optionScale) / 2,
                    firstOptionY + i * GameConstants.Menu.OptionSpacing
                );

                // Determine color based on selection
                Color optionColor = (_selectedMenuOption == i) ? Color.Yellow : Color.White;

                // Draw selection indicator
                if (_selectedMenuOption == i)
                {
                    // Draw highlight background
                    DrawFilledRectangle(spriteBatch, _pixelTexture,
                        new Rectangle((int)optionPosition.X - 10, (int)optionPosition.Y - 5,
                                     (int)(optionSize.X * optionScale) + 20, (int)(optionSize.Y * optionScale) + 10),
                        Color.Orange * GameConstants.Opacity.VeryLight);

                    // Draw arrow indicator
                    string arrow = ">";
                    spriteBatch.DrawString(_font, arrow,
                        new Vector2(optionPosition.X - 30, optionPosition.Y),
                        Color.Yellow, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                }

                // Draw option shadow for depth
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
            string controlsHint = "Use W/S or Arrow Keys to navigate, Enter/Space to select";
            Vector2 hintSize = _font.MeasureString(controlsHint);
            Vector2 hintPosition = new Vector2(
                (ScreenWidth - hintSize.X) / 2,
                ScreenHeight - 30
            );

            spriteBatch.DrawString(_font, controlsHint,
                hintPosition,
                Color.White * GameConstants.Opacity.High, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
