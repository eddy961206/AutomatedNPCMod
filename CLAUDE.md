# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an **Automated NPC Mod** for Stardew Valley built using SMAPI (Stardew Modding API). The mod allows players to create and control automated NPCs that can perform farming, foraging, and mining tasks autonomously.

## Development Commands

### Build and Test
```bash
# Build the project
dotnet build

# Clean and rebuild
dotnet clean
dotnet build

# Run specific build configuration
dotnet build --configuration Release
```

### Project Structure Requirements
- The `.csproj` file requires a `<GamePath>` property pointing to the Stardew Valley installation directory
- Example: `<GamePath>D:\Steam\steamapps\common\Stardew Valley</GamePath>`
- This path is used to reference game assemblies (StardewModdingAPI.dll, MonoGame.Framework.dll, etc.)

### Testing in Game
1. Ensure SMAPI is installed in the Stardew Valley directory
2. Build creates `AutomatedNPCMod.dll` in `bin/Debug/net6.0/`
3. Launch Stardew Valley through SMAPI to test the mod
4. Use in-game keyboard shortcuts:
   - **F9**: Create new test NPC at player location
   - **F10**: Assign farming task to most recent NPC
   - **F11**: Display status of all active NPCs
   - **F12**: Remove all active NPCs

## Architecture

### Core Components

**ModEntry.cs** (SMAPI Entry Point)
- Main mod initialization and SMAPI event handling
- Coordinates all manager classes
- Handles game lifecycle events (save/load, update ticks)

**NPCManager.cs** (Core/NPCManager.cs)
- Central management of all custom NPCs
- NPC creation, removal, and lifecycle management
- Data persistence for NPC information
- Location tracking and updates

**TaskManager.cs** (Core/TaskManager.cs)
- Work task creation, assignment, and completion
- Task queuing and NPC availability matching
- Profit calculation and distribution to player
- Task data persistence

**CustomNPC.cs** (Models/CustomNPC.cs)
- Extended NPC class with automation capabilities
- AI controller integration for pathfinding
- Work execution capabilities
- Task assignment and state management

**AIController.cs** (AI/AIController.cs)
- State-based AI system (Idle, Moving, Working)
- Pathfinding and navigation
- Goal-oriented behavior

**WorkExecutor.cs** (Work/WorkExecutor.cs)
- Strategy pattern for different work types
- Actual game interaction for farming/mining/foraging
- Result calculation and reporting

### Key Models

**WorkTask.cs**
- Task definition with type, location, priority
- Tracking assigned NPC and completion status
- Parameter storage for task-specific data

**TaskResult.cs**
- Work completion results
- Items obtained, gold earned, experience gained
- Success/failure status and error handling

## Important Implementation Details

### Data Persistence
- Uses SMAPI's `Helper.Data.WriteSaveData()` and `ReadSaveData()` for persistence
- NPC data saved as `"npc-data"` key
- Task data saved as `"task-data"` key
- Automatic save/load on game save/load events

### Game Integration
- Inherits from Stardew Valley's base `NPC` class for CustomNPC
- Uses game's character system for NPC placement and rendering
- Integrates with game's item and money systems for profit distribution

### Performance Considerations
- Update loop runs every 15 ticks (4 times per second) instead of every tick
- Efficient NPC state management to minimize game impact
- Task queuing system to handle multiple NPCs and work assignments

### Error Handling
- Comprehensive try-catch blocks around all major operations
- SMAPI logging for debugging and monitoring
- Graceful degradation when game objects are unavailable

## Development Patterns

### Dependency Injection
- Manager classes accept IModHelper and IMonitor in constructors
- Consistent logging and data access patterns

### Event-Driven Architecture
- Subscribes to SMAPI events for game lifecycle management
- UI input handling through button press events
- Location change tracking for NPC management

### Modular Design
- Clear separation between NPC management, task management, and UI
- Interface-based work handlers for extensibility
- State machines for AI behavior

## Configuration

The mod uses SMAPI's standard configuration system. Key configuration points:
- Game path must be correctly set in `.csproj` for assembly references
- Manifest.json defines mod metadata and SMAPI version requirements
- No user-configurable settings in current implementation

## Common Development Tasks

When extending this mod:
1. **Adding new work types**: Implement `IWorkHandler` interface and register in `WorkExecutor`
2. **NPC behavior changes**: Modify `AIController` state machine or add new AI states
3. **UI improvements**: Extend `UIManager` with new input handlers or display methods
4. **Data model changes**: Update persistence logic in both save and load methods

## Dependencies

- **.NET 6.0**: Target framework
- **SMAPI 3.18.0+**: Minimum required version
- **Pathoschild.Stardew.ModBuildConfig**: NuGet package for build automation
- **Game assemblies**: StardewModdingAPI.dll, MonoGame.Framework.dll, xTile.dll

## Documentation References

See the following files for detailed project information:
- `system_design_document.md`: Comprehensive system architecture
- `technical_stack_analysis.md`: Technology stack and implementation details
- `stardew_mod_test_guide.md`: Testing procedures and setup instructions