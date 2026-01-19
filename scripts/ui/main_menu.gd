extends Control


func _on_start_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/levels/world/main.tscn")


func _on_credits_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/ui/menus/Credits.tscn")


func _on_quit_pressed() -> void:
	get_tree().quit()
