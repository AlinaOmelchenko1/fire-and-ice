using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using fire_and_ice.States;

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

        // State Management
        private StateManager _stateManager;

        // Resources
        private Texture2D _levelTexture;
        private Texture2D _startPageTexture;
        private Texture2D _pixelTexture;
        private SpriteFont _debugFont;

        // Game Entities
        private Player _player;
        private Player _player2; // Second player (blue)
        private List<InteractableObject> _platforms;

        // Keys and Doors
        private Key _key1; // For player 1
        private Key _key2; // For player 2
        private Door _door1; // Left door
        private Door _door2; // Right door

        // Flames and Ice Shards
        private List<Flame> _flames;
        private List<IceShard> _iceShards;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
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

            // Load platforms using manual platform definition
            // NOTE: Collision map doesn't include fire/ice hazards, so we use manual platforms
            _platforms = LevelPlatforms.GetLevel1Platforms();
            System.Diagnostics.Debug.WriteLine($"Loaded {_platforms.Count} platforms from manual platform list");

            // Player 1 - Original (white/default color) - WASD + Space controls
            _player = new Player(heroTexture, GameConstants.SpawnPositions.Player1Start);
            _player.Type = PlayerType.Fire; // Fire character
            _player.PlayerColor = Color.White;
            _player.MoveLeftKey = Keys.A;
            _player.MoveRightKey = Keys.D;
            _player.JumpKey1 = Keys.W;
            _player.JumpKey2 = Keys.Space;
            _player.JumpKey3 = Keys.None; // Not used
            System.Diagnostics.Debug.WriteLine($"Player 1 created at spawn: {GameConstants.SpawnPositions.Player1Start}");

            // Player 2 - Ice character, spawns in opposite corner (right side) - Arrow keys
            _player2 = new Player(character2Texture, GameConstants.SpawnPositions.Player2Start); // 4 frames (default)
            _player2.Type = PlayerType.Ice; // Ice character
            _player2.PlayerColor = Color.White; // No color tinting needed
            _player2.MoveLeftKey = Keys.Left;
            _player2.MoveRightKey = Keys.Right;
            _player2.JumpKey1 = Keys.Up;
            _player2.JumpKey2 = Keys.RightControl;
            _player2.JumpKey3 = Keys.RightShift;
            System.Diagnostics.Debug.WriteLine($"Player 2 created at spawn: {GameConstants.SpawnPositions.Player2Start}");

            // Initialize keys - spawn at specific locations
            _key1 = new Key(GameConstants.SpawnPositions.Key1Position); // Left upper corner of left wooden crate
            _key2 = new Key(GameConstants.SpawnPositions.Key2Position); // On right wooden crate

            // Set pixel texture for keys after _pixelTexture is initialized
            _key1.SetPixelTexture(_pixelTexture);
            _key2.SetPixelTexture(_pixelTexture);

            // Initialize doors at top of map (matching green rectangles)
            _door1 = new Door(GameConstants.SpawnPositions.Door1Position); // Top left corner door (moved up 30px, left 5px)
            _door2 = new Door(GameConstants.SpawnPositions.Door2Position); // Top right corner door (moved up 30px, left 5px)

            // Set pixel texture for doors after _pixelTexture is initialized
            _door1.SetPixelTexture(_pixelTexture);
            _door2.SetPixelTexture(_pixelTexture);

            // Initialize flames for all fire hazards
            _flames = new List<Flame>();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.Fire)
                {
                    var flame = new Flame(platform.Bounds);
                    flame.SetPixelTexture(_pixelTexture);
                    _flames.Add(flame);
                }
            }

            // Initialize ice shards for all ice hazards
            _iceShards = new List<IceShard>();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.IceHazard)
                {
                    var iceShard = new IceShard(platform.Bounds);
                    iceShard.SetPixelTexture(_pixelTexture);
                    _iceShards.Add(iceShard);
                }
            }

            System.Diagnostics.Debug.WriteLine($"=== PLATFORM LOADING COMPLETE ===");
            System.Diagnostics.Debug.WriteLine($"Total platforms: {_platforms.Count}");
            System.Diagnostics.Debug.WriteLine($"Fire platforms: {_platforms.FindAll(p => p.Type == SurfaceType.Fire).Count}");
            System.Diagnostics.Debug.WriteLine($"Ice hazard platforms: {_platforms.FindAll(p => p.Type == SurfaceType.IceHazard).Count}");
            System.Diagnostics.Debug.WriteLine($"Created {_flames.Count} animated flames");
            System.Diagnostics.Debug.WriteLine($"Created {_iceShards.Count} animated ice shards");

            // Initialize collision timer
            _collisionTimer = new GlobalTimer();

            // Initialize State Manager and register all states
            InitializeStateManager();
        }

        protected override void Update(GameTime gameTime)
        {
            // Delegate input handling and update to StateManager
            _stateManager.HandleInput();
            _stateManager.Update(gameTime);

            base.Update(gameTime);
        }

        private void RestartGame()
        {
            System.Diagnostics.Debug.WriteLine("=== RESTARTING GAME ===");

            // Reset players completely
            _player.Reset(GameConstants.SpawnPositions.Player1Start);
            _player2.Reset(GameConstants.SpawnPositions.Player2Start);

            System.Diagnostics.Debug.WriteLine($"Players reset - P1: {_player.Health}HP, P2: {_player2.Health}HP");

            // Reset keys and doors
            _key1 = new Key(GameConstants.SpawnPositions.Key1Position); // Left upper corner of left wooden crate
            _key2 = new Key(GameConstants.SpawnPositions.Key2Position); // On right wooden crate
            _key1.SetPixelTexture(_pixelTexture);
            _key2.SetPixelTexture(_pixelTexture);
            _door1.Reset();
            _door2.Reset();

            // Reset flames
            _flames.Clear();
            foreach (var platform in _platforms)
            {
                if (platform.Type == SurfaceType.Fire)
                {
                    var flame = new Flame(platform.Bounds);
                    flame.SetPixelTexture(_pixelTexture);
                    _flames.Add(flame);
                }
            }

            System.Diagnostics.Debug.WriteLine("Game reset complete - ready for Playing state");
        }

        private void InitializeStateManager()
        {
            System.Diagnostics.Debug.WriteLine("=== INITIALIZING STATE MANAGER ===");

            _stateManager = new StateManager();

            // Create callback for state changes
            Action<GameState> changeState = (newState) => _stateManager.ChangeState(newState);

            // Create callback for exiting game
            Action exitGame = () => Exit();

            // Register MainMenu state
            var mainMenuState = new MainMenuState(
                this,
                _spriteBatch,
                _startPageTexture,
                _pixelTexture,
                _debugFont,
                changeState,
                exitGame);
            _stateManager.RegisterState(GameState.MainMenu, mainMenuState);

            // Register Playing state
            var playingState = new PlayingState(
                this,
                _spriteBatch,
                _levelTexture,
                _pixelTexture,
                _debugFont,
                _player,
                _player2,
                _platforms,
                _key1,
                _key2,
                _door1,
                _door2,
                _flames,
                _iceShards,
                _collisionTimer,
                changeState);
            _stateManager.RegisterState(GameState.Playing, playingState);

            // Register GameOver state
            var gameOverState = new GameOverState(
                this,
                _spriteBatch,
                _levelTexture,
                _pixelTexture,
                _debugFont,
                _player,
                _player2,
                RestartGame);
            _stateManager.RegisterState(GameState.GameOver, gameOverState);

            // Register Paused state
            var pausedState = new PausedState(
                this,
                _spriteBatch,
                _levelTexture,
                _pixelTexture,
                _debugFont,
                _player,
                _player2,
                _door1,
                _door2,
                _key1,
                _key2,
                _flames,
                _iceShards,
                changeState,
                exitGame);
            _stateManager.RegisterState(GameState.Paused, pausedState);

            // Register Victory state
            var victoryState = new VictoryState(
                this,
                _spriteBatch,
                _levelTexture,
                _pixelTexture,
                _debugFont,
                _player,
                _player2,
                _door1,
                _door2,
                changeState,
                RestartGame);
            _stateManager.RegisterState(GameState.Victory, victoryState);

            // Set initial state and enter it
            _stateManager.SetInitialState(GameState.MainMenu);
            _stateManager.ChangeState(GameState.MainMenu);

            System.Diagnostics.Debug.WriteLine("=== STATE MANAGER INITIALIZED ===");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Delegate drawing to StateManager
            _stateManager.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}