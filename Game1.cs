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
        private List<Rectangle> _platforms;

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

            _player = new Player(heroTexture, new Vector2(85, 270));

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

            _collisionTimer.Update(gameTime, (fixedDeltaTime) =>
            {
                // Detect ground and collisions before physics step
                _player.CheckCollisions(_platforms);

                // Then apply movement, gravity, and jump logic
                _player.UpdatePhysics(fixedDeltaTime, GraphicsDevice.Viewport.Width);
            });


            _player.UpdateAnimation(gameTime);

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

            if (_showHitboxes)
            {
                _player.DrawDebug(_spriteBatch, _pixelTexture);

                foreach (Rectangle platform in _platforms)
                {
                    _spriteBatch.Draw(_pixelTexture, platform, Color.Cyan * 0.4f);
                }
            }

            if (_showTimerInfo && _debugFont != null)
            {
                string timerInfo = _collisionTimer.GetDiagnostics();
                _spriteBatch.DrawString(_debugFont, timerInfo, new Vector2(10, 10), Color.White);
                _spriteBatch.DrawString(_debugFont, "H: Hitboxes | T: Timer",
                    new Vector2(10, 30), Color.Yellow);
                _spriteBatch.DrawString(_debugFont,
                    $"Offset: X={_player.HitboxOffsetX} Y={_player.HitboxOffsetY}",
                    new Vector2(10, 50), Color.Cyan);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}