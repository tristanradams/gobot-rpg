extends Node

# Game states
enum GameState { MENU, PLAYING, PAUSED, DIALOGUE, CUTSCENE }

# Current state
var current_state: GameState = GameState.MENU
var previous_state: GameState = GameState.MENU

# Player reference
var player: Node = null

# Autoload references
var _event_bus: Node


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_event_bus = get_node("/root/EventBus")


func change_state(new_state: GameState) -> void:
	previous_state = current_state
	current_state = new_state
	_event_bus.game_state_changed.emit(new_state)


func pause_game() -> void:
	if current_state == GameState.PLAYING:
		change_state(GameState.PAUSED)
		get_tree().paused = true


func resume_game() -> void:
	if current_state == GameState.PAUSED:
		change_state(GameState.PLAYING)
		get_tree().paused = false


func start_game() -> void:
	change_state(GameState.PLAYING)


func return_to_menu() -> void:
	get_tree().paused = false
	change_state(GameState.MENU)
