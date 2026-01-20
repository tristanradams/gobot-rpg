extends Control

@export var fade_duration: float = 2.0

@onready var _content: VBoxContainer = $VBoxContainer


func _ready() -> void:
	# Start content fully transparent (background stays visible)
	_content.modulate.a = 0.0

	# Fade in content only
	var tween := create_tween()
	tween.tween_property(_content, "modulate:a", 1.0, fade_duration)


func _on_start_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/levels/world/main.tscn")
	
func _on_load_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/levels/world/main.tscn")

func _on_credits_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/ui/menus/Credits.tscn")

func _on_quit_pressed() -> void:
	get_tree().quit()
