# RPG Game

A 2D RPG game built with Godot 4.5.

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
├── scripts/                    # All .gd GDScript files
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
All GDScript `.gd` files. Scripts are organized to match their corresponding scenes where applicable.

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
- **Scripts**: `snake_case.gd` (e.g., `player_controller.gd`, `inventory_system.gd`)
- **Resources**: `snake_case.tres` (e.g., `iron_sword.tres`, `quest_dragon_slayer.tres`)
- **Assets**: `snake_case` with descriptive names (e.g., `player_idle.png`, `battle_theme.ogg`)

### Script Organization
Each script should follow this structure:
```gdscript
class_name ClassName
extends ParentClass

# Signals
signal health_changed(new_health)

# Enums
enum State { IDLE, WALKING, RUNNING }

# Constants
const MAX_SPEED := 200.0

# Exports (inspector variables)
@export var speed: float = 100.0

# Public variables
var health: int = 100

# Private variables (prefix with _)
var _internal_state: State = State.IDLE

# Onready variables
@onready var sprite: Sprite2D = $Sprite2D

# Built-in callbacks
func _ready() -> void:
    pass

func _process(delta: float) -> void:
    pass

# Public methods
func take_damage(amount: int) -> void:
    pass

# Private methods
func _update_state() -> void:
    pass
```

## Autoload Singletons

Register these in **Project > Project Settings > Autoload**:

| Name | Path | Purpose |
|------|------|---------|
| `GameManager` | `scripts/autoload/game_manager.gd` | Global game state |
| `EventBus` | `scripts/autoload/event_bus.gd` | Global signal hub |
| `AudioManager` | `scripts/autoload/audio_manager.gd` | Audio playback |
| `SaveManager` | `scripts/autoload/save_manager.gd` | Save/load handling |

## Getting Started

1. Open the project in Godot 4.5+
2. Create your main scene in `scenes/levels/`
3. Set it as the main scene in Project Settings
4. Build your player in `scenes/characters/player/`
5. Create autoload singletons as needed

## Resources

- [Godot Documentation](https://docs.godotengine.org/)
- [GDScript Reference](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_basics.html)
- [Best Practices](https://docs.godotengine.org/en/stable/tutorials/best_practices/index.html)
