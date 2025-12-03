using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace fire_and_ice.States
{
    /// <summary>
    /// Abstract base class for all game states.
    /// Provides common functionality and resource access that all states need.
    /// Concrete states should inherit from this class and override methods as needed.
    /// </summary>
    public abstract class BaseGameState : IGameState
    {
        /// <summary>
        /// Reference to the main game instance for accessing shared resources
        /// </summary>
        protected Game1 Game { get; private set; }

        /// <summary>
        /// Quick access to the graphics device for viewport and screen information
        /// </summary>
        protected GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

        /// <summary>
        /// Quick access to the sprite batch for rendering
        /// </summary>
        protected SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Width of the game viewport/screen
        /// </summary>
        protected int ScreenWidth => GraphicsDevice.Viewport.Width;

        /// <summary>
        /// Height of the game viewport/screen
        /// </summary>
        protected int ScreenHeight => GraphicsDevice.Viewport.Height;

        /// <summary>
        /// Center X coordinate of the screen
        /// </summary>
        protected int ScreenCenterX => ScreenWidth / 2;

        /// <summary>
        /// Center Y coordinate of the screen
        /// </summary>
        protected int ScreenCenterY => ScreenHeight / 2;

        /// <summary>
        /// Creates a new base game state
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        protected BaseGameState(Game1 game, SpriteBatch spriteBatch)
        {
            Game = game;
            SpriteBatch = spriteBatch;
        }

        /// <summary>
        /// Called when entering this state.
        /// Override to perform state-specific initialization.
        /// </summary>
        public virtual void Enter()
        {
            // Default implementation does nothing
            // Override in derived classes to add initialization logic
        }

        /// <summary>
        /// Called when exiting this state.
        /// Override to perform state-specific cleanup.
        /// </summary>
        public virtual void Exit()
        {
            // Default implementation does nothing
            // Override in derived classes to add cleanup logic
        }

        /// <summary>
        /// Updates the state's logic.
        /// Override to implement state-specific update behavior.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Renders the state to the screen.
        /// Override to implement state-specific rendering.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Processes keyboard input for this state.
        /// Override to implement state-specific input handling.
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        public abstract void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState);

        /// <summary>
        /// Helper method to detect if a key was just pressed (edge-triggered).
        /// Returns true only on the frame when the key transitions from up to down.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="currentState">Current keyboard state</param>
        /// <param name="previousState">Previous keyboard state</param>
        /// <returns>True if the key was just pressed this frame</returns>
        protected bool IsKeyPressed(Keys key, KeyboardState currentState, KeyboardState previousState)
        {
            return currentState.IsKeyDown(key) && previousState.IsKeyUp(key);
        }

        /// <summary>
        /// Helper method to detect if a key is currently being held down.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="currentState">Current keyboard state</param>
        /// <returns>True if the key is currently held down</returns>
        protected bool IsKeyDown(Keys key, KeyboardState currentState)
        {
            return currentState.IsKeyDown(key);
        }

        /// <summary>
        /// Helper method to draw centered text.
        /// Useful for menu options, titles, and messages.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="font">The font to use</param>
        /// <param name="text">The text to draw</param>
        /// <param name="y">The Y position (text will be centered horizontally)</param>
        /// <param name="color">The color to draw the text</param>
        /// <param name="scale">Optional scale factor (default 1.0)</param>
        protected void DrawCenteredText(SpriteBatch spriteBatch, SpriteFont font, string text, int y, Color color, float scale = 1.0f)
        {
            if (font == null)
                return;

            Vector2 textSize = font.MeasureString(text);
            Vector2 position = new Vector2(
                (ScreenWidth - textSize.X * scale) / 2,
                y
            );
            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Helper method to draw a filled rectangle.
        /// Useful for backgrounds, UI elements, and visual effects.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="pixelTexture">A 1x1 white pixel texture</param>
        /// <param name="rectangle">The rectangle to fill</param>
        /// <param name="color">The color to fill with</param>
        protected void DrawFilledRectangle(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rectangle, Color color)
        {
            if (pixelTexture != null)
            {
                spriteBatch.Draw(pixelTexture, rectangle, color);
            }
        }

        /// <summary>
        /// Helper method to draw a rectangle border.
        /// Useful for UI elements, menus, and highlights.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="pixelTexture">A 1x1 white pixel texture</param>
        /// <param name="rectangle">The rectangle to outline</param>
        /// <param name="color">The color of the border</param>
        /// <param name="thickness">The thickness of the border in pixels</param>
        protected void DrawRectangleBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rectangle, Color color, int thickness)
        {
            if (pixelTexture == null)
                return;

            // Top
            spriteBatch.Draw(pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(pixelTexture, new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
            // Left
            spriteBatch.Draw(pixelTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Right
            spriteBatch.Draw(pixelTexture, new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }
    }
}
