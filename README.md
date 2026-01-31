# RPG Game

A 2D RPG game built with Godot 4.6 and C#.

## Project Structure

```
/
├── assets/                     # Raw assets (art, audio, fonts)
│   ├── sprites/               # All sprite assets
│   │   ├── backgrounds/       # Scene backgrounds
│   │   ├── characters/        # Player, Enemy, and NPC sprites
│   │   ├── effects/           # Visual effect sprites
│   │   ├── items/             #  Item/pickup sprites
│   │   └── ui/                # UI elements (buttons, icons) 
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
│   ├── dialogues/             # Dialogue trees
│   ├── quests/                # Quest definitions
│   └── themes/                # Godot themes
│
└── addons/                     # Third-party Godot plugins
```

## Directory Purposes

### `addons/`
Third-party plugins from the Godot Asset Library or custom tooling.

### `assets/`
Raw asset files organized by type. Sprites are further categorized by their usage in the game. This keeps art, audio, and fonts separate from game logic.

### `resources/`
Data files (`.tres`) that use custom Resource classes. Resources are Godot's way of storing game data like item stats, dialogue content, and quest objectives.

### `scenes/`
All `.tscn` scene files. Each scene represents a reusable node tree. Scenes are organized to mirror the game's structure:
- **characters/**: Anything that moves and has behavior
- **effects/**: Visual effects like particles
- **items/**: Collectibles and equipment
- **levels/**: Game world and map layouts
- **ui/**: All user interface elements

### `scripts/`
All C# `.cs` files. Scripts are organized to match their corresponding scenes where applicable.

- **autoload/**: Scripts registered as singletons in Project Settings. These are globally accessible (e.g., `GameManager`, `AudioManager`, `EventBus`).
- **systems/**: Core game mechanics that don't attach directly to a single scene (inventory logic, combat calculations, save/load).

## Conventions

### Naming
- **Scenes**: `PascalCase.tscn` (e.g., `Player.tscn`, `MainMenu.tscn`)
- **Scripts**: `PascalCase.cs` (e.g., `Player.cs`, `InventorySystem.cs`)
- **Resources**: `snake_case.tres` (e.g., `iron_sword.tres`, `quest_dragon_slayer.tres`)
- **Assets**: `snake_case` with descriptive names (e.g., `player_idle.png`, `battle_theme.ogg`)

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
Anything that implements `ISavable` will be saved via the `SaveManager`.

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

1. Open the project in Godot 4.6+ with .NET support
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

https://www.spriters-resource.com/pc_computer/maplestory/
https://www.spriters-resource.com/pc_computer/maplestory/asset/21703/