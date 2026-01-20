extends Control

@onready var _event_bus: Node = get_node("/root/EventBus")


func _ready() -> void:
	hide()
	_event_bus.player_died.connect(_on_player_died)


func _on_player_died() -> void:
	get_tree().paused = true
	show()


func _on_main_menu_pressed() -> void:
	get_tree().paused = false
	get_tree().change_scene_to_file("res://scenes/ui/menus/MainMenu.tscn")
