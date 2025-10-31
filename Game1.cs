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
        MainMenu,    // For future start screen
        Playing,
        GameOver,
        Paused       // For future pause functionality
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GlobalTimer _collisionTimer;

        // Game State
        private GameState _currentState = GameState.Playing;
        private float _gameOverTimer = 0f;
        private const float GAME_OVER_DELAY = 2f; // Show game over for 2 seconds before allowing restart

        private Texture2D _levelTexture;
        private Texture2D _pixelTexture;
        private SpriteFont _debugFont;

        private Player _player;
        private Player _player2; // Second player (blue)
        private List<InteractableObject> _platforms;

        // Keys and Doors
        private Key _key1; // For player 1
        private Key _key2; // For player 2
        private Door _door1; // Left door
        private Door _door2; // Right door
        private bool _doorsOpening = false;

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
            Texture2D heroTexture = Content.Load<Texture2D>("hero_walk");

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
            _player.PlayerColor = Color.White;
            _player.MoveLeftKey = Keys.A;
            _player.MoveRightKey = Keys.D;
            _player.JumpKey1 = Keys.W;
            _player.JumpKey2 = Keys.Space;
            _player.JumpKey3 = Keys.None; // Not used

            // Player 2 - Light Blue, spawns in opposite corner (right side) - Arrow keys
            _player2 = new Player(heroTexture, new Vector2(700, 270));
            _player2.PlayerColor = Color.Cyan;
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

            System.Diagnostics.Debug.WriteLine($"Loaded {_platforms.Count} platforms");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Only allow exit in playing state, not in game over
            if (keyboardState.IsKeyDown(Keys.Escape) && _currentState == GameState.Playing)
                Exit();

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
                    // Future: Implement main menu
                    break;

                case GameState.Paused:
                    // Future: Implement pause
                    break;
            }

            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void UpdatePlaying(GameTime gameTime, KeyboardState keyboardState)
        {
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
            if (!_key1.IsCollected)
            {
                Rectangle playerHitbox = _player.GetHitbox();
                Rectangle player2Hitbox = _player2.GetHitbox();

                if (playerHitbox.Intersects(_key1.GetBounds()))
                {
                    _key1.IsCollected = true;
                    _key1.PlayerOwner = 1;
                    System.Diagnostics.Debug.WriteLine("Player 1 collected key!");
                }
                else if (player2Hitbox.Intersects(_key1.GetBounds()))
                {
                    _key1.IsCollected = true;
                    _key1.PlayerOwner = 2;
                    System.Diagnostics.Debug.WriteLine("Player 2 collected key 1!");
                }
            }

            if (!_key2.IsCollected)
            {
                Rectangle playerHitbox = _player.GetHitbox();
                Rectangle player2Hitbox = _player2.GetHitbox();

                if (playerHitbox.Intersects(_key2.GetBounds()))
                {
                    _key2.IsCollected = true;
                    _key2.PlayerOwner = 1;
                    System.Diagnostics.Debug.WriteLine("Player 1 collected key 2!");
                }
                else if (player2Hitbox.Intersects(_key2.GetBounds()))
                {
                    _key2.IsCollected = true;
                    _key2.PlayerOwner = 2;
                    System.Diagnostics.Debug.WriteLine("Player 2 collected key!");
                }
            }

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

            // Reset game state
            _currentState = GameState.Playing;
            _gameOverTimer = 0f;

            System.Diagnostics.Debug.WriteLine("Game state set to Playing");
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
                    // Future: Draw main menu
                    break;

                case GameState.Paused:
                    // Future: Draw pause screen
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

            // Health bar - Player 1 (White)
            int healthBarWidth = 150;
            int healthBarHeight = 20;
            int healthBarX1 = GraphicsDevice.Viewport.Width - healthBarWidth - 10;
            int healthBarY1 = 10;

            // Background (max health)
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX1, healthBarY1, healthBarWidth, healthBarHeight),
                Color.DarkRed * 0.7f);

            // Current health
            int currentHealthWidth1 = (int)(healthBarWidth * (_player.Health / _player.MaxHealth));
            Color healthColor1 = _player.Health > 50 ? Color.Green : (_player.Health > 25 ? Color.Yellow : Color.Red);
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX1, healthBarY1, currentHealthWidth1, healthBarHeight),
                healthColor1);

            // Health text
            if (_debugFont != null)
            {
                string healthText = $"P1: {_player.Health:F0}/{_player.MaxHealth:F0}";
                Vector2 textSize = _debugFont.MeasureString(healthText);
                _spriteBatch.DrawString(_debugFont, healthText,
                    new Vector2(healthBarX1 + healthBarWidth/2 - textSize.X/2, healthBarY1 + 2),
                    Color.White);
            }

            // Draw key icon for Player 1 if they have collected a key
            if ((_key1.IsCollected && _key1.PlayerOwner == 1) || (_key2.IsCollected && _key2.PlayerOwner == 1))
            {
                Vector2 keyIconPos = new Vector2(healthBarX1 - 30, healthBarY1);
                _key1.DrawIcon(_spriteBatch, _pixelTexture, keyIconPos);
            }

            // Health bar - Player 2 (Blue)
            int healthBarX2 = GraphicsDevice.Viewport.Width - healthBarWidth - 10;
            int healthBarY2 = 35;

            // Background (max health)
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX2, healthBarY2, healthBarWidth, healthBarHeight),
                Color.DarkRed * 0.7f);

            // Current health
            int currentHealthWidth2 = (int)(healthBarWidth * (_player2.Health / _player2.MaxHealth));
            Color healthColor2 = _player2.Health > 50 ? Color.Green : (_player2.Health > 25 ? Color.Yellow : Color.Red);
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle(healthBarX2, healthBarY2, currentHealthWidth2, healthBarHeight),
                healthColor2);

            // Health text
            if (_debugFont != null)
            {
                string healthText2 = $"P2: {_player2.Health:F0}/{_player2.MaxHealth:F0}";
                Vector2 textSize2 = _debugFont.MeasureString(healthText2);
                _spriteBatch.DrawString(_debugFont, healthText2,
                    new Vector2(healthBarX2 + healthBarWidth/2 - textSize2.X/2, healthBarY2 + 2),
                    Color.Cyan);
            }

            // Draw key icon for Player 2 if they have collected a key
            if ((_key1.IsCollected && _key1.PlayerOwner == 2) || (_key2.IsCollected && _key2.PlayerOwner == 2))
            {
                Vector2 keyIconPos = new Vector2(healthBarX2 - 30, healthBarY2);
                _key2.DrawIcon(_spriteBatch, _pixelTexture, keyIconPos);
            }

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

            // Draw big colored blocks to spell GAME OVER (no font needed!)
            int blockSize = 40;
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;

            // Draw "GAME OVER" as colored blocks
            _spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 200, centerY - 60, 380, 120), Color.Red * 0.9f);

            // Draw GAME OVER text if font available
            if (_debugFont != null)
            {
                string gameOverText = "GAME OVER";
                Vector2 textSize = _debugFont.MeasureString(gameOverText);
                Vector2 textPosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - textSize.X * 3) / 2,
                    (GraphicsDevice.Viewport.Height - textSize.Y * 3) / 2
                );

                // Draw shadow
                _spriteBatch.DrawString(_debugFont, gameOverText,
                    textPosition + new Vector2(4, 4) * 3,
                    Color.Black, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

                // Draw main text
                _spriteBatch.DrawString(_debugFont, gameOverText,
                    textPosition,
                    Color.Red, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

                // Draw restart prompt after delay
                if (_gameOverTimer >= GAME_OVER_DELAY)
                {
                    string restartText = "Press ENTER or SPACE to Restart";
                    Vector2 restartSize = _debugFont.MeasureString(restartText);
                    Vector2 restartPosition = new Vector2(
                        (GraphicsDevice.Viewport.Width - restartSize.X) / 2,
                        textPosition.Y + textSize.Y * 3 + 40
                    );

                    _spriteBatch.DrawString(_debugFont, restartText,
                        restartPosition,
                        Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }

                // Show which player died
                // Which player died indicator
                if (!_player.IsAlive && !_player2.IsAlive)
                {
                    string deathMessage = "Both players died!";
                    Vector2 deathSize = _debugFont.MeasureString(deathMessage);
                    _spriteBatch.DrawString(_debugFont, deathMessage,
                        new Vector2((GraphicsDevice.Viewport.Width - deathSize.X) / 2, textPosition.Y - 40),
                        Color.Yellow);
                }
                else if (!_player.IsAlive)
                {
                    _spriteBatch.DrawString(_debugFont, "Player 1 died!",
                        new Vector2(GraphicsDevice.Viewport.Width / 2 - 60, textPosition.Y - 40),
                        Color.White);
                }
                else if (!_player2.IsAlive)
                {
                    _spriteBatch.DrawString(_debugFont, "Player 2 died!",
                        new Vector2(GraphicsDevice.Viewport.Width / 2 - 60, textPosition.Y - 40),
                        Color.Cyan);
                }
            }
            else
            {
                // No font available - just show colored indicators
                // Draw color-coded death indicator
                int indicatorY = centerY - 100;
                if (!_player.IsAlive)
                {
                    _spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 100, indicatorY, 80, 30), Color.White);
                }
                if (!_player2.IsAlive)
                {
                    _spriteBatch.Draw(_pixelTexture, new Rectangle(centerX + 20, indicatorY, 80, 30), Color.Cyan);
                }
            }
        }
    }
}