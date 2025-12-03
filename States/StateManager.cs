using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice.States
{
    /// <summary>
    /// Manages game states and transitions between them.
    /// Handles state lifecycle (Enter/Exit) and delegates Update/Draw/HandleInput calls to the current state.
    /// </summary>
    public class StateManager
    {
        private readonly Dictionary<GameState, IGameState> _states;
        private IGameState _currentState;
        private GameState _currentStateEnum;
        private KeyboardState _previousKeyboardState;

        /// <summary>
        /// Gets the current game state enum value
        /// </summary>
        public GameState CurrentState => _currentStateEnum;

        /// <summary>
        /// Creates a new state manager
        /// </summary>
        public StateManager()
        {
            _states = new Dictionary<GameState, IGameState>();
            _previousKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// Registers a state with the state manager
        /// </summary>
        /// <param name="stateEnum">The game state enum value</param>
        /// <param name="state">The state implementation</param>
        public void RegisterState(GameState stateEnum, IGameState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (_states.ContainsKey(stateEnum))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: State {stateEnum} is already registered. Replacing.");
            }

            _states[stateEnum] = state;
            System.Diagnostics.Debug.WriteLine($"Registered state: {stateEnum}");
        }

        /// <summary>
        /// Changes to a new state, calling Exit on the old state and Enter on the new state
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        public void ChangeState(GameState newState)
        {
            if (!_states.ContainsKey(newState))
            {
                throw new InvalidOperationException($"State {newState} is not registered with the StateManager.");
            }

            // Exit current state
            if (_currentState != null)
            {
                System.Diagnostics.Debug.WriteLine($"Exiting state: {_currentStateEnum}");
                _currentState.Exit();
            }

            // Change to new state
            _currentStateEnum = newState;
            _currentState = _states[newState];

            // Enter new state
            System.Diagnostics.Debug.WriteLine($"Entering state: {_currentStateEnum}");
            _currentState.Enter();
        }

        /// <summary>
        /// Sets the initial state without calling Enter/Exit
        /// Call ChangeState after this to properly enter the initial state
        /// </summary>
        /// <param name="initialState">The initial state enum value</param>
        public void SetInitialState(GameState initialState)
        {
            if (!_states.ContainsKey(initialState))
            {
                throw new InvalidOperationException($"State {initialState} is not registered with the StateManager.");
            }

            _currentStateEnum = initialState;
            _currentState = _states[initialState];
            System.Diagnostics.Debug.WriteLine($"Initial state set to: {_currentStateEnum}");
        }

        /// <summary>
        /// Updates the current state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            if (_currentState == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: No current state set in StateManager.Update()");
                return;
            }

            _currentState.Update(gameTime);
        }

        /// <summary>
        /// Handles input for the current state
        /// </summary>
        public void HandleInput()
        {
            if (_currentState == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: No current state set in StateManager.HandleInput()");
                return;
            }

            KeyboardState currentKeyboardState = Keyboard.GetState();
            _currentState.HandleInput(currentKeyboardState, _previousKeyboardState);
            _previousKeyboardState = currentKeyboardState;
        }

        /// <summary>
        /// Draws the current state
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentState == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: No current state set in StateManager.Draw()");
                return;
            }

            _currentState.Draw(spriteBatch);
        }

        /// <summary>
        /// Gets the current state implementation (for testing or advanced scenarios)
        /// </summary>
        /// <returns>The current IGameState implementation</returns>
        public IGameState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Checks if a state is registered
        /// </summary>
        /// <param name="state">The state enum to check</param>
        /// <returns>True if the state is registered</returns>
        public bool IsStateRegistered(GameState state)
        {
            return _states.ContainsKey(state);
        }

        /// <summary>
        /// Gets the number of registered states
        /// </summary>
        public int RegisteredStateCount => _states.Count;
    }
}
