using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace fire_and_ice
{
    /// <summary>
    /// Visual tool to adjust platform positions and sizes in real-time
    /// Press E to enable editor mode, then use mouse to select and adjust platforms
    /// </summary>
    public class PlatformEditor
    {
        private List<Rectangle> _platforms;
        private int _selectedPlatformIndex = -1;
        private bool _isEditorActive = false;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private Point _dragOffset;

        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        // Editor modes
        private enum EditMode
        {
            Move,
            Resize,
            Delete,
            Add
        }
        private EditMode _currentMode = EditMode.Move;

        public bool IsActive => _isEditorActive;

        public PlatformEditor(List<Rectangle> platforms)
        {
            _platforms = platforms;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            // Toggle editor with E key
            if (keyboardState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E))
            {
                _isEditorActive = !_isEditorActive;
                _selectedPlatformIndex = -1;
            }

            if (!_isEditorActive)
            {
                _previousMouseState = mouseState;
                _previousKeyboardState = keyboardState;
                return;
            }

            // Switch modes with number keys
            if (keyboardState.IsKeyDown(Keys.D1)) _currentMode = EditMode.Move;
            if (keyboardState.IsKeyDown(Keys.D2)) _currentMode = EditMode.Resize;
            if (keyboardState.IsKeyDown(Keys.D3)) _currentMode = EditMode.Delete;
            if (keyboardState.IsKeyDown(Keys.D4)) _currentMode = EditMode.Add;

            Point mousePos = mouseState.Position;

            // Select platform with left click
            if (mouseState.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                _selectedPlatformIndex = FindPlatformAtPosition(mousePos);

                if (_selectedPlatformIndex >= 0)
                {
                    var platform = _platforms[_selectedPlatformIndex];
                    _dragOffset = new Point(mousePos.X - platform.X, mousePos.Y - platform.Y);

                    if (_currentMode == EditMode.Move)
                    {
                        _isDragging = true;
                    }
                    else if (_currentMode == EditMode.Resize)
                    {
                        _isResizing = true;
                    }
                    else if (_currentMode == EditMode.Delete)
                    {
                        _platforms.RemoveAt(_selectedPlatformIndex);
                        _selectedPlatformIndex = -1;
                    }
                }
                else if (_currentMode == EditMode.Add)
                {
                    // Add new platform at mouse position
                    _platforms.Add(new Rectangle(mousePos.X, mousePos.Y, 100, 20));
                    _selectedPlatformIndex = _platforms.Count - 1;
                }
            }

            // Drag/Resize platform
            if (mouseState.LeftButton == ButtonState.Pressed && _selectedPlatformIndex >= 0)
            {
                var platform = _platforms[_selectedPlatformIndex];

                if (_isDragging)
                {
                    platform.X = mousePos.X - _dragOffset.X;
                    platform.Y = mousePos.Y - _dragOffset.Y;
                    _platforms[_selectedPlatformIndex] = platform;
                }
                else if (_isResizing)
                {
                    platform.Width = Math.Max(10, mousePos.X - platform.X);
                    platform.Height = Math.Max(10, mousePos.Y - platform.Y);
                    _platforms[_selectedPlatformIndex] = platform;
                }
            }

            // Release drag/resize
            if (mouseState.LeftButton == ButtonState.Released)
            {
                if (_isDragging || _isResizing)
                {
                    PrintPlatformCode();
                }
                _isDragging = false;
                _isResizing = false;
            }

            // Fine adjustment with arrow keys
            if (_selectedPlatformIndex >= 0)
            {
                var platform = _platforms[_selectedPlatformIndex];
                bool changed = false;

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    platform.X -= 1;
                    changed = true;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    platform.X += 1;
                    changed = true;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    platform.Y -= 1;
                    changed = true;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    platform.Y += 1;
                    changed = true;
                }

                // Size adjustment with Shift + Arrows
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                {
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        platform.Width -= 1;
                        changed = true;
                    }
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        platform.Width += 1;
                        changed = true;
                    }
                    if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        platform.Height -= 1;
                        changed = true;
                    }
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        platform.Height += 1;
                        changed = true;
                    }
                }

                if (changed)
                {
                    _platforms[_selectedPlatformIndex] = platform;
                }
            }

            // Print code with P key
            if (keyboardState.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P))
            {
                PrintPlatformCode();
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        private int FindPlatformAtPosition(Point position)
        {
            for (int i = _platforms.Count - 1; i >= 0; i--)
            {
                if (_platforms[i].Contains(position))
                {
                    return i;
                }
            }
            return -1;
        }

        private void PrintPlatformCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n// Generated Platform Code:");
            sb.AppendLine("public static List<Rectangle> GetLevel1Platforms()");
            sb.AppendLine("{");
            sb.AppendLine("    List<Rectangle> platforms = new List<Rectangle>();");
            sb.AppendLine();

            for (int i = 0; i < _platforms.Count; i++)
            {
                var p = _platforms[i];
                sb.AppendLine($"    platforms.Add(new Rectangle({p.X}, {p.Y}, {p.Width}, {p.Height}));");
            }

            sb.AppendLine();
            sb.AppendLine("    return platforms;");
            sb.AppendLine("}");

            System.Diagnostics.Debug.WriteLine(sb.ToString());
            Console.WriteLine(sb.ToString());
        }

        public string GetHelpText()
        {
            if (!_isEditorActive) return "Press E to enter Platform Editor";

            StringBuilder help = new StringBuilder();
            help.AppendLine("=== PLATFORM EDITOR ===");
            help.AppendLine("E - Exit Editor");
            help.AppendLine("P - Print Code to Console");
            help.AppendLine();
            help.AppendLine("Modes:");
            help.AppendLine("1 - Move Mode");
            help.AppendLine("2 - Resize Mode");
            help.AppendLine("3 - Delete Mode");
            help.AppendLine("4 - Add Mode");
            help.AppendLine();
            help.AppendLine($"Current Mode: {_currentMode}");
            help.AppendLine();
            help.AppendLine("Controls:");
            help.AppendLine("Left Click - Select/Move/Resize");
            help.AppendLine("Arrow Keys - Fine Adjust Position");
            help.AppendLine("Shift+Arrows - Fine Adjust Size");

            if (_selectedPlatformIndex >= 0)
            {
                var p = _platforms[_selectedPlatformIndex];
                help.AppendLine();
                help.AppendLine($"Selected Platform {_selectedPlatformIndex}:");
                help.AppendLine($"  X: {p.X}, Y: {p.Y}");
                help.AppendLine($"  W: {p.Width}, H: {p.Height}");
            }

            return help.ToString();
        }

        public Color GetPlatformColor(int index)
        {
            if (index == _selectedPlatformIndex)
            {
                return _currentMode switch
                {
                    EditMode.Move => Color.Yellow,
                    EditMode.Resize => Color.Orange,
                    EditMode.Delete => Color.Red,
                    _ => Color.White
                };
            }
            return Color.Cyan;
        }
    }
}