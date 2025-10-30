using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace fire_and_ice
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GlobalTimer _collisionTimer;

        private Texture2D _levelTexture;
        private Texture2D _pixelTexture;
        private SpriteFont _debugFont;

        private Player _player;
        private Player _player2; // Second player (blue)
        private List<InteractableObject> _platforms;

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
            }
            catch
            {
                _debugFont = null;
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

            // Player 2 - Blue, spawns in opposite corner (right side) - Arrow keys
            _player2 = new Player(heroTexture, new Vector2(700, 270));
            _player2.PlayerColor = Color.Blue;
            _player2.MoveLeftKey = Keys.Left;
            _player2.MoveRightKey = Keys.Right;
            _player2.JumpKey1 = Keys.Up;
            _player2.JumpKey2 = Keys.RightControl;
            _player2.JumpKey3 = Keys.RightShift;

            System.Diagnostics.Debug.WriteLine($"Loaded {_platforms.Count} platforms");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

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

            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);

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

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}