using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice.Systems
{
    /// <summary>
    /// Represents a game action that can be triggered by input
    /// </summary>
    public enum GameAction
    {
        MoveLeft,
        MoveRight,
        Jump,
        Interact,
        Pause,
        Confirm,
        Cancel,
        MenuUp,
        MenuDown,
        ToggleDebug,
        ToggleHitboxes,
        ToggleTimer,
        Restart
    }

    /// <summary>
    /// Represents the state of an input action
    /// </summary>
    public enum InputState
    {
        /// <summary>
        /// Input is not currently pressed
        /// </summary>
        Released,

        /// <summary>
        /// Input was just pressed this frame
        /// </summary>
        Pressed,

        /// <summary>
        /// Input is being held down
        /// </summary>
        Held,

        /// <summary>
        /// Input was just released this frame
        /// </summary>
        JustReleased
    }

    /// <summary>
    /// Centralized input management system with action mapping and rebinding support.
    /// Handles keyboard state tracking, action mapping, and input queries.
    /// </summary>
    public class InputSystem
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private readonly Dictionary<GameAction, List<Keys>> _actionBindings;
        private readonly Dictionary<Keys, InputState> _keyStates;

        /// <summary>
        /// Gets the current keyboard state
        /// </summary>
        public KeyboardState CurrentKeyboardState => _currentKeyboardState;

        /// <summary>
        /// Gets the previous frame's keyboard state
        /// </summary>
        public KeyboardState PreviousKeyboardState => _previousKeyboardState;

        /// <summary>
        /// Gets or sets whether input is enabled (can be used to disable input during cutscenes, etc.)
        /// </summary>
        public bool InputEnabled { get; set; }

        /// <summary>
        /// Creates a new input system with default key bindings
        /// </summary>
        public InputSystem()
        {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
            _actionBindings = new Dictionary<GameAction, List<Keys>>();
            _keyStates = new Dictionary<Keys, InputState>();
            InputEnabled = true;

            // Set up default key bindings
            SetupDefaultBindings();
        }

        /// <summary>
        /// Sets up default key bindings for all game actions
        /// </summary>
        private void SetupDefaultBindings()
        {
            // Movement
            BindAction(GameAction.MoveLeft, Keys.A, Keys.Left);
            BindAction(GameAction.MoveRight, Keys.D, Keys.Right);
            BindAction(GameAction.Jump, Keys.W, Keys.Up, Keys.Space);

            // Game controls
            BindAction(GameAction.Pause, Keys.Escape, Keys.P);
            BindAction(GameAction.Interact, Keys.E, Keys.Enter);
            BindAction(GameAction.Restart, Keys.R);

            // Menu controls
            BindAction(GameAction.MenuUp, Keys.W, Keys.Up);
            BindAction(GameAction.MenuDown, Keys.S, Keys.Down);
            BindAction(GameAction.Confirm, Keys.Enter, Keys.Space);
            BindAction(GameAction.Cancel, Keys.Escape);

            // Debug controls
            BindAction(GameAction.ToggleDebug, Keys.F1);
            BindAction(GameAction.ToggleHitboxes, Keys.H);
            BindAction(GameAction.ToggleTimer, Keys.T);
        }

        /// <summary>
        /// Binds one or more keys to a game action
        /// </summary>
        /// <param name="action">The game action to bind</param>
        /// <param name="keys">The keys to bind to this action</param>
        public void BindAction(GameAction action, params Keys[] keys)
        {
            if (!_actionBindings.ContainsKey(action))
            {
                _actionBindings[action] = new List<Keys>();
            }

            foreach (var key in keys)
            {
                if (!_actionBindings[action].Contains(key))
                {
                    _actionBindings[action].Add(key);
                }
            }
        }

        /// <summary>
        /// Unbinds all keys from a game action
        /// </summary>
        /// <param name="action">The game action to unbind</param>
        public void UnbindAction(GameAction action)
        {
            if (_actionBindings.ContainsKey(action))
            {
                _actionBindings[action].Clear();
            }
        }

        /// <summary>
        /// Unbinds a specific key from a game action
        /// </summary>
        /// <param name="action">The game action</param>
        /// <param name="key">The key to unbind</param>
        public void UnbindKey(GameAction action, Keys key)
        {
            if (_actionBindings.ContainsKey(action))
            {
                _actionBindings[action].Remove(key);
            }
        }

        /// <summary>
        /// Gets all keys bound to a game action
        /// </summary>
        /// <param name="action">The game action</param>
        /// <returns>List of keys bound to the action</returns>
        public List<Keys> GetBoundKeys(GameAction action)
        {
            if (_actionBindings.ContainsKey(action))
            {
                return new List<Keys>(_actionBindings[action]);
            }
            return new List<Keys>();
        }

        /// <summary>
        /// Resets all key bindings to default
        /// </summary>
        public void ResetToDefaults()
        {
            _actionBindings.Clear();
            SetupDefaultBindings();
        }

        /// <summary>
        /// Updates the input system. Call this once per frame.
        /// </summary>
        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            // Update key states for all pressed keys
            UpdateKeyStates();
        }

        /// <summary>
        /// Updates the state of all keys
        /// </summary>
        private void UpdateKeyStates()
        {
            var allKeys = Enum.GetValues(typeof(Keys));
            foreach (Keys key in allKeys)
            {
                bool currentlyDown = _currentKeyboardState.IsKeyDown(key);
                bool previouslyDown = _previousKeyboardState.IsKeyDown(key);

                if (currentlyDown && !previouslyDown)
                {
                    _keyStates[key] = InputState.Pressed;
                }
                else if (currentlyDown && previouslyDown)
                {
                    _keyStates[key] = InputState.Held;
                }
                else if (!currentlyDown && previouslyDown)
                {
                    _keyStates[key] = InputState.JustReleased;
                }
                else
                {
                    _keyStates[key] = InputState.Released;
                }
            }
        }

        /// <summary>
        /// Checks if a game action was just pressed this frame
        /// </summary>
        /// <param name="action">The game action to check</param>
        /// <returns>True if the action was just pressed</returns>
        public bool IsActionPressed(GameAction action)
        {
            if (!InputEnabled) return false;

            if (_actionBindings.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (IsKeyPressed(key))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a game action is currently being held
        /// </summary>
        /// <param name="action">The game action to check</param>
        /// <returns>True if the action is being held</returns>
        public bool IsActionHeld(GameAction action)
        {
            if (!InputEnabled) return false;

            if (_actionBindings.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (IsKeyHeld(key))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a game action was just released this frame
        /// </summary>
        /// <param name="action">The game action to check</param>
        /// <returns>True if the action was just released</returns>
        public bool IsActionReleased(GameAction action)
        {
            if (!InputEnabled) return false;

            if (_actionBindings.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (IsKeyReleased(key))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a game action is in any active state (pressed or held)
        /// </summary>
        /// <param name="action">The game action to check</param>
        /// <returns>True if the action is active</returns>
        public bool IsActionActive(GameAction action)
        {
            return IsActionPressed(action) || IsActionHeld(action);
        }

        /// <summary>
        /// Checks if a specific key was just pressed this frame
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key was just pressed</returns>
        public bool IsKeyPressed(Keys key)
        {
            if (!InputEnabled) return false;
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key is currently being held
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key is being held</returns>
        public bool IsKeyHeld(Keys key)
        {
            if (!InputEnabled) return false;
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key was just released this frame
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key was just released</returns>
        public bool IsKeyReleased(Keys key)
        {
            if (!InputEnabled) return false;
            return !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key is down (pressed or held)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key is down</returns>
        public bool IsKeyDown(Keys key)
        {
            if (!InputEnabled) return false;
            return _currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Gets the state of a specific key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>The input state of the key</returns>
        public InputState GetKeyState(Keys key)
        {
            if (!InputEnabled) return InputState.Released;
            return _keyStates.TryGetValue(key, out var state) ? state : InputState.Released;
        }

        /// <summary>
        /// Gets the state of a game action
        /// </summary>
        /// <param name="action">The game action to check</param>
        /// <returns>The input state of the action (based on first bound key that's active)</returns>
        public InputState GetActionState(GameAction action)
        {
            if (!InputEnabled) return InputState.Released;

            if (_actionBindings.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    var state = GetKeyState(key);
                    if (state != InputState.Released)
                        return state;
                }
            }
            return InputState.Released;
        }

        /// <summary>
        /// Gets a horizontal axis value based on left/right movement actions (-1, 0, or 1)
        /// </summary>
        /// <returns>-1 for left, 1 for right, 0 for neither or both</returns>
        public float GetHorizontalAxis()
        {
            bool left = IsActionActive(GameAction.MoveLeft);
            bool right = IsActionActive(GameAction.MoveRight);

            if (left && !right) return -1f;
            if (right && !left) return 1f;
            return 0f;
        }

        /// <summary>
        /// Gets a vertical axis value based on up/down menu navigation (-1, 0, or 1)
        /// </summary>
        /// <returns>-1 for up, 1 for down, 0 for neither or both</returns>
        public float GetVerticalAxis()
        {
            bool up = IsActionActive(GameAction.MenuUp);
            bool down = IsActionActive(GameAction.MenuDown);

            if (up && !down) return -1f;
            if (down && !up) return 1f;
            return 0f;
        }

        /// <summary>
        /// Checks if any key was pressed this frame
        /// </summary>
        /// <returns>True if any key was pressed</returns>
        public bool AnyKeyPressed()
        {
            if (!InputEnabled) return false;

            var currentKeys = _currentKeyboardState.GetPressedKeys();
            var previousKeys = _previousKeyboardState.GetPressedKeys();

            return currentKeys.Length > previousKeys.Length;
        }

        /// <summary>
        /// Gets all keys that were pressed this frame
        /// </summary>
        /// <returns>Array of keys pressed this frame</returns>
        public Keys[] GetPressedKeys()
        {
            if (!InputEnabled) return Array.Empty<Keys>();

            var pressedKeys = new List<Keys>();
            foreach (var kvp in _keyStates)
            {
                if (kvp.Value == InputState.Pressed)
                    pressedKeys.Add(kvp.Key);
            }
            return pressedKeys.ToArray();
        }

        /// <summary>
        /// Waits for a key press (useful for key rebinding)
        /// Returns the first key pressed, or null if no key is pressed
        /// </summary>
        /// <returns>The first key pressed, or null</returns>
        public Keys? WaitForKeyPress()
        {
            var pressedKeys = GetPressedKeys();
            return pressedKeys.Length > 0 ? pressedKeys[0] : null;
        }

        /// <summary>
        /// Gets a human-readable string for a key
        /// </summary>
        /// <param name="key">The key to get the name for</param>
        /// <returns>Human-readable key name</returns>
        public static string GetKeyName(Keys key)
        {
            return key switch
            {
                Keys.Space => "Space",
                Keys.Enter => "Enter",
                Keys.Escape => "Escape",
                Keys.Back => "Backspace",
                Keys.Tab => "Tab",
                Keys.LeftShift => "Left Shift",
                Keys.RightShift => "Right Shift",
                Keys.LeftControl => "Left Ctrl",
                Keys.RightControl => "Right Ctrl",
                Keys.LeftAlt => "Left Alt",
                Keys.RightAlt => "Right Alt",
                Keys.Up => "Up Arrow",
                Keys.Down => "Down Arrow",
                Keys.Left => "Left Arrow",
                Keys.Right => "Right Arrow",
                _ => key.ToString()
            };
        }

        /// <summary>
        /// Gets diagnostic information about the input system
        /// </summary>
        /// <returns>Diagnostic string</returns>
        public string GetDiagnostics()
        {
            int pressedCount = 0;
            int heldCount = 0;
            foreach (var state in _keyStates.Values)
            {
                if (state == InputState.Pressed) pressedCount++;
                if (state == InputState.Held) heldCount++;
            }

            return $"Input: Enabled={InputEnabled}, Actions={_actionBindings.Count}, " +
                   $"Pressed={pressedCount}, Held={heldCount}";
        }
    }
}
