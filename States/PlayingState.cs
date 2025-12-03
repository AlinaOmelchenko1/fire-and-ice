using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace fire_and_ice.States
{
    /// <summary>
    /// Represents the playing state where the actual gameplay occurs.
    /// Manages all game entities, collision detection, victory/defeat conditions, and gameplay rendering.
    /// </summary>
    public class PlayingState : BaseGameState
    {
        // Resources
        private readonly Texture2D _levelTexture;
        private readonly Texture2D _pixelTexture;
        private readonly SpriteFont _font;

        // Game entities
        private readonly Player _player;
        private readonly Player _player2;
        private readonly List<InteractableObject> _platforms;
        private readonly Key _key1;
        private readonly Key _key2;
        private readonly Door _door1;
        private readonly Door _door2;
        private readonly List<Flame> _flames;
        private readonly List<IceShard> _iceShards;

        // Systems
        private readonly GlobalTimer _collisionTimer;

        // State management
        private readonly Action<GameState> _changeState;
        private bool _doorsOpening;

        // Debug/display flags
        private bool _showHitboxes;
        private bool _showTimerInfo;

        /// <summary>
        /// Creates a new playing state with all required game entities and resources
        /// </summary>
        /// <param name="game">Reference to the main game instance</param>
        /// <param name="spriteBatch">Reference to the sprite batch for rendering</param>
        /// <param name="levelTexture">Background texture for the level</param>
        /// <param name="pixelTexture">1x1 white pixel texture for UI elements</param>
        /// <param name="font">Font for UI text</param>
        /// <param name="player">First player instance</param>
        /// <param name="player2">Second player instance</param>
        /// <param name="platforms">List of platform collision objects</param>
        /// <param name="key1">First key instance</param>
        /// <param name="key2">Second key instance</param>
        /// <param name="door1">First door instance</param>
        /// <param name="door2">Second door instance</param>
        /// <param name="flames">List of flame hazards</param>
        /// <param name="iceShards">List of ice shard hazards</param>
        /// <param name="collisionTimer">Fixed timestep timer for physics</param>
        /// <param name="changeState">Callback to change game state</param>
        public PlayingState(
            Game1 game,
            SpriteBatch spriteBatch,
            Texture2D levelTexture,
            Texture2D pixelTexture,
            SpriteFont font,
            Player player,
            Player player2,
            List<InteractableObject> platforms,
            Key key1,
            Key key2,
            Door door1,
            Door door2,
            List<Flame> flames,
            List<IceShard> iceShards,
            GlobalTimer collisionTimer,
            Action<GameState> changeState) : base(game, spriteBatch)
        {
            _levelTexture = levelTexture;
            _pixelTexture = pixelTexture;
            _font = font;
            _player = player;
            _player2 = player2;
            _platforms = platforms;
            _key1 = key1;
            _key2 = key2;
            _door1 = door1;
            _door2 = door2;
            _flames = flames;
            _iceShards = iceShards;
            _collisionTimer = collisionTimer;
            _changeState = changeState;

            _doorsOpening = false;
            _showHitboxes = false;
            _showTimerInfo = false;
        }

        /// <summary>
        /// Called when entering the playing state.
        /// Resets doors opening flag.
        /// </summary>
        public override void Enter()
        {
            _doorsOpening = false;
            System.Diagnostics.Debug.WriteLine("Entered Playing state");
        }

        /// <summary>
        /// Updates all gameplay logic including entity updates, collision detection, and win/loss conditions
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            // Process player input
            _player.ProcessInput(Keyboard.GetState());
            _player2.ProcessInput(Keyboard.GetState());

            // Update physics with fixed timestep
            _collisionTimer.Update(gameTime, (fixedDeltaTime) =>
            {
                // Update Player 1
                _player.UpdatePhysics(fixedDeltaTime, ScreenWidth);
                _player.CheckCollisions(_platforms);

                // Update Player 2
                _player2.UpdatePhysics(fixedDeltaTime, ScreenWidth);
                _player2.CheckCollisions(_platforms);
            });

            // Update animations
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
            CheckVictoryCondition();

            // Check for game over
            CheckGameOverCondition();
        }

        /// <summary>
        /// Processes keyboard input for gameplay controls (pause, debug toggles)
        /// </summary>
        /// <param name="keyboardState">Current keyboard state</param>
        /// <param name="previousKeyboardState">Previous frame's keyboard state for edge detection</param>
        public override void HandleInput(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            // Handle pause
            if (IsKeyPressed(Keys.Escape, keyboardState, previousKeyboardState))
            {
                _changeState?.Invoke(GameState.Paused);
                System.Diagnostics.Debug.WriteLine("Game paused");
                return;
            }

            // Toggle hitbox display
            if (IsKeyPressed(Keys.H, keyboardState, previousKeyboardState))
            {
                _showHitboxes = !_showHitboxes;
            }

            // Toggle timer info display
            if (IsKeyPressed(Keys.T, keyboardState, previousKeyboardState))
            {
                _showTimerInfo = !_showTimerInfo;
            }
        }

        /// <summary>
        /// Renders all gameplay elements to the screen
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw level background
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, ScreenWidth, ScreenHeight),
                Color.White);

            // Draw doors first (behind players)
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

            // Draw debug info if enabled
            if (_showHitboxes)
            {
                DrawDebugInfo(spriteBatch);
            }

            // Draw UI elements
            DrawUI(spriteBatch);
        }

        /// <summary>
        /// Checks if a player has collected a key and updates the key state
        /// </summary>
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

        /// <summary>
        /// Checks if both players have reached their doors for victory
        /// </summary>
        private void CheckVictoryCondition()
        {
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
                    _changeState?.Invoke(GameState.Victory);
                }
            }
        }

        /// <summary>
        /// Checks if either player has died for game over
        /// </summary>
        private void CheckGameOverCondition()
        {
            if (!_player.IsAlive || !_player2.IsAlive)
            {
                System.Diagnostics.Debug.WriteLine($"=== GAME OVER TRIGGERED ===");
                System.Diagnostics.Debug.WriteLine($"P1 Health: {_player.Health}, P1 Alive: {_player.IsAlive}");
                System.Diagnostics.Debug.WriteLine($"P2 Health: {_player2.Health}, P2 Alive: {_player2.IsAlive}");
                System.Diagnostics.Debug.WriteLine($"Switching to GameOver state");
                _changeState?.Invoke(GameState.GameOver);
            }
        }

        /// <summary>
        /// Draws debug information including hitboxes and surface type legend
        /// </summary>
        private void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            // Draw player hitboxes
            _player.DrawDebug(spriteBatch, _pixelTexture);
            _player2.DrawDebug(spriteBatch, _pixelTexture);

            // Draw platform hitboxes
            foreach (InteractableObject obj in _platforms)
            {
                spriteBatch.Draw(_pixelTexture, obj.Bounds, obj.GetDebugColor());
            }

            // Draw surface type legend
            if (_font != null)
            {
                DrawSurfaceTypeLegend(spriteBatch);
            }
        }

        /// <summary>
        /// Draws the surface type legend for debug mode
        /// </summary>
        private void DrawSurfaceTypeLegend(SpriteBatch spriteBatch)
        {
            int legendY = GameConstants.UI.LegendStartY;
            spriteBatch.DrawString(_font, "Surface Types:", new Vector2(10, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Solid
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.Cyan * 0.4f);
            spriteBatch.DrawString(_font, "Solid", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Ice
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.LightBlue * 0.4f);
            spriteBatch.DrawString(_font, "Ice (Slippery)", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Bouncy
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.Purple * 0.5f);
            spriteBatch.DrawString(_font, "Bouncy", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Sticky
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.Brown * 0.5f);
            spriteBatch.DrawString(_font, "Sticky", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Fire
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.Yellow * 0.6f);
            spriteBatch.DrawString(_font, "Fire (Damage)", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
            legendY += GameConstants.UI.LegendSpacing;

            // Spike
            spriteBatch.Draw(_pixelTexture, new Rectangle(10, legendY, GameConstants.UI.LegendIconSize, GameConstants.UI.LegendIconSize), Color.DarkRed * 0.7f);
            spriteBatch.DrawString(_font, "Spike (High Damage)", new Vector2(GameConstants.UI.LegendTextOffset, legendY), Color.White);
        }

        /// <summary>
        /// Draws all UI elements including health bars, key icons, and controls
        /// </summary>
        private void DrawUI(SpriteBatch spriteBatch)
        {
            // Draw health bars and key icons
            DrawHealthBar(spriteBatch, _player, "P1", Color.White, GameConstants.UI.Player1HealthY);
            DrawPlayerKeyIcon(spriteBatch, 1, GameConstants.UI.Player1HealthY);

            DrawHealthBar(spriteBatch, _player2, "P2", Color.Cyan, GameConstants.UI.Player2HealthY);
            DrawPlayerKeyIcon(spriteBatch, 2, GameConstants.UI.Player2HealthY);

            // Controls display (always shown)
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "P1: WASD + Space", new Vector2(10, GameConstants.UI.ControlsY), Color.White);
                spriteBatch.DrawString(_font, "P2: Arrow Keys", new Vector2(10, GameConstants.UI.Player2ControlsY), Color.Cyan);
            }

            // Timer info if enabled
            if (_showTimerInfo && _font != null)
            {
                DrawTimerInfo(spriteBatch);
            }
        }

        /// <summary>
        /// Draws a health bar for a player
        /// </summary>
        private void DrawHealthBar(SpriteBatch spriteBatch, Player player, string label, Color textColor, int yPosition)
        {
            int healthBarX = ScreenWidth - GameConstants.UI.HealthBarWidth - GameConstants.UI.HealthBarMargin;
            int healthBarY = yPosition;

            // Draw background (max health)
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(healthBarX, healthBarY, GameConstants.UI.HealthBarWidth, GameConstants.UI.HealthBarHeight),
                Color.DarkRed * GameConstants.Opacity.High);

            // Draw current health
            int currentHealthWidth = (int)(GameConstants.UI.HealthBarWidth * (player.Health / player.MaxHealth));
            Color healthColor = player.Health > 50 ? Color.Green :
                               (player.Health > 25 ? Color.Yellow : Color.Red);
            DrawFilledRectangle(spriteBatch, _pixelTexture,
                new Rectangle(healthBarX, healthBarY, currentHealthWidth, GameConstants.UI.HealthBarHeight),
                healthColor);

            // Draw health text
            if (_font != null)
            {
                string healthText = $"{label}: {player.Health:F0}/{player.MaxHealth:F0}";
                Vector2 textSize = _font.MeasureString(healthText);
                spriteBatch.DrawString(_font, healthText,
                    new Vector2(healthBarX + GameConstants.UI.HealthBarWidth / 2 - textSize.X / 2, healthBarY + 2),
                    textColor);
            }
        }

        /// <summary>
        /// Draws a key icon next to the player's health bar if they have collected a key
        /// </summary>
        private void DrawPlayerKeyIcon(SpriteBatch spriteBatch, int playerNumber, int yPosition)
        {
            if ((_key1.IsCollected && _key1.PlayerOwner == playerNumber) ||
                (_key2.IsCollected && _key2.PlayerOwner == playerNumber))
            {
                int healthBarX = ScreenWidth - GameConstants.UI.HealthBarWidth - GameConstants.UI.HealthBarMargin;
                Vector2 keyIconPos = new Vector2(healthBarX - GameConstants.UI.KeyIconOffset, yPosition);
                _key1.DrawIcon(spriteBatch, _pixelTexture, keyIconPos);
            }
        }

        /// <summary>
        /// Draws timer debug information
        /// </summary>
        private void DrawTimerInfo(SpriteBatch spriteBatch)
        {
            string timerInfo = $"Timer: {_collisionTimer.TotalElapsedTime:F2}s | Ticks: {_collisionTimer.FixedUpdateCount}";
            spriteBatch.DrawString(_font, timerInfo,
                new Vector2(GameConstants.UI.TimerInfoX, GameConstants.UI.TimerInfoY),
                Color.Yellow);

            string debugInfo = $"P1 Pos: ({_player.Position.X:F0}, {_player.Position.Y:F0}) Ground: {_player.IsOnGround}";
            spriteBatch.DrawString(_font, debugInfo,
                new Vector2(10, GameConstants.UI.DebugInfoY),
                Color.White);

            string debugInfo2 = $"P2 Pos: ({_player2.Position.X:F0}, {_player2.Position.Y:F0}) Ground: {_player2.IsOnGround}";
            spriteBatch.DrawString(_font, debugInfo2,
                new Vector2(10, GameConstants.UI.DebugInfo2Y),
                Color.Cyan);
        }
    }
}
