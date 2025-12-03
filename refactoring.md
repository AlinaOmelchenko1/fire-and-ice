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

#### Step 4.7: Extract Victory State
- [ ] Create `States/VictoryState.cs`
- [ ] Move victory logic from Game1
- [ ] Move victory rendering from Game1

#### Step 4.8: Create State Manager
- [ ] Create `States/StateManager.cs`
- [ ] Implement state switching logic
- [ ] Handle state lifecycle (Enter/Exit)
- [ ] Update Game1 to use StateManager

---

### Phase 5: Implement Systems Architecture

#### Step 5.1: Create Rendering System
- [ ] Create `Systems/RenderingSystem.cs`
- [ ] Manage render order
- [ ] Handle sprite batch begin/end
- [ ] Implement camera system (if needed)

#### Step 5.2: Create Physics System
- [ ] Create `Systems/PhysicsSystem.cs`
- [ ] Move physics calculations from Player
- [ ] Centralize gravity and movement logic

#### Step 5.3: Create Collision System Enhancement
- [ ] Refactor existing `CollisionSystem.cs`
- [ ] Separate spatial partitioning
- [ ] Implement quadtree or grid-based collision (optional)

#### Step 5.4: Create Input System
- [ ] Create `Systems/InputSystem.cs`
- [ ] Centralize keyboard input handling
- [ ] Support input mapping/rebinding

#### Step 5.5: Create Entity Manager
- [ ] Create `Systems/EntityManager.cs`
- [ ] Manage all game entities
- [ ] Handle entity lifecycle (spawn/destroy)
- [ ] Implement entity queries

---

### Phase 6: Implement Level System

#### Step 6.1: Create Level Interface
- [ ] Create `Levels/ILevel.cs` interface
  ```csharp
  interface ILevel
  {
      void Load();
      void Unload();
      List<IGameObject> GetEntities();
  }
  ```

#### Step 6.2: Create Level Manager
- [ ] Create `Levels/LevelManager.cs`
- [ ] Handle level loading/unloading
- [ ] Manage level transitions

#### Step 6.3: Refactor Level Loading
- [ ] Create `Levels/Level1.cs` class
- [ ] Move level data from LevelPlatforms
- [ ] Integrate with CollisionMapReader

#### Step 6.4: Create Level Factory
- [ ] Create `Levels/LevelFactory.cs`
- [ ] Centralize level creation logic

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
