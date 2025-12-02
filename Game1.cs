using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice
{
    // Game State Machine
    public enum GameState
    {
        MainMenu,    // For start screen
        Playing,
        GameOver,
        Paused,      // For pause functionality
        Victory      // Level completed!
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GlobalTimer _collisionTimer;

        // Game State
        private GameState _currentState = GameState.MainMenu;
        private float _gameOverTimer = 0f;
        private const float GAME_OVER_DELAY = 2f; // Show game over for 2 seconds before allowing restart
        private float _victoryTimer = 0f;
        private const float VICTORY_DISPLAY_TIME = 5f; // Show victory screen for 5 seconds

        private Texture2D _levelTexture;
        private Texture2D _startPageTexture;
        private Texture2D _pixelTexture;
        private SpriteFont _debugFont;

        // Main Menu State
        private int _selectedMenuOption = 0; // 0 = Start, 1 = Exit
        private const int MENU_OPTIONS_COUNT = 2;

        // Pause Menu State
        private int _selectedPauseOption = 0; // 0 = Resume, 1 = Exit
        private const int PAUSE_OPTIONS_COUNT = 2;

        private Player _player;
        private Player _player2; // Second player (blue)
        private List<InteractableObject> _platforms;

        // Keys and Doors
        private Key _key1; // For player 1
        private Key _key2; // For player 2
        private Door _door1; // Left door
        private Door _door2; // Right door
        private bool _doorsOpening = false;

        // Flames and Ice Shards
        private List<Flame> _flames;
        private List<IceShard> _iceShards;

        private bool _showHitboxes = false;
        private bool _showTimerInfo = false;
        private KeyboardState _previousKeyboardState;

        private string _collisionMethod = "manual";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _collisionTimer = new GlobalTimer();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _levelTexture = Content.Load<Texture2D>("first_level");
            _startPageTexture = Content.Load<Texture2D>("start_page");
            Texture2D heroTexture = Content.Load<Texture2D>("hero_walk");
            Texture2D character2Texture = Content.Load<Texture2D>("character2");

            System.Diagnostics.Debug.WriteLine($"hero_walk dimensions: {heroTexture.Width}x{heroTexture.Height}, frame size: {heroTexture.Width/4}x{heroTexture.Height}");
            System.Diagnostics.Debug.WriteLine($"character2 dimensions: {character2Texture.Width}x{character2Texture.Height}, frame size: {character2Texture.Width/4}x{character2Texture.Height}");

            try
            {
                _debugFont = Content.Load<SpriteFont>("DebugFont");
                System.Diagnostics.Debug.WriteLine("DebugFont loaded successfully");
            }
            catch (Exception ex)
            {
                _debugFont = null;
                System.Diagnostics.Debug.WriteLine($"DebugFont failed to load: {ex.Message}");
            }

            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            if (_collisionMethod == "manual")
            {
                _platforms = LevelPlatforms.GetLevel1Platforms();
            }
            else
            {
                Texture2D collisionMapTexture;
                try
                {
                    collisionMapTexture = Content.Load<Texture2D>("first_level_collision");
                }
                catch
                {
                    collisionMapTexture = _levelTexture;
                }
                CollisionMapReader collisionReader = new CollisionMapReader(collisionMapTexture);
                _platforms = collisionReader.ExtractCollisionRectangles();
            }

            // Player 1 - Original (white/default color) - WASD + Space controls
            _player = new Player(heroTexture, new Vector2(85, 270));
            _player.Type = PlayerType.Fire; // Fire character
            _player.PlayerColor = Color.White;
            _player.MoveLeftKey = Keys.A;
            _player.MoveRightKey = Keys.D;
            _player.JumpKey1 = Keys.W;
            _player.JumpKey2 = Keys.Space;
            _player.JumpKey3 = Keys.None; // Not used

            // Player 2 - Ice character, spawns in opposite corner (right side) - Arrow keys
            _player2 = new Player(character2Texture, new Vector2(700, 270)); // 4 frames (default)
            _player2.Type = PlayerType.Ice; // Ice character
            _player2.PlayerColor = Color.White; // No color tinting needed
            _player2.MoveLeftKey = Keys.Left;
            _player2.MoveRightKey = Keys.Right;
            _player2.JumpKey1 = Keys.Up;
            _player2.JumpKey2 = Keys.RightControl;
            _player2.JumpKey3 = Keys.RightShift;

            // Initialize keys - spawn at specific locations
            _key1 = new Key(new Vector2(65, 315)); // Left upper corner of left wooden crate
            _key2 = new Key(new Vector2(625, 305)); // On right wooden crate

            // Initialize doors at top of map (matching green rectangles)
            _door1 = new Door(new Vector2(5, 65)); // Top left corner door (moved up 30px, left 5px)
            _door2 = new Door(new Vector2(737, 65)); // Top right corner door (moved up 30px, left 5px)

            // Initialize flames for all fire hazards
            _flames = new List<Flame>();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.Fire)
                {
                    _flames.Add(new Flame(platform.Bounds));
                }
            }

            // Initialize ice shards for all ice hazards
            _iceShards = new List<IceShard>();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.IceHazard)
                {
                    _iceShards.Add(new IceShard(platform.Bounds));
                }
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {_platforms.Count} platforms");
            System.Diagnostics.Debug.WriteLine($"Created {_flames.Count} animated flames");
            System.Diagnostics.Debug.WriteLine($"Created {_iceShards.Count} animated ice shards");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // State machine update
            switch (_currentState)
            {
                case GameState.Playing:
                    UpdatePlaying(gameTime, keyboardState);
                    break;

                case GameState.GameOver:
                    UpdateGameOver(gameTime, keyboardState);
                    break;

                case GameState.MainMenu:
                    UpdateMainMenu(gameTime, keyboardState);
                    break;

                case GameState.Paused:
                    UpdatePaused(gameTime, keyboardState);
                    break;

                case GameState.Victory:
                    UpdateVictory(gameTime, keyboardState);
                    break;
            }

            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void UpdatePlaying(GameTime gameTime, KeyboardState keyboardState)
        {
            // Handle pause
            if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _currentState = GameState.Paused;
                _selectedPauseOption = 0; // Reset to Resume
                System.Diagnostics.Debug.WriteLine("Game paused");
                return;
            }

            if (keyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H))
                _showHitboxes = !_showHitboxes;

            if (keyboardState.IsKeyDown(Keys.T) && !_previousKeyboardState.IsKeyDown(Keys.T))
                _showTimerInfo = !_showTimerInfo;

            _player.ProcessInput(keyboardState);
            _player2.ProcessInput(keyboardState);

            _collisionTimer.Update(gameTime, (fixedDeltaTime) =>
            {
                // Update Player 1
                _player.UpdatePhysics(fixedDeltaTime, GraphicsDevice.Viewport.Width);
                _player.CheckCollisions(_platforms);

                // Update Player 2
                _player2.UpdatePhysics(fixedDeltaTime, GraphicsDevice.Viewport.Width);
                _player2.CheckCollisions(_platforms);
            });

            _player.UpdateAnimation(gameTime);
            _player2.UpdateAnimation(gameTime);

            // Update keys
            _key1.Update(gameTime);
            _key2.Update(gameTime);

            // Check for key collection
            CheckKeyCollection(_key1, 1);
            CheckKeyCollection(_key2, 2);

            // Open doors when both keys are collected
            if (_key1.IsCollected && _key2.IsCollected && !_doorsOpening)
            {
                _doorsOpening = true;
                _door1.StartOpening();
                _door2.StartOpening();
                System.Diagnostics.Debug.WriteLine("Both keys collected - opening doors!");
            }

            // Update doors
            _door1.Update(gameTime);
            _door2.Update(gameTime);

            // Update flames
            foreach (var flame in _flames)
            {
                flame.Update(gameTime);
            }

            // Update ice shards
            foreach (var iceShard in _iceShards)
            {
                iceShard.Update(gameTime);
            }

            // Check for victory - both doors open and both players at their respective doors
            if (_door1.IsOpen && _door2.IsOpen)
            {
                Rectangle door1Bounds = _door1.GetBounds();
                Rectangle door2Bounds = _door2.GetBounds();
                Rectangle player1Hitbox = _player.GetHitbox();
                Rectangle player2Hitbox = _player2.GetHitbox();

                bool player1AtDoor1 = player1Hitbox.Intersects(door1Bounds);
                bool player2AtDoor2 = player2Hitbox.Intersects(door2Bounds);

                if (player1AtDoor1 && player2AtDoor2)
                {
                    System.Diagnostics.Debug.WriteLine("=== VICTORY! Both players reached their doors! ===");
                    _currentState = GameState.Victory;
                    _victoryTimer = 0f;
                    return;
                }
            }

            // Check for game over
            if (!_player.IsAlive || !_player2.IsAlive)
            {
                System.Diagnostics.Debug.WriteLine($"=== GAME OVER TRIGGERED ===");
                System.Diagnostics.Debug.WriteLine($"P1 Health: {_player.Health}, P1 Alive: {_player.IsAlive}");
                System.Diagnostics.Debug.WriteLine($"P2 Health: {_player2.Health}, P2 Alive: {_player2.IsAlive}");
                System.Diagnostics.Debug.WriteLine($"Switching to GameOver state");
                _currentState = GameState.GameOver;
                _gameOverTimer = 0f;
            }
        }

        private void UpdateGameOver(GameTime gameTime, KeyboardState keyboardState)
        {
            _gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine($"GameOver Update - Timer: {_gameOverTimer:F2}s");

            // Allow restart after delay
            if (_gameOverTimer >= GAME_OVER_DELAY)
            {
                if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space))
                {
                    System.Diagnostics.Debug.WriteLine("Restart key pressed - Restarting game");
                    RestartGame();
                }
            }
        }

        private void UpdateVictory(GameTime gameTime, KeyboardState keyboardState)
        {
            _victoryTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine($"Victory Update - Timer: {_victoryTimer:F2}s");

            // After display time, return to main menu
            if (_victoryTimer >= VICTORY_DISPLAY_TIME)
            {
                System.Diagnostics.Debug.WriteLine("Victory timer complete - Returning to main menu");
                _currentState = GameState.MainMenu;
                _selectedMenuOption = 0;

                // Reset game state for next play
                RestartGame();
                // Override the state back to MainMenu since RestartGame sets it to Playing
                _currentState = GameState.MainMenu;
            }
        }

        private void UpdateMainMenu(GameTime gameTime, KeyboardState keyboardState)
        {
            // Navigate menu with Up/Down or W/S keys
            if ((keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S)) &&
                !_previousKeyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.S))
            {
                _selectedMenuOption = (_selectedMenuOption + 1) % MENU_OPTIONS_COUNT;
            }

            if ((keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)) &&
                !_previousKeyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.W))
            {
                _selectedMenuOption = (_selectedMenuOption - 1 + MENU_OPTIONS_COUNT) % MENU_OPTIONS_COUNT;
            }

            // Select option with Enter or Space
            if ((keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space)) &&
                (!_previousKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Space)))
            {
                if (_selectedMenuOption == 0) // Start
                {
                    _currentState = GameState.Playing;
                    System.Diagnostics.Debug.WriteLine("Starting game from main menu");
                }
                else if (_selectedMenuOption == 1) // Exit
                {
                    Exit();
                }
            }

            // Also allow Escape to exit from main menu
            if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
        }

        private void UpdatePaused(GameTime gameTime, KeyboardState keyboardState)
        {
            // Navigate pause menu with Up/Down or W/S keys
            if ((keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S)) &&
                !_previousKeyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.S))
            {
                _selectedPauseOption = (_selectedPauseOption + 1) % PAUSE_OPTIONS_COUNT;
            }

            if ((keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)) &&
                !_previousKeyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.W))
            {
                _selectedPauseOption = (_selectedPauseOption - 1 + PAUSE_OPTIONS_COUNT) % PAUSE_OPTIONS_COUNT;
            }

            // Select option with Enter or Space
            if ((keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space)) &&
                (!_previousKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Space)))
            {
                if (_selectedPauseOption == 0) // Resume
                {
                    _currentState = GameState.Playing;
                    System.Diagnostics.Debug.WriteLine("Resuming game from pause menu");
                }
                else if (_selectedPauseOption == 1) // Exit
                {
                    Exit();
                }
            }

            // Also allow Escape to resume (toggle pause)
            if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _currentState = GameState.Playing;
                System.Diagnostics.Debug.WriteLine("Resuming game with ESC key");
            }
        }

        private void RestartGame()
        {
            System.Diagnostics.Debug.WriteLine("=== RESTARTING GAME ===");

            // Reset players completely
            _player.Reset(new Vector2(85, 270));
            _player2.Reset(new Vector2(700, 270));

            System.Diagnostics.Debug.WriteLine($"Players reset - P1: {_player.Health}HP, P2: {_player2.Health}HP");

            // Reset keys and doors
            _key1 = new Key(new Vector2(65, 315)); // Left upper corner of left wooden crate
            _key2 = new Key(new Vector2(625, 305)); // On right wooden crate
            _door1.Reset();
            _door2.Reset();
            _doorsOpening = false;

            // Reset flames
            _flames.Clear();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.Fire)
                {
                    _flames.Add(new Flame(platform.Bounds));
                }
            }

            // Reset game state
            _currentState = GameState.Playing;
            _gameOverTimer = 0f;

            System.Diagnostics.Debug.WriteLine("Game state set to Playing");
        }

        private void CheckKeyCollection(Key key, int keyNumber)
        {
            if (key.IsCollected)
                return;

            Rectangle player1Hitbox = _player.GetHitbox();
            Rectangle player2Hitbox = _player2.GetHitbox();

            if (player1Hitbox.Intersects(key.GetBounds()))
            {
                key.IsCollected = true;
                key.PlayerOwner = 1;
                System.Diagnostics.Debug.WriteLine($"Player 1 collected key {keyNumber}!");
            }
            else if (player2Hitbox.Intersects(key.GetBounds()))
            {
                key.IsCollected = true;
                key.PlayerOwner = 2;
                System.Diagnostics.Debug.WriteLine($"Player 2 collected key {keyNumber}!");
            }
        }

        private void DrawHealthBar(Player player, string label, Color textColor, int yPosition)
        {
            const int HEALTH_BAR_WIDTH = 150;
            const int HEALTH_BAR_HEIGHT = 20;
            int healthBarX = GraphicsDevice.Viewport.Width - HEALTH_BAR_WIDTH - 10;
            int healthBarY = yPosition;

            // Draw background (max health)
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX, healthBarY, HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT),
                Color.DarkRed * 0.7f);

            // Draw current health
            int currentHealthWidth = (int)(HEALTH_BAR_WIDTH * (player.Health / player.MaxHealth));
            Color healthColor = player.Health > 50 ? Color.Green :
                               (player.Health > 25 ? Color.Yellow : Color.Red);
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX, healthBarY, currentHealthWidth, HEALTH_BAR_HEIGHT),
                healthColor);

            // Draw health text
            if (_debugFont != null)
            {
                string healthText = $"{label}: {player.Health:F0}/{player.MaxHealth:F0}";
                Vector2 textSize = _debugFont.MeasureString(healthText);
                _spriteBatch.DrawString(_debugFont, healthText,
                    new Vector2(healthBarX + HEALTH_BAR_WIDTH / 2 - textSize.X / 2, healthBarY + 2),
                    textColor);
            }
        }

        private void DrawPlayerKeyIcon(int playerNumber, int yPosition)
        {
            if ((_key1.IsCollected && _key1.PlayerOwner == playerNumber) ||
                (_key2.IsCollected && _key2.PlayerOwner == playerNumber))
            {
                const int HEALTH_BAR_WIDTH = 150;
                int healthBarX = GraphicsDevice.Viewport.Width - HEALTH_BAR_WIDTH - 10;
                Vector2 keyIconPos = new Vector2(healthBarX - 30, yPosition);
                _key1.DrawIcon(_spriteBatch, _pixelTexture, keyIconPos);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw level and players based on state
            switch (_currentState)
            {
                case GameState.Playing:
                    DrawPlaying();
                    break;

                case GameState.GameOver:
                    DrawGameOver();
                    break;

                case GameState.MainMenu:
                    DrawMainMenu();
                    break;

                case GameState.Paused:
                    DrawPaused();
                    break;

                case GameState.Victory:
                    DrawVictory();
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawPlaying()
        {
            _spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);

            // Draw doors first (behind players)
            _door1.Draw(_spriteBatch, _pixelTexture);
            _door2.Draw(_spriteBatch, _pixelTexture);

            // Draw animated flames
            foreach (var flame in _flames)
            {
                flame.Draw(_spriteBatch, _pixelTexture);
            }

            // Draw animated ice shards
            foreach (var iceShard in _iceShards)
            {
                iceShard.Draw(_spriteBatch, _pixelTexture);
            }

            // Draw keys
            _key1.Draw(_spriteBatch, _pixelTexture);
            _key2.Draw(_spriteBatch, _pixelTexture);

            _player.Draw(_spriteBatch);
            _player2.Draw(_spriteBatch);

            if (_showHitboxes)
            {
                _player.DrawDebug(_spriteBatch, _pixelTexture);
                _player2.DrawDebug(_spriteBatch, _pixelTexture);

                foreach (InteractableObject obj in _platforms)
                {
                    _spriteBatch.Draw(_pixelTexture, obj.Bounds, obj.GetDebugColor());
                }
            }

            // Draw health bars and key icons
            DrawHealthBar(_player, "P1", Color.White, 10);
            DrawPlayerKeyIcon(1, 10);

            DrawHealthBar(_player2, "P2", Color.Cyan, 35);
            DrawPlayerKeyIcon(2, 35);

            // Controls display (always shown)
            if (_debugFont != null)
            {
                _spriteBatch.DrawString(_debugFont, "P1: WASD + Space", new Vector2(10, 10), Color.White);
                _spriteBatch.DrawString(_debugFont, "P2: Arrow Keys", new Vector2(10, 30), Color.Cyan);
            }

            // Surface type legend when hitboxes are shown
            if (_showHitboxes && _debugFont != null)
            {
                int legendY = 70;
                _spriteBatch.DrawString(_debugFont, "Surface Types:", new Vector2(10, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.Cyan * 0.4f);
                _spriteBatch.DrawString(_debugFont, "Solid", new Vector2(30, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.LightBlue * 0.4f);
                _spriteBatch.DrawString(_debugFont, "Ice (Slippery)", new Vector2(30, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.Purple * 0.5f);
                _spriteBatch.DrawString(_debugFont, "Bouncy", new Vector2(30, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.Brown * 0.5f);
                _spriteBatch.DrawString(_debugFont, "Sticky", new Vector2(30, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.Yellow * 0.6f);
                _spriteBatch.DrawString(_debugFont, "Fire (Damage)", new Vector2(30, legendY), Color.White);
                legendY += 20;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, 15, 15), Color.DarkRed * 0.7f);
                _spriteBatch.DrawString(_debugFont, "Spike (High Damage)", new Vector2(30, legendY), Color.White);
            }

            if (_showTimerInfo && _debugFont != null)
            {
                string timerInfo = _collisionTimer.GetDiagnostics();
                _spriteBatch.DrawString(_debugFont, timerInfo, new Vector2(250, 10), Color.White);
                _spriteBatch.DrawString(_debugFont, "H: Hitboxes | T: Timer",
                    new Vector2(10, 230), Color.Yellow);
                _spriteBatch.DrawString(_debugFont,
                    $"Offset: X={_player.HitboxOffsetX} Y={_player.HitboxOffsetY}",
                    new Vector2(10, 250), Color.Cyan);
            }
        }

        private void DrawGameOver()
        {
            try
            {
                // Draw the game world (greyed out)
                _spriteBatch.Draw(_levelTexture,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Color.Gray * 0.5f);

                _player.Draw(_spriteBatch);
                _player2.Draw(_spriteBatch);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing game world in GameOver: {ex.Message}");
            }

            // Draw semi-transparent overlay
            Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_pixelTexture, screenRect, Color.Black * 0.7f);

            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;

            // Draw red modal window
            Rectangle modalWindow = new Rectangle(centerX - 200, centerY - 60, 400, 120);
            _spriteBatch.Draw(_pixelTexture, modalWindow, Color.Red * 0.9f);

            // Draw GAME OVER text if font available
            if (_debugFont != null)
            {
                string gameOverText = "GAME OVER";
                Vector2 textSize = _debugFont.MeasureString(gameOverText);
                float textScale = 3f;

                // Center text within the red modal window
                Vector2 textPosition = new Vector2(
                    modalWindow.X + (modalWindow.Width - textSize.X * textScale) / 2,
                    modalWindow.Y + (modalWindow.Height - textSize.Y * textScale) / 2
                );

                // Draw black text centered in modal window
                _spriteBatch.DrawString(_debugFont, gameOverText,
                    textPosition,
                    Color.Black, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                // Draw restart prompt after delay (below the modal window)
                if (_gameOverTimer >= GAME_OVER_DELAY)
                {
                    string restartText = "Press ENTER or SPACE to Restart";
                    Vector2 restartSize = _debugFont.MeasureString(restartText);
                    Vector2 restartPosition = new Vector2(
                        (GraphicsDevice.Viewport.Width - restartSize.X) / 2,
                        modalWindow.Bottom + 30
                    );

                    _spriteBatch.DrawString(_debugFont, restartText,
                        restartPosition,
                        Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawVictory()
        {
            // Draw the game world in the background
            _spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);

            // Draw doors
            _door1.Draw(_spriteBatch, _pixelTexture);
            _door2.Draw(_spriteBatch, _pixelTexture);

            // Draw players
            _player.Draw(_spriteBatch);
            _player2.Draw(_spriteBatch);

            // Draw semi-transparent overlay
            Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_pixelTexture, screenRect, Color.Black * 0.6f);

            if (_debugFont != null)
            {
                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;

                // Draw "CONGRATULATIONS!" text
                string victoryText = "CONGRATULATIONS!";
                Vector2 textSize = _debugFont.MeasureString(victoryText);
                float textScale = 2.5f;
                Vector2 textPosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - textSize.X * textScale) / 2,
                    (GraphicsDevice.Viewport.Height - textSize.Y * textScale) / 2 - 40
                );

                // Draw shadow
                _spriteBatch.DrawString(_debugFont, victoryText,
                    textPosition + new Vector2(4, 4),
                    Color.Black, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                // Draw main text with gold color
                _spriteBatch.DrawString(_debugFont, victoryText,
                    textPosition,
                    Color.Gold, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

                // Draw "Level Complete!" subtitle
                string subtitleText = "Level Complete!";
                Vector2 subtitleSize = _debugFont.MeasureString(subtitleText);
                float subtitleScale = 1.5f;
                Vector2 subtitlePosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - subtitleSize.X * subtitleScale) / 2,
                    textPosition.Y + textSize.Y * textScale + 20
                );

                _spriteBatch.DrawString(_debugFont, subtitleText,
                    subtitlePosition + new Vector2(2, 2),
                    Color.Black, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0f);

                _spriteBatch.DrawString(_debugFont, subtitleText,
                    subtitlePosition,
                    Color.White, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0f);

                // Timer countdown removed - just display congratulations
            }
            else
            {
                // Fallback: Draw colored victory indicator if no font
                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;
                _spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 150, centerY - 50, 300, 100), Color.Gold * 0.8f);
            }
        }

        private void DrawMainMenu()
        {
            // Draw start page background
            _spriteBatch.Draw(_startPageTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);

            // Menu dimensions and positioning
            int menuWidth = 300;
            int menuHeight = 200;
            int menuX = (GraphicsDevice.Viewport.Width - menuWidth) / 2;
            int menuY = (GraphicsDevice.Viewport.Height - menuHeight) / 2;

            // Draw semi-transparent menu background
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Black * 0.7f);

            // Draw menu border
            int borderThickness = 3;
            // Top border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, menuWidth, borderThickness), Color.Gold);
            // Bottom border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY + menuHeight - borderThickness, menuWidth, borderThickness), Color.Gold);
            // Left border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, borderThickness, menuHeight), Color.Gold);
            // Right border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX + menuWidth - borderThickness, menuY, borderThickness, menuHeight), Color.Gold);

            if (_debugFont != null)
            {
                // Title removed - already present on background image
                // Keeping blank space for layout

                // Menu options
                string[] menuOptions = { "START", "EXIT" };
                int optionSpacing = 50;
                int firstOptionY = menuY + 90;

                for (int i = 0; i < menuOptions.Length; i++)
                {
                    string option = menuOptions[i];
                    Vector2 optionSize = _debugFont.MeasureString(option);
                    float optionScale = 1.5f;
                    Vector2 optionPosition = new Vector2(
                        menuX + (menuWidth - optionSize.X * optionScale) / 2,
                        firstOptionY + i * optionSpacing
                    );

                    // Determine color based on selection
                    Color optionColor = (_selectedMenuOption == i) ? Color.Yellow : Color.White;

                    // Draw selection indicator
                    if (_selectedMenuOption == i)
                    {
                        // Draw highlight background
                        _spriteBatch.Draw(_pixelTexture,
                            new Rectangle((int)optionPosition.X - 10, (int)optionPosition.Y - 5,
                                         (int)(optionSize.X * optionScale) + 20, (int)(optionSize.Y * optionScale) + 10),
                            Color.Orange * 0.3f);

                        // Draw arrow indicator
                        string arrow = ">";
                        _spriteBatch.DrawString(_debugFont, arrow,
                            new Vector2(optionPosition.X - 30, optionPosition.Y),
                            Color.Yellow, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                    }

                    // Draw option shadow
                    _spriteBatch.DrawString(_debugFont, option,
                        optionPosition + new Vector2(2, 2),
                        Color.Black, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);

                    // Draw option text
                    _spriteBatch.DrawString(_debugFont, option,
                        optionPosition,
                        optionColor, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                }

                // Draw controls hint at bottom
                string controlsHint = "Use W/S or Arrow Keys to navigate, Enter/Space to select";
                Vector2 hintSize = _debugFont.MeasureString(controlsHint);
                Vector2 hintPosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - hintSize.X) / 2,
                    GraphicsDevice.Viewport.Height - 30
                );

                _spriteBatch.DrawString(_debugFont, controlsHint,
                    hintPosition,
                    Color.White * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                // Fallback: Draw simple colored blocks for menu options if no font
                int blockHeight = 40;
                int blockWidth = 200;
                int blockX = menuX + (menuWidth - blockWidth) / 2;
                int firstBlockY = menuY + 80;
                int blockSpacing = 60;

                // Start option
                Color startColor = (_selectedMenuOption == 0) ? Color.Green : Color.DarkGreen;
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(blockX, firstBlockY, blockWidth, blockHeight),
                    startColor);

                // Exit option
                Color exitColor = (_selectedMenuOption == 1) ? Color.Red : Color.DarkRed;
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(blockX, firstBlockY + blockSpacing, blockWidth, blockHeight),
                    exitColor);
            }
        }

        private void DrawPaused()
        {
            // Draw the game world in the background (slightly dimmed)
            _spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White * 0.6f);

            // Draw doors (behind players)
            _door1.Draw(_spriteBatch, _pixelTexture);
            _door2.Draw(_spriteBatch, _pixelTexture);

            // Draw animated flames
            foreach (var flame in _flames)
            {
                flame.Draw(_spriteBatch, _pixelTexture);
            }

            // Draw animated ice shards
            foreach (var iceShard in _iceShards)
            {
                iceShard.Draw(_spriteBatch, _pixelTexture);
            }

            // Draw keys
            _key1.Draw(_spriteBatch, _pixelTexture);
            _key2.Draw(_spriteBatch, _pixelTexture);

            // Draw players
            _player.Draw(_spriteBatch);
            _player2.Draw(_spriteBatch);

            // Draw semi-transparent overlay
            Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_pixelTexture, screenRect, Color.Black * 0.5f);

            // Pause menu dimensions and positioning
            int menuWidth = 300;
            int menuHeight = 200;
            int menuX = (GraphicsDevice.Viewport.Width - menuWidth) / 2;
            int menuY = (GraphicsDevice.Viewport.Height - menuHeight) / 2;

            // Draw semi-transparent menu background
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                Color.Black * 0.8f);

            // Draw menu border
            int borderThickness = 3;
            // Top border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, menuWidth, borderThickness), Color.Cyan);
            // Bottom border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY + menuHeight - borderThickness, menuWidth, borderThickness), Color.Cyan);
            // Left border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, borderThickness, menuHeight), Color.Cyan);
            // Right border
            _spriteBatch.Draw(_pixelTexture, new Rectangle(menuX + menuWidth - borderThickness, menuY, borderThickness, menuHeight), Color.Cyan);

            if (_debugFont != null)
            {
                // Draw title
                string title = "PAUSED";
                Vector2 titleSize = _debugFont.MeasureString(title);
                float titleScale = 2.5f;
                Vector2 titlePosition = new Vector2(
                    menuX + (menuWidth - titleSize.X * titleScale) / 2,
                    menuY + 20
                );

                // Draw title shadow
                _spriteBatch.DrawString(_debugFont, title,
                    titlePosition + new Vector2(2, 2),
                    Color.Black, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Draw title
                _spriteBatch.DrawString(_debugFont, title,
                    titlePosition,
                    Color.Cyan, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Pause menu options
                string[] pauseOptions = { "RESUME", "EXIT" };
                int optionSpacing = 50;
                int firstOptionY = menuY + 90;

                for (int i = 0; i < pauseOptions.Length; i++)
                {
                    string option = pauseOptions[i];
                    Vector2 optionSize = _debugFont.MeasureString(option);
                    float optionScale = 1.5f;
                    Vector2 optionPosition = new Vector2(
                        menuX + (menuWidth - optionSize.X * optionScale) / 2,
                        firstOptionY + i * optionSpacing
                    );

                    // Determine color based on selection
                    Color optionColor = (_selectedPauseOption == i) ? Color.Yellow : Color.White;

                    // Draw selection indicator
                    if (_selectedPauseOption == i)
                    {
                        // Draw highlight background
                        _spriteBatch.Draw(_pixelTexture,
                            new Rectangle((int)optionPosition.X - 10, (int)optionPosition.Y - 5,
                                         (int)(optionSize.X * optionScale) + 20, (int)(optionSize.Y * optionScale) + 10),
                            Color.Cyan * 0.3f);

                        // Draw arrow indicator
                        string arrow = ">";
                        _spriteBatch.DrawString(_debugFont, arrow,
                            new Vector2(optionPosition.X - 30, optionPosition.Y),
                            Color.Yellow, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                    }

                    // Draw option shadow
                    _spriteBatch.DrawString(_debugFont, option,
                        optionPosition + new Vector2(2, 2),
                        Color.Black, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);

                    // Draw option text
                    _spriteBatch.DrawString(_debugFont, option,
                        optionPosition,
                        optionColor, 0f, Vector2.Zero, optionScale, SpriteEffects.None, 0f);
                }

                // Draw controls hint at bottom
                string controlsHint = "W/S or Arrows to navigate | Enter/Space to select | ESC to resume";
                Vector2 hintSize = _debugFont.MeasureString(controlsHint);
                Vector2 hintPosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - hintSize.X) / 2,
                    GraphicsDevice.Viewport.Height - 30
                );

                _spriteBatch.DrawString(_debugFont, controlsHint,
                    hintPosition,
                    Color.White * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                // Fallback: Draw simple colored blocks for pause menu options if no font
                int blockHeight = 40;
                int blockWidth = 200;
                int blockX = menuX + (menuWidth - blockWidth) / 2;
                int firstBlockY = menuY + 80;
                int blockSpacing = 60;

                // Resume option
                Color resumeColor = (_selectedPauseOption == 0) ? Color.Green : Color.DarkGreen;
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(blockX, firstBlockY, blockWidth, blockHeight),
                    resumeColor);

                // Exit option
                Color exitColor = (_selectedPauseOption == 1) ? Color.Red : Color.DarkRed;
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(blockX, firstBlockY + blockSpacing, blockWidth, blockHeight),
                    exitColor);
            }
        }
    }
}