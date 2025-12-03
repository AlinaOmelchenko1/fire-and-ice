# Fire and Ice - Refactoring Plan

## Architecture Analysis

### Current Issues

1. **God Object (Game1.cs - 1049 lines)** - Violates Single Responsibility Principle
2. **Tight Coupling** - Direct dependencies between components
3. **Missing Abstractions** - No interfaces for game objects
4. **No State Pattern** - State logic scattered in switch statements
5. **Mixed Concerns** - Rendering and logic intertwined
6. **Magic Numbers** - Hard-coded values throughout
7. **Duplicate Code** - Similar patterns in Flame and IceShard
8. **No Layered Architecture** - Everything in one layer
9. **Unused Legacy Code** - CollisionProtagonist, PlatformEditor, etc.

### Recommended Architecture

```
┌─────────────────────────────────────────────┐
│           Presentation Layer                │
│  (UI, Rendering, Input Handling)            │
├─────────────────────────────────────────────┤
│           Game Logic Layer                  │
│  (Game States, Entities, Systems)           │
├─────────────────────────────────────────────┤
│           Core/Domain Layer                 │
│  (Interfaces, Base Classes, Utilities)      │
├─────────────────────────────────────────────┤
│        Infrastructure Layer                 │
│  (Content Loading, Physics, Collision)      │
└─────────────────────────────────────────────┘
```

---

## Refactoring Steps

### Phase 1: Clean Up and Remove Unused Code

#### Step 1.1: Identify Unused Files ✅ COMPLETED
- [x] Verify if `CollisionProtagonist.cs` is used - **UNUSED** (Early/incomplete player implementation)
- [x] Verify if `PlatformEditor.cs` is used - **UNUSED** (Development tool for platform editing)
- [x] Verify if `PlatformCaveBackground.cs` is used - **UNUSED** (Alternative cave background implementation)
- [x] Verify if `PlatformProtagonist.cs` is used - **UNUSED** (Alternative player implementation)
- [x] Verify if `ImageLevelBackground.cs` is used - **UNUSED** (Alternative background loader)

#### Step 1.2: Remove Unused Files ✅ COMPLETED
- [x] Delete or move unused files to a `_Legacy` folder - **DONE** (Created _Legacy folder and moved all 5 files)
- [x] Update namespace references if needed - **N/A** (No references existed)
- [x] Build and test to ensure nothing breaks - **PASSED** (0 errors, 0 warnings)
- [x] Exclude `_Legacy` folder from build in .csproj - **DONE**

#### Step 1.3: Remove Magic Numbers ✅ COMPLETED
- [x] Create `Constants.cs` file - **DONE** (Comprehensive constants file with organized sections)
- [x] Extract screen dimensions constants - **DONE**
- [x] Extract physics constants (gravity, jump power, etc.) - **DONE**
- [x] Extract color constants - **DONE** (Opacity values organized)
- [x] Extract timing constants (animation speeds, delays) - **DONE**
- [x] Replace hard-coded values with named constants - **MOSTLY DONE**
  - [x] Key.cs - Fully refactored
  - [x] Door.cs - Fully refactored
  - [x] Game1.cs - Spawn positions, game state timers, menu counts, health bars, UI positions
  - [x] Flame.cs - Fully refactored with GameConstants.Flame
  - [x] IceShard.cs - Fully refactored with GameConstants.IceShard
  - [ ] Player.cs - Physics constants still using local values (can be refactored in Step 3.5)

---

### Phase 2: Create Core Abstractions

#### Step 2.1: Create Base Interfaces ✅ COMPLETED
- [x] Create `Interfaces/IGameObject.cs` interface - **DONE**
  - Created comprehensive IGameObject interface with Update and Draw methods
  - Added XML documentation for interface and methods
  - Build passed with 0 errors, 0 warnings

#### Step 2.2: Create Rendering Interface ✅ COMPLETED
- [x] Create `Interfaces/IRenderable.cs` interface - **DONE**
  - Created IRenderable interface with Draw method and DrawOrder property
  - Added comprehensive XML documentation explaining draw order ranges
  - Provides guidelines for layering (background: 0-100, platforms: 100-200, players: 300-400, UI: 500+)
  - Build passed with 0 errors, 0 warnings

#### Step 2.3: Create Update Interface ✅ COMPLETED
- [x] Create `Interfaces/IUpdateable.cs` interface - **DONE**
  - Created IUpdateable interface with Update method and Enabled property
  - Added comprehensive XML documentation
  - Enabled property allows objects to be temporarily disabled without removal from collections
  - Build passed with 0 errors, 0 warnings

#### Step 2.4: Create Collision Interface ✅ COMPLETED
- [x] Create `Interfaces/ICollidable.cs` interface - **DONE**
  - Created ICollidable interface with GetBounds and CheckCollision methods
  - Added comprehensive XML documentation
  - GetBounds() returns the collision Rectangle for the object
  - CheckCollision() allows object-to-object collision testing
  - Build passed with 0 errors, 0 warnings

#### Step 2.5: Create Animated Object Base Class ✅ COMPLETED
- [x] Create `Core/AnimatedObject.cs` base class - **DONE**
- [x] Move common animation logic from Flame and IceShard - **DONE**
  - Provides Bounds property for all animated objects
  - Includes AnimationTimer and SecondaryTimer for multi-phase animations
  - CurrentIntensity property for brightness/alpha effects
  - Shared Random instance with randomized initial phase
  - Implements IGameObject interface
- [x] Implement common Update pattern for animations - **DONE**
  - Abstract UpdateAnimation() method for derived classes
  - Helper methods: CalculateSineWave(), CalculateIntensity(), Clamp()
  - Comprehensive XML documentation throughout
- [x] Build passed with 0 errors, 0 warnings

---

### Phase 3: Refactor Game Objects

#### Step 3.1: Refactor Key Class ✅ COMPLETED
- [x] Implement `IGameObject` interface - **DONE**
  - Implemented Update(GameTime) and Draw(SpriteBatch) methods
  - Draw method now uses internal _pixelTexture field
  - Added SetPixelTexture() method for initialization
- [x] Implement `ICollidable` interface - **DONE**
  - GetBounds() already existed, kept implementation
  - Added CheckCollision(ICollidable) method with proper collision logic
  - Collision returns false when key is already collected
- [x] Extract drawing constants to class-level - **ALREADY DONE** (using GameConstants)
  - All magic numbers replaced with GameConstants.Key values
  - Opacity value uses GameConstants.Opacity.VeryLight
- [x] Add XML documentation comments - **DONE**
  - Comprehensive documentation for class, properties, and all methods
  - Clear descriptions of parameters and return values
- [x] Updated Game1.cs to work with refactored Key - **DONE**
  - Keys now use SetPixelTexture() for initialization
  - Draw calls updated to new signature without pixelTexture parameter
- [x] Build passed with 0 errors, 0 warnings

#### Step 3.2: Refactor Door Class ✅ COMPLETED
- [x] Implement `IGameObject` interface - **DONE**
  - Implemented Update(GameTime) and Draw(SpriteBatch) methods
  - Draw method now uses internal _pixelTexture field
  - Added SetPixelTexture() method for initialization
- [x] Implement `ICollidable` interface - **DONE**
  - GetBounds() already existed, kept implementation
  - Added CheckCollision(ICollidable) method with proper collision logic
  - Collision returns false when door is fully open
- [x] Extract animation constants - **ALREADY DONE** (using GameConstants)
  - All magic numbers replaced with GameConstants.Door values
  - Opacity values use GameConstants.Opacity constants (VeryHigh, Medium)
- [x] Add comprehensive XML documentation - **DONE**
  - Documented class, properties, and all methods
  - Clear descriptions of parameters, return values, and behavior
  - Explained door opening animation and bar retraction
- [x] Updated Game1.cs to work with refactored Door - **DONE**
  - Doors now use SetPixelTexture() for initialization in LoadContent
  - Draw calls updated to new signature without pixelTexture parameter
  - Doors maintain texture through Reset() calls (not recreated)
- [x] Build passed with 0 errors, 0 warnings
- [ ] Add state enum for door states - **DEFERRED** (Current implementation works well with boolean and progress)

#### Step 3.3: Refactor Flame Class ✅ COMPLETED
- [x] Inherit from `AnimatedObject` base class - **DONE**
  - Flame now extends AnimatedObject
  - Uses base class timers (AnimationTimer, SecondaryTimer)
  - Uses base class CurrentIntensity and Random
- [x] Implement `IGameObject` interface - **DONE**
  - IGameObject inherited through AnimatedObject base class
  - Implements Update(GameTime) and Draw(SpriteBatch)
  - Added SetPixelTexture() method for initialization
- [x] Extract magic numbers to constants - **DONE**
  - All magic numbers replaced with GameConstants.Flame values
  - Layer sizes use constants (OuterLayerWidth, MiddleLayerWidth, etc.)
  - Opacity values use GameConstants.Opacity constants
  - Animation parameters use GameConstants.Flame constants
- [x] Updated Game1.cs to work with refactored Flame - **DONE**
  - Flames now use SetPixelTexture() for initialization
  - Draw calls updated to new signature without pixelTexture parameter
- [x] Build passed with 0 errors, 0 warnings

#### Step 3.4: Refactor IceShard Class ✅ COMPLETED
- [x] Inherit from `AnimatedObject` base class - **DONE**
  - IceShard now extends AnimatedObject
  - Uses base class timers (AnimationTimer, SecondaryTimer)
  - Uses base class CurrentIntensity and Random
- [x] Implement `IGameObject` interface - **DONE**
  - IGameObject inherited through AnimatedObject base class
  - Implements Update(GameTime) and Draw(SpriteBatch)
  - Added SetPixelTexture() method for initialization
- [x] Extract magic numbers to constants - **DONE**
  - All magic numbers replaced with GameConstants.IceShard values
  - Layer sizes use constants (OuterLayerWidth, MiddleLayerWidth, etc.)
  - Opacity values use GameConstants.Opacity constants
  - Animation parameters use GameConstants.IceShard constants
  - Sparkle and shimmer effects use constants
- [x] Updated Game1.cs to work with refactored IceShard - **DONE**
  - Ice shards now use SetPixelTexture() for initialization
  - Draw calls updated to new signature without pixelTexture parameter
- [x] Build passed with 0 errors, 0 warnings

#### Step 3.5: Refactor Player Class ✅ COMPLETED
- [x] Implement `IGameObject` and `ICollidable` interfaces - **DONE**
  - Added using statement for fire_and_ice.Interfaces
  - Implemented IGameObject with Update(GameTime) and Draw(SpriteBatch)
  - Implemented ICollidable with GetBounds() and CheckCollision(ICollidable)
  - Added comprehensive XML documentation for class and all public methods
- [x] Replace magic numbers with GameConstants - **DONE**
  - All physics constants now use GameConstants.Physics (Gravity, JumpPower, MaxRunSpeed, Acceleration, Deceleration, MaxFallSpeed, CoyoteTime)
  - All player constants now use GameConstants.Player (DefaultHealth, DamageCooldownTime, HitboxOffsets, AnimationInterval, InvincibilityFlashRate)
  - All surface constants now use GameConstants.Surface (IceFriction, StickyFriction, NormalFriction, BouncyForce, MinBounceVelocity, BounceCooldownTime)
  - All damage constants now use GameConstants.Damage (FireHazard, IceHazard, Spike, Lava)
  - All water constants now use GameConstants.Water (HorizontalDrag, VerticalDrag, FrictionMultiplier)
  - All debug constants now use GameConstants.Debug (HitboxBorderThickness, GroundIndicatorSize, GroundIndicatorOffsetX/Y)
  - All opacity values now use GameConstants.Opacity
- [x] Methods are well-separated - **EXISTING DESIGN MAINTAINED**
  - ProcessInput() - Handles keyboard input
  - UpdatePhysics() - Handles physics simulation
  - CheckCollisions() - Handles collision detection and resolution
  - UpdateAnimation() - Handles sprite animation
  - CalculateInteraction() - Calculates surface interaction effects
  - Update() - Main update method (delegates to UpdateAnimation for IGameObject interface)
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - XML docs for all public methods with parameter descriptions
  - Clear explanations of complex logic (coyote time, bounce mechanics, damage cooldown)
- [x] Build passed with 0 errors, 0 warnings
- [ ] Extract to component classes - **DEFERRED** (Current method separation works well; extracting to separate component classes would require significant architectural changes better suited for a future refactoring phase)

---

### Phase 4: Implement State Pattern

#### Step 4.1: Create State Interface ✅ COMPLETED
- [x] Create `States/IGameState.cs` interface - **DONE**
  - Created States folder in project structure
  - Implemented IGameState interface with all required lifecycle methods:
    - `Enter()` - Called when entering the state for initialization
    - `Exit()` - Called when exiting the state for cleanup
    - `Update(GameTime)` - Updates state logic every frame
    - `Draw(SpriteBatch)` - Renders state visuals every frame
    - `HandleInput(KeyboardState, KeyboardState)` - Processes keyboard input (includes previous state for edge detection)
  - Added comprehensive XML documentation for interface and all methods
  - Build passed with 0 errors, 0 warnings

#### Step 4.2: Create Base State Class ✅ COMPLETED
- [x] Create `States/BaseGameState.cs` abstract class - **DONE**
  - Inherits from IGameState interface
  - Provides protected access to Game1 instance
  - Quick access properties for GraphicsDevice, SpriteBatch, and screen dimensions
  - Includes ScreenWidth, ScreenHeight, ScreenCenterX, ScreenCenterY helper properties
- [x] Implement common state functionality - **DONE**
  - Virtual Enter() and Exit() methods with default empty implementations
  - Abstract Update(), Draw(), and HandleInput() methods (must be implemented by derived classes)
  - Helper method: IsKeyPressed() - Edge-triggered key detection
  - Helper method: IsKeyDown() - Check if key is held
  - Helper method: DrawCenteredText() - Draw text centered horizontally
  - Helper method: DrawFilledRectangle() - Draw filled rectangles for UI
  - Helper method: DrawRectangleBorder() - Draw rectangle borders for UI
  - Comprehensive XML documentation for all members
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.3: Extract Main Menu State ✅ COMPLETED
- [x] Create `States/MainMenuState.cs` - **DONE**
  - Inherits from BaseGameState
  - Holds menu state (_selectedMenuOption, menu options array)
  - Uses Action<GameState> callback for state transitions
  - Uses Action callback for exiting the game
  - Accepts required textures and font in constructor
- [x] Move main menu logic from Game1 - **DONE**
  - Menu navigation with Up/Down or W/S keys
  - Menu selection with Enter or Space
  - Exit game with Escape key
  - Option cycling with modulo arithmetic
  - Extracted SelectCurrentOption() method for cleaner code
- [x] Move main menu rendering from Game1 - **DONE**
  - Background rendering with start page texture
  - Semi-transparent menu background with border
  - Menu options with selection highlighting
  - Arrow indicator for selected option
  - Text shadow for depth effect
  - Controls hint at bottom of screen
  - Uses GameConstants for all dimensions and values
  - Split into helper methods: DrawMenuOptions() and DrawControlsHint()
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - Constructor parameter documentation
  - Method documentation for all public and private methods
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.4: Extract Playing State ✅ COMPLETED
- [x] Create `States/PlayingState.cs` - **DONE**
  - Inherits from BaseGameState
  - Accepts all game entities as constructor parameters (players, keys, doors, flames, ice shards, platforms)
  - Holds game state (_doorsOpening, _showHitboxes, _showTimerInfo)
  - Uses Action<GameState> callback for state transitions
  - Manages collision timer for fixed timestep physics
- [x] Move gameplay logic from Game1 - **DONE**
  - Player input processing
  - Physics updates with fixed timestep (via GlobalTimer)
  - Collision detection for both players
  - Animation updates for all entities
  - Key collection detection (CheckKeyCollection method)
  - Door opening logic when both keys collected
  - Victory condition checking (CheckVictoryCondition method)
  - Game over condition checking (CheckGameOverCondition method)
  - Debug toggle handling (H for hitboxes, T for timer info)
  - Pause handling (Escape key)
- [x] Move gameplay rendering from Game1 - **DONE**
  - Level background rendering
  - Door rendering (behind players)
  - Flame and ice shard animations
  - Key rendering
  - Player rendering
  - Debug hitbox visualization (DrawDebugInfo method)
  - Surface type legend (DrawSurfaceTypeLegend method)
  - Health bars with gradient colors (DrawHealthBar method)
  - Key icons next to health bars (DrawPlayerKeyIcon method)
  - Controls display
  - Timer debug info (DrawTimerInfo method)
  - All UI rendering split into helper methods
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - All 17 constructor parameters documented
  - Method documentation for all public and private methods
  - Clear separation of concerns with helper methods
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.5: Extract Game Over State ✅ COMPLETED
- [x] Create `States/GameOverState.cs` - **DONE**
  - Inherits from BaseGameState
  - Holds game over timer
  - Uses Action callback for restarting the game
  - Accepts required textures, font, and players
- [x] Move game over logic from Game1 - **DONE**
  - Timer tracking for delay before allowing restart
  - Restart input handling (Enter or Space after delay)
  - Debug logging for timer and restart events
- [x] Move game over rendering from Game1 - **DONE**
  - Greyed-out game world rendering (50% gray)
  - Semi-transparent black overlay (70% opacity)
  - Red modal window centered on screen
  - "GAME OVER" text centered in modal window
  - Restart prompt displayed after delay period
  - Uses GameConstants for all dimensions and delays
  - Split into helper methods: DrawGameOverText() and DrawRestartPrompt()
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - Constructor parameter documentation
  - Method documentation for all public and private methods
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.6: Extract Paused State ✅ COMPLETED
- [x] Create `States/PausedState.cs` - **DONE**
  - Inherits from BaseGameState
  - Holds selected pause option (_selectedPauseOption)
  - Uses Action<GameState> callback for state transitions
  - Uses Action callback for exiting the game
  - Accepts all game entities for background rendering (players, doors, keys, flames, ice shards)
- [x] Move pause logic from Game1 - **DONE**
  - Menu navigation with Up/Down or W/S keys
  - Menu selection with Enter or Space
  - Resume with Escape key (toggle pause)
  - Option cycling with modulo arithmetic
  - Extracted SelectCurrentOption() method for cleaner code
- [x] Move pause rendering from Game1 - **DONE**
  - Dimmed game world rendering (60% opacity)
  - All game entities rendered frozen in place (doors, flames, ice shards, keys, players)
  - Semi-transparent black overlay (50% opacity)
  - Menu with semi-transparent background and cyan border
  - "PAUSED" title in cyan with shadow effect
  - Menu options with selection highlighting (yellow for selected, white for others)
  - Arrow indicator for selected option
  - Cyan highlight background for selection
  - Text shadows for depth effect
  - Controls hint at bottom of screen
  - Fallback colored blocks when no font available (green for Resume, red for Exit)
  - Uses GameConstants for all dimensions and values
  - Split into helper methods: DrawGameWorld(), DrawPauseMenu(), DrawTitle(), DrawMenuOptions(), DrawControlsHint(), DrawFallbackMenu()
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - All 15 constructor parameters documented
  - Method documentation for all public and private methods
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.7: Extract Victory State ✅ COMPLETED
- [x] Create `States/VictoryState.cs` - **DONE**
  - Inherits from BaseGameState
  - Holds victory timer
  - Uses Action<GameState> callback for state transitions
  - Uses Action callback for restarting the game
  - Accepts required textures, font, players, and doors
- [x] Move victory logic from Game1 - **DONE**
  - Timer tracking to auto-transition after display time (3 seconds)
  - Automatic restart and return to main menu after timer
  - Debug logging for timer and state transition
  - RestartGame callback followed by state transition to MainMenu
- [x] Move victory rendering from Game1 - **DONE**
  - Full color game world background
  - Open doors and players at victory positions
  - Semi-transparent black overlay (60% opacity)
  - "CONGRATULATIONS!" text in gold with shadow (2.5x scale)
  - "Level Complete!" subtitle in white with shadow (1.5x scale)
  - Proper text centering and positioning
  - Fallback gold rectangle when no font available
  - Uses GameConstants for opacity values and display time
  - Split into helper methods: DrawVictoryMessage(), DrawSubtitle(), DrawFallbackVictory()
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - All 11 constructor parameters documented
  - Method documentation for all public and private methods
- [x] Build passed with 0 errors, 0 warnings

#### Step 4.8: Create State Manager and Integrate into Game1 ✅ COMPLETED
- [x] Create `States/StateManager.cs` - **DONE**
  - Dictionary-based state storage using GameState enum as key
  - RegisterState() method to add states to the manager
  - SetInitialState() method to set starting state without lifecycle calls
  - Helper methods: IsStateRegistered(), RegisteredStateCount, GetCurrentState()
- [x] Implement state switching logic - **DONE**
  - ChangeState() method handles state transitions
  - Validates that target state is registered before switching
  - Thread-safe state tracking with _currentState and _currentStateEnum
  - Debug logging for all state transitions
- [x] Handle state lifecycle (Enter/Exit) - **DONE**
  - Calls Exit() on current state before switching
  - Calls Enter() on new state after switching
  - Proper lifecycle management ensures clean transitions
  - Initial state setup separated from lifecycle
- [x] Delegation methods - **DONE**
  - Update(GameTime) - Delegates to current state's Update
  - Draw(SpriteBatch) - Delegates to current state's Draw
  - HandleInput() - Delegates to current state's HandleInput with previous keyboard state tracking
  - Tracks previous keyboard state for edge-triggered input
  - Null checks with warnings for all delegation methods
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation explaining purpose
  - Method documentation for all public methods
  - Property documentation
  - Parameter and return value documentation
- [x] Update Game1 to use StateManager - **DONE**
  - Added StateManager field to Game1
  - Created InitializeStateManager() method in LoadContent()
  - Registered all 5 states (MainMenu, Playing, GameOver, Paused, Victory) with proper dependencies
  - Set initial state to MainMenu
  - Updated Update() to delegate to StateManager.HandleInput() and StateManager.Update()
  - Updated Draw() to delegate to StateManager.Draw()
  - Removed all old state management code:
    - Removed old Update methods (UpdatePlaying, UpdateMainMenu, UpdatePaused, UpdateGameOver, UpdateVictory)
    - Removed old Draw methods (DrawPlaying, DrawMainMenu, DrawPaused, DrawGameOver, DrawVictory)
    - Removed helper methods moved to states (CheckKeyCollection, DrawHealthBar, DrawPlayerKeyIcon)
    - Removed obsolete fields (_doorsOpening, removed _collisionMethod reference)
  - Wired up state transition callbacks using Action<GameState>
  - Wired up game exit and restart callbacks
- [x] Build passed with 0 errors, 0 warnings
- **Phase 4 Complete**: State Pattern fully implemented and integrated!

---

### Phase 5: Implement Systems Architecture

#### Step 5.1: Create Rendering System ✅ COMPLETED
- [x] Create `Systems/RenderingSystem.cs` - **DONE**
  - Created Systems folder in project structure
  - Comprehensive RenderingSystem class with full sprite batch management
  - Supports IRenderable interface from Phase 2
- [x] Manage render order - **DONE**
  - Register/Unregister methods for managing renderables
  - Automatic sorting by DrawOrder property
  - AutoSort property to enable/disable automatic sorting
  - Manual Sort() method for explicit sorting
  - GetRenderablesInRange() method to query specific layers
- [x] Handle sprite batch begin/end - **DONE**
  - Automatic SpriteBatch.Begin() and End() in Draw() method
  - Configurable sprite batch parameters (sort mode, blend state, sampler state, etc.)
  - Configure() method to set rendering settings
  - DrawLayer() method to render specific DrawOrder ranges
  - DrawWithExternalBatch() for manual batch control
- [x] Implement camera system support - **DONE**
  - SetCameraTransform() method to apply camera matrix
  - Transformation matrix parameter in Configure() method
  - Full support for camera transformations via Matrix
- [x] Add comprehensive XML documentation - **DONE**
  - Class-level documentation
  - Method documentation for all public methods
  - Property documentation
  - Parameter and return value documentation
- [x] Build passed with 0 errors, 0 warnings

#### Step 5.2: Create Physics System ✅ COMPLETED
- [x] Create `Systems/PhysicsSystem.cs` - **DONE**
  - Created IPhysicsEntity interface for physics-enabled entities
  - Comprehensive PhysicsSystem class with full physics management
  - Position, velocity, and acceleration calculations
- [x] Centralize gravity and movement logic - **DONE**
  - Gravity application with terminal velocity (max fall speed)
  - Velocity and position updates
  - Air resistance support
  - Fixed timestep and variable timestep support
- [x] Extract common physics logic - **DONE**
  - ApplyImpulse() for instantaneous forces (jumping, bouncing)
  - ApplyForce() for continuous forces (wind, thrust)
  - ApplyFriction() for surface friction
  - ClampVelocity() for speed limits
  - SetVelocity() and Stop() for direct control
  - PredictMovement() for collision prediction
  - CalculateKineticEnergy() for physics calculations
  - IsMoving() for movement detection
- [x] Add comprehensive XML documentation - **DONE**
  - Interface and class-level documentation
  - Method documentation for all public methods
  - Property documentation
  - Parameter and return value documentation
- [x] Build passed with 0 errors, 0 warnings

#### Step 5.3: Create Collision System Enhancement ✅ COMPLETED
- [x] Create enhanced `Systems/CollisionDetectionSystem.cs` - **DONE**
  - New comprehensive collision detection system in Systems folder
  - Works with ICollidable interface from Phase 2
  - Existing CollisionSystem.cs preserved (contains SurfaceType, InteractableObject, InteractionResult)
- [x] Implement spatial partitioning - **DONE**
  - Grid-based spatial partitioning for broad-phase collision detection
  - Configurable cell size (default 128 pixels)
  - Automatically rebuilds grid as objects move
  - Can be toggled on/off with UseSpatialPartitioning property
  - Significantly reduces collision checks from O(n²) to O(n)
- [x] Implement grid-based collision detection - **DONE**
  - Two-phase collision detection: broad-phase (grid) + narrow-phase (AABB)
  - Duplicate check elimination using HashSet
  - CollisionStats tracking (broad phase checks, narrow phase checks, collisions found)
  - GetDiagnostics() method for performance monitoring
- [x] Add advanced collision features - **DONE**
  - QueryArea() - Find all collidables in a rectangular area
  - FindClosest() - Find closest collidable to a point
  - Raycast() - Ray-rectangle intersection testing
  - CollisionDetected event for collision notifications
  - Register/Unregister/Clear for entity management
- [x] Add comprehensive XML documentation - **DONE**
  - CollisionEventArgs class for event data
  - CollisionStats struct for performance statistics
  - Full documentation for all public methods and properties
- [x] Build passed with 0 errors, 0 warnings

#### Step 5.4: Create Input System ✅ COMPLETED
- [x] Create `Systems/InputSystem.cs` - **DONE**
  - Comprehensive input management system
  - GameAction enum for high-level game actions
  - InputState enum for tracking key states
- [x] Centralize keyboard input handling - **DONE**
  - Automatic keyboard state tracking (current and previous frame)
  - Update() method called once per frame
  - InputEnabled property to disable input during cutscenes
  - Edge detection for pressed/released/held states
- [x] Support input mapping/rebinding - **DONE**
  - BindAction() - Bind multiple keys to game actions
  - UnbindAction() / UnbindKey() - Remove bindings
  - GetBoundKeys() - Query current bindings
  - ResetToDefaults() - Restore default bindings
  - WaitForKeyPress() - For rebinding UI
  - GetKeyName() - Human-readable key names
- [x] Add input query methods - **DONE**
  - IsActionPressed() - Action just pressed this frame
  - IsActionHeld() - Action being held
  - IsActionReleased() - Action just released
  - IsActionActive() - Action pressed or held
  - IsKeyPressed/Held/Released/Down() - Direct key queries
  - GetActionState() / GetKeyState() - Get InputState enum
- [x] Add convenience methods - **DONE**
  - GetHorizontalAxis() - Returns -1/0/1 for left/right movement
  - GetVerticalAxis() - Returns -1/0/1 for up/down menu navigation
  - AnyKeyPressed() - Detect any key press
  - GetPressedKeys() - Get all keys pressed this frame
  - GetDiagnostics() - Performance monitoring
- [x] Add comprehensive XML documentation - **DONE**
  - Full documentation for all enums, methods, and properties
  - Usage examples in documentation comments
- [x] Build passed with 0 errors, 0 warnings

#### Step 5.5: Create Entity Manager ✅ COMPLETED
- [x] Create `Systems/EntityManager.cs` - **DONE**
  - Comprehensive entity management system
  - Works with IGameObject interface from Phase 2
  - EntityEventArgs for lifecycle events
  - EntityManagerStats struct for statistics
- [x] Manage all game entities - **DONE**
  - AddEntity() / RemoveEntity() - Add/remove entities
  - Clear() - Remove all entities
  - GetAllEntities() - Get read-only list of all entities
  - EntityAdded / EntityRemoved events
  - Deferred add/remove during updates (prevents collection modification issues)
- [x] Handle entity lifecycle (spawn/destroy) - **DONE**
  - Safe addition during updates (deferred to end of frame)
  - Safe removal during updates (deferred to end of frame)
  - ProcessPendingChanges() - Handles deferred operations
  - Entity event notifications (EntityAdded, EntityRemoved)
- [x] Implement entity queries - **DONE**
  - GetEntitiesOfType<T>() - Get entities by type
  - GetEntitiesWithTag(tag) - Get entities by tag
  - GetEntities(predicate) - Get entities matching condition
  - GetFirstEntity(predicate) - Get first matching entity
  - CountEntities(predicate) - Count matching entities
  - AnyEntity(predicate) - Check if any match exists
- [x] Add entity grouping by tags - **DONE**
  - TagEntity() / UntagEntity() - Add/remove tags
  - HasTag() - Check if entity has tag
  - GetAllTags() - Get all unique tags
  - Multiple tags per entity support
  - Tag-based queries and updates
- [x] Add batch operations - **DONE**
  - Update() - Update all updateable entities
  - UpdateEntitiesOfType<T>() - Update specific type
  - UpdateEntitiesWithTag() - Update tagged entities
  - ForEach() / ForEachOfType() / ForEachWithTag() - Execute actions
- [x] Add comprehensive XML documentation - **DONE**
  - Full documentation for all methods and properties
  - EntityEventArgs and EntityManagerStats structs documented
- [x] Build passed with 0 errors, 0 warnings

---

### Phase 6: Implement Level System

#### Step 6.1: Create Level Interface ✅ COMPLETED
- [x] Create `Levels/ILevel.cs` interface - **DONE**
  - Comprehensive ILevel interface with all required properties and methods
  - LevelId and LevelName for identification
  - Player1StartPosition and Player2StartPosition for spawn points
  - Platforms property for level platforms
  - IsLoaded property to track load state
  - Load() and Unload() methods for lifecycle management
  - GetEntities() for all game objects
  - GetKeys(), GetDoors(), GetFlames(), GetIceShards() for specific entity types
  - GetBackgroundTexture() for level background
  - Reset() method to reset level to initial state
  - Full XML documentation for all members

#### Step 6.2: Create Level Manager ✅ COMPLETED
- [x] Create `Levels/LevelManager.cs` - **DONE**
  - Comprehensive level management system
  - RegisterLevel() / UnregisterLevel() for level registration
  - LoadLevel() / UnloadCurrentLevel() for level lifecycle
  - TransitionToLevel() for smooth level transitions
  - ReloadCurrentLevel() and ResetCurrentLevel() for level reset
  - GetCurrentLevelEntities/Keys/Doors/Flames/IceShards/Platforms() accessors
  - GetCurrentLevelBackground() for background texture
- [x] Handle level loading/unloading - **DONE**
  - Proper lifecycle management with Enter/Exit semantics
  - Event notifications (LevelLoaded, LevelUnloaded, LevelTransitioning)
  - Thread-safe level tracking
  - Debug logging for all operations
- [x] Manage level transitions - **DONE**
  - TransitionToLevel() with validation
  - Event system for transition notifications
  - Proper cleanup of previous level before loading new one

#### Step 6.3: Refactor Level Loading ✅ COMPLETED
- [x] Create `Levels/Level1.cs` class - **DONE**
  - Implements ILevel interface
  - Level1 ID: "level1", Name: "Level 1: Fire and Ice Temple"
  - Player spawn positions from GameConstants
  - LoadPlatforms() from LevelPlatforms.GetLevel1Platforms()
  - CreateKeys() at spawn positions
  - CreateDoors() at spawn positions
  - CreateHazards() based on platform types (Fire and IceHazard)
  - Proper resource management (pixel texture creation and disposal)
  - Reset() method to reset keys and doors
- [x] Move level data from LevelPlatforms - **DONE**
  - Level1 uses LevelPlatforms.GetLevel1Platforms() for platform data
  - All platform types preserved (Solid, Ice, Sticky, Fire, IceHazard)
  - Damage amounts preserved for hazards
- [x] Integrate with existing systems - **DONE**
  - Uses InteractableObject for platforms (consistent with existing code)
  - Creates animated Flame objects for Fire platforms
  - Creates animated IceShard objects for IceHazard platforms
  - Uses GameConstants for spawn positions

#### Step 6.4: Create Level Factory ✅ COMPLETED
- [x] Create `Levels/LevelFactory.cs` - **DONE**
  - Factory pattern for level creation
  - RegisterLevel() for custom level registration
  - CreateLevel(levelId) to create levels by ID
  - CreateLevelByNumber(levelNumber) for numeric level selection
  - CreateAllLevels() to instantiate all registered levels
  - ValidateAllLevels() for testing all level creators
  - GetRegisteredLevelIds() to query available levels
- [x] Centralize level creation logic - **DONE**
  - RegisterDefaultLevels() automatically registers Level1
  - Func<ILevel> creator pattern for lazy instantiation
  - Exception handling with debug logging
  - CreateDefault() static factory method

---

### Phase 7: Separate Concerns in Game1

#### Step 7.1: Extract Content Management
- [ ] Create `Content/ContentManager.cs` wrapper
- [ ] Centralize all Content.Load calls
- [ ] Implement resource caching

#### Step 7.2: Extract UI Rendering
- [ ] Create `UI/UIManager.cs`
- [ ] Move HUD rendering logic
- [ ] Create UI components (HealthBar, KeyIcon, etc.)

#### Step 7.3: Simplify Game1.cs
- [ ] Game1 should only:
  - Initialize systems
  - Forward Update/Draw to StateManager
  - Handle MonoGame lifecycle
- [ ] Target: Reduce to under 200 lines

---

### Phase 8: Improve Code Quality

#### Step 8.1: Add XML Documentation
- [ ] Document all public APIs
- [ ] Add class-level documentation
- [ ] Document complex algorithms

#### Step 8.2: Implement SOLID Principles
- [ ] Single Responsibility: Each class does one thing
- [ ] Open/Closed: Use interfaces for extensibility
- [ ] Liskov Substitution: Ensure proper inheritance
- [ ] Interface Segregation: Split large interfaces
- [ ] Dependency Inversion: Depend on abstractions

#### Step 8.3: Apply Design Patterns
- [ ] Observer Pattern: For game events
- [ ] Factory Pattern: For entity creation
- [ ] Strategy Pattern: For AI behaviors (if needed)
- [ ] Command Pattern: For input handling (optional)

#### Step 8.4: Improve Error Handling
- [ ] Add try-catch where appropriate
- [ ] Add null checks
- [ ] Add validation for parameters

---

### Phase 9: Configuration and Data

#### Step 9.1: Create Configuration System
- [ ] Create `Config/GameConfig.cs`
- [ ] Move all configuration to JSON file
- [ ] Load configuration at startup

#### Step 9.2: Externalize Level Data
- [ ] Create JSON format for levels
- [ ] Implement level loader from JSON
- [ ] Move LevelPlatforms data to files

#### Step 9.3: Create Save/Load System
- [ ] Create `Data/SaveManager.cs`
- [ ] Implement game state serialization
- [ ] Add checkpoint system (optional)

---

### Phase 10: Testing and Optimization

#### Step 10.1: Add Unit Tests
- [ ] Set up test project
- [ ] Test collision detection
- [ ] Test physics calculations
- [ ] Test state transitions

#### Step 10.2: Performance Optimization
- [ ] Profile the game
- [ ] Optimize rendering (sprite batching)
- [ ] Optimize collision detection
- [ ] Reduce allocations in Update loops

#### Step 10.3: Code Review and Cleanup
- [ ] Remove commented-out code
- [ ] Ensure consistent naming conventions
- [ ] Format code consistently
- [ ] Remove any remaining code smells

---

## Folder Structure (Target)

```
fire-and-ice/
├── Core/
│   ├── Interfaces/
│   │   ├── IGameObject.cs
│   │   ├── IRenderable.cs
│   │   ├── IUpdateable.cs
│   │   └── ICollidable.cs
│   ├── Constants.cs
│   └── AnimatedObject.cs
├── Entities/
│   ├── Player.cs
│   ├── Key.cs
│   ├── Door.cs
│   └── Hazards/
│       ├── Flame.cs
│       └── IceShard.cs
├── States/
│   ├── IGameState.cs
│   ├── BaseGameState.cs
│   ├── StateManager.cs
│   ├── MainMenuState.cs
│   ├── PlayingState.cs
│   ├── GameOverState.cs
│   ├── PausedState.cs
│   └── VictoryState.cs
├── Systems/
│   ├── RenderingSystem.cs
│   ├── PhysicsSystem.cs
│   ├── CollisionSystem.cs
│   ├── InputSystem.cs
│   └── EntityManager.cs
├── Levels/
│   ├── ILevel.cs
│   ├── LevelManager.cs
│   ├── LevelFactory.cs
│   ├── Level1.cs
│   └── CollisionMapReader.cs
├── UI/
│   ├── UIManager.cs
│   ├── HealthBar.cs
│   └── KeyIcon.cs
├── Content/
│   └── ContentManager.cs
├── Config/
│   └── GameConfig.cs
├── Data/
│   └── SaveManager.cs
├── Utilities/
│   └── GlobalTimer.cs
└── Game1.cs (simplified)
```

---

## Priority Recommendations

### High Priority (Do First)
1. **Phase 1** - Remove unused code (quick wins)
2. **Phase 2** - Create core abstractions (foundation)
3. **Phase 4** - Implement State Pattern (major improvement)
4. **Phase 7** - Simplify Game1.cs (architectural improvement)

### Medium Priority (Do Next)
5. **Phase 3** - Refactor game objects
6. **Phase 5** - Implement systems architecture
7. **Phase 8** - Improve code quality

### Low Priority (Nice to Have)
8. **Phase 6** - Implement level system
9. **Phase 9** - Configuration and data
10. **Phase 10** - Testing and optimization

---

## Notes

- Each checkbox represents a discrete, testable task
- Build and test after completing each step
- Commit to version control after each phase
- Some steps may be combined or reordered based on dependencies
- Estimated total effort: 40-60 hours of focused work
- Can be done incrementally while maintaining working game

---

## Benefits After Refactoring

1. **Maintainability**: Easier to find and fix bugs
2. **Extensibility**: Simple to add new features (levels, enemies, power-ups)
3. **Testability**: Can unit test individual components
4. **Readability**: Clear separation of concerns
5. **Reusability**: Components can be reused in other projects
6. **Performance**: Optimized systems and reduced coupling
7. **Scalability**: Can grow without becoming unmaintainable

---

**Last Updated**: December 2, 2025
**Project**: Fire and Ice (C# MonoGame)
**Total Steps**: 100+ discrete refactoring tasks
