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

#### Step 1.2: Remove Unused Files
- [ ] Delete or move unused files to a `_Legacy` folder
- [ ] Update namespace references if needed
- [ ] Build and test to ensure nothing breaks

#### Step 1.3: Remove Magic Numbers
- [ ] Create `Constants.cs` file
- [ ] Extract screen dimensions constants
- [ ] Extract physics constants (gravity, jump power, etc.)
- [ ] Extract color constants
- [ ] Extract timing constants (animation speeds, delays)
- [ ] Replace hard-coded values with named constants

---

### Phase 2: Create Core Abstractions

#### Step 2.1: Create Base Interfaces
- [ ] Create `Interfaces/IGameObject.cs` interface
  ```csharp
  interface IGameObject
  {
      void Update(GameTime gameTime);
      void Draw(SpriteBatch spriteBatch);
  }
  ```

#### Step 2.2: Create Rendering Interface
- [ ] Create `Interfaces/IRenderable.cs` interface
  ```csharp
  interface IRenderable
  {
      void Draw(SpriteBatch spriteBatch);
      int DrawOrder { get; }
  }
  ```

#### Step 2.3: Create Update Interface
- [ ] Create `Interfaces/IUpdateable.cs` interface
  ```csharp
  interface IUpdateable
  {
      void Update(GameTime gameTime);
      bool Enabled { get; }
  }
  ```

#### Step 2.4: Create Collision Interface
- [ ] Create `Interfaces/ICollidable.cs` interface
  ```csharp
  interface ICollidable
  {
      Rectangle GetBounds();
      bool CheckCollision(ICollidable other);
  }
  ```

#### Step 2.5: Create Animated Object Base Class
- [ ] Create `Core/AnimatedObject.cs` base class
- [ ] Move common animation logic from Flame and IceShard
- [ ] Implement common Update pattern for animations

---

### Phase 3: Refactor Game Objects

#### Step 3.1: Refactor Key Class
- [ ] Implement `IGameObject` interface
- [ ] Implement `ICollidable` interface
- [ ] Extract drawing constants to class-level
- [ ] Add XML documentation comments

#### Step 3.2: Refactor Door Class
- [ ] Implement `IGameObject` interface
- [ ] Extract animation constants
- [ ] Add state enum for door states (Closed, Opening, Open)

#### Step 3.3: Refactor Flame Class
- [ ] Inherit from `AnimatedObject` base class
- [ ] Implement `IGameObject` interface
- [ ] Extract magic numbers to constants

#### Step 3.4: Refactor IceShard Class
- [ ] Inherit from `AnimatedObject` base class
- [ ] Implement `IGameObject` interface
- [ ] Extract magic numbers to constants

#### Step 3.5: Refactor Player Class
- [ ] Implement `IGameObject` and `ICollidable` interfaces
- [ ] Split into smaller methods (Update is doing too much)
- [ ] Extract physics to separate component
- [ ] Extract input handling to separate component
- [ ] Extract animation to separate component
- [ ] Move collision logic to collision system

---

### Phase 4: Implement State Pattern

#### Step 4.1: Create State Interface
- [ ] Create `States/IGameState.cs` interface
  ```csharp
  interface IGameState
  {
      void Enter();
      void Exit();
      void Update(GameTime gameTime);
      void Draw(SpriteBatch spriteBatch);
      void HandleInput(KeyboardState keyboard);
  }
  ```

#### Step 4.2: Create Base State Class
- [ ] Create `States/BaseGameState.cs` abstract class
- [ ] Implement common state functionality

#### Step 4.3: Extract Main Menu State
- [ ] Create `States/MainMenuState.cs`
- [ ] Move main menu logic from Game1
- [ ] Move main menu rendering from Game1

#### Step 4.4: Extract Playing State
- [ ] Create `States/PlayingState.cs`
- [ ] Move gameplay logic from Game1
- [ ] Move gameplay rendering from Game1

#### Step 4.5: Extract Game Over State
- [ ] Create `States/GameOverState.cs`
- [ ] Move game over logic from Game1
- [ ] Move game over rendering from Game1

#### Step 4.6: Extract Paused State
- [ ] Create `States/PausedState.cs`
- [ ] Move pause logic from Game1
- [ ] Move pause rendering from Game1

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
