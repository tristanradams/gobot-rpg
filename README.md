# RPG Game

A 2D RPG game built with Godot 4.5 and C#.

## Project Structure

```
/
├── assets/                     # Raw assets (art, audio, fonts)
│   ├── sprites/               # All sprite assets
│   │   ├── characters/        # Player and NPC sprites
│   │   ├── enemies/           # Enemy sprites
│   │   ├── items/             # Item/pickup sprites
│   │   ├── ui/                # UI elements (buttons, icons)
│   │   ├── tilesets/          # Tileset images for levels
│   │   └── effects/           # Visual effect sprites
│   ├── audio/
│   │   ├── music/             # Background music tracks
│   │   └── sfx/               # Sound effects
│   └── fonts/                 # Custom fonts
│
├── scenes/                     # All .tscn scene files
│   ├── characters/
│   │   ├── player/            # Player scene(s)
│   │   ├── npcs/              # NPC scenes
│   │   └── enemies/           # Enemy scenes
│   ├── ui/
│   │   ├── hud/               # In-game HUD elements
│   │   ├── menus/             # Main menu, pause menu, etc.
│   │   └── dialogs/           # Dialogue boxes, popups
│   ├── levels/
│   │   └── world/             # Game world/map scenes
│   ├── items/                 # Pickup and equipment scenes
│   └── effects/               # Particle and visual effect scenes
│
├── scripts/                    # All C# .cs files
│   ├── characters/
│   │   ├── player/            # Player logic
│   │   ├── npcs/              # NPC behavior and AI
│   │   └── enemies/           # Enemy AI and behavior
│   ├── ui/                    # UI controller scripts
│   ├── systems/               # Core game systems
│   │   ├── inventory/         # Inventory management
│   │   ├── combat/            # Combat mechanics
│   │   ├── dialogue/          # Dialogue system
│   │   ├── quest/             # Quest tracking
│   │   └── save_load/         # Save/load functionality
│   ├── resources/             # Custom Resource class definitions
│   └── autoload/              # Singleton/autoload scripts
│
├── resources/                  # Custom Resource data files (.tres)
│   ├── items/                 # Item definitions
│   ├── characters/            # Character stats/data
│   ├── quests/                # Quest definitions
│   └── dialogues/             # Dialogue trees
│
└── addons/                     # Third-party Godot plugins
```

## Directory Purposes

### `assets/`
Raw asset files organized by type. Sprites are further categorized by their usage in the game. This keeps art, audio, and fonts separate from game logic.

### `scenes/`
All `.tscn` scene files. Each scene represents a reusable node tree. Scenes are organized to mirror the game's structure:
- **characters/**: Anything that moves and has behavior
- **ui/**: All user interface elements
- **levels/**: Game world and map layouts
- **items/**: Collectibles and equipment
- **effects/**: Visual effects like particles

### `scripts/`
All C# `.cs` files. Scripts are organized to match their corresponding scenes where applicable.

- **autoload/**: Scripts registered as singletons in Project Settings. These are globally accessible (e.g., `GameManager`, `AudioManager`, `EventBus`).
- **systems/**: Core game mechanics that don't attach directly to a single scene (inventory logic, combat calculations, save/load).
- **resources/**: Class definitions that extend `Resource` for custom data types.

### `resources/`
Data files (`.tres`) that use custom Resource classes. Resources are Godot's way of storing game data like item stats, dialogue content, and quest objectives.

### `addons/`
Third-party plugins from the Godot Asset Library or custom tooling.

## Conventions

### Naming
- **Scenes**: `PascalCase.tscn` (e.g., `Player.tscn`, `MainMenu.tscn`)
- **Scripts**: `PascalCase.cs` (e.g., `Player.cs`, `InventorySystem.cs`)
- **Resources**: `snake_case.tres` (e.g., `iron_sword.tres`, `quest_dragon_slayer.tres`)
- **Assets**: `snake_case` with descriptive names (e.g., `player_idle.png`, `battle_theme.ogg`)

### Script Organization
Each script should follow this structure:
```csharp
using Godot;

namespace RpgCSharp.scripts.characters;

public partial class Character : CharacterBody2D
{
    // Constants
    public static class Anim
    {
        public const string Idle = "idle";
        public const string Walk = "walk";
    }

    // Enums
    public enum State { Idle, Walking, Running }

    // Exports (inspector variables)
    [Export] public float Speed { get; set; } = 100.0f;

    // Signals
    [Signal] public delegate void HealthChangedEventHandler(int newHealth);

    // Protected/private fields
    protected State _state = State.Idle;
    private int _health = 100;

    // Node references
    protected AnimatedSprite2D Sprite;

    // Built-in callbacks
    public override void _Ready()
    {
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    // Public methods
    public void TakeDamage(int amount)
    {
    }

    // Private methods
    private void UpdateState()
    {
    }
}
```

## Autoload Singletons

Registered in **Project > Project Settings > Autoload**:

| Name | Path | Purpose |
|------|------|---------|
| `GameManager` | `scripts/autoload/GameManager.cs` | Global game state, pause/resume |
| `EventBus` | `scripts/autoload/EventBus.cs` | Global signal hub |
| `AudioManager` | `scripts/autoload/AudioManager.cs` | Music and SFX playback |
| `SaveManager` | `scripts/autoload/SaveManager.cs` | Save/load handling |

## Features

### Save System
Characters implement a Savable interface via the `Character` base class:
- `SavableId`: Unique identifier (auto-generated from scene path + node path)
- `GatherSaveData()`: Collects position, health, death state
- `ApplySaveData()`: Restores state on load

### Game State Management
`GameManager` tracks game state (`Menu`, `Playing`, `Paused`, `Dialogue`, `Cutscene`) and provides:
- `StartGame()`, `PauseGame()`, `ResumeGame()`, `ReturnToMenu()`
- Player reference for global access

### Platformer Physics
Player includes:
- Coyote time (jump grace period after leaving ground)
- Jump buffering (queue jump input before landing)
- Drop-through platforms (down + jump)

## Getting Started

1. Open the project in Godot 4.5+ with .NET support
2. Build the C# solution
3. Run the main scene (`scenes/ui/menus/MainMenu.tscn`)

## Resources

- [Godot Documentation](https://docs.godotengine.org/)
- [C# in Godot](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [Best Practices](https://docs.godotengine.org/en/stable/tutorials/best_practices/index.html)

## TODOs

- block ability
- dash ability
- double jump ability
- npcs
  - dialog
  - quests?
  - fully stationary?
- save / rest destinations
- respawn all enemies on save
- floating familiar
  - unlocks chests & doors (possibly through equippable item)
  - fires projectiles (possibly through equippable item)