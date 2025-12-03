using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace fire_and_ice.States
{
    /// <summary>
    /// Defines the interface for game states in the state machine pattern.
    /// Each game state (MainMenu, Playing, Paused, GameOver, Victory) implements this interface
    /// to provide consistent lifecycle management and behavior.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Called when entering this state.
        /// Use this to initialize state-specific resources, reset variables, and perform setup.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when exiting this state.
        /// Use this to clean up state-specific resources and perform any necessary teardown.
        /// </summary>
        void Exit();

        /// <summary>
        /// Updates the state's logic.
        /// Called every frame to update animations, timers, and game logic specific to this state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Renders the state to the screen.
        /// Called every frame to draw all visual elements for this state.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Processes keyboard input for this state.
        /// Called every frame to handle user input and respond to keyboard events.
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState);
    }
}
