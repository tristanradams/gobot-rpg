extends Node

const SAVE_DIR := "user://saves/"
const SAVE_EXTENSION := ".save"
const SETTINGS_PATH := "user://settings.cfg"

# Save data structure
var _current_save: Dictionary = {}


func _ready() -> void:
	_ensure_save_directory()


func _ensure_save_directory() -> void:
	DirAccess.make_dir_recursive_absolute(SAVE_DIR)


func save_game(slot: int = 0) -> bool:
	var save_data := _gather_save_data()
	var path := _get_save_path(slot)

	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("Failed to open save file: %s" % path)
		return false

	file.store_var(save_data)
	file.close()
	return true


func load_game(slot: int = 0) -> bool:
	var path := _get_save_path(slot)

	if not FileAccess.file_exists(path):
		push_error("Save file not found: %s" % path)
		return false

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Failed to open save file: %s" % path)
		return false

	_current_save = file.get_var()
	file.close()

	_apply_save_data(_current_save)
	return true


func delete_save(slot: int = 0) -> bool:
	var path := _get_save_path(slot)
	if FileAccess.file_exists(path):
		return DirAccess.remove_absolute(path) == OK
	return false


func has_save(slot: int = 0) -> bool:
	return FileAccess.file_exists(_get_save_path(slot))


func _get_save_path(slot: int) -> String:
	return SAVE_DIR + "slot_%d%s" % [slot, SAVE_EXTENSION]


func _gather_save_data() -> Dictionary:
	# Override this to collect data from your game systems
	return {
		"timestamp": Time.get_unix_time_from_system(),
		"player": {},
		"inventory": {},
		"quests": {},
		"world": {},
	}


func _apply_save_data(data: Dictionary) -> void:
	# Override this to apply loaded data to your game systems
	pass


# Settings (separate from save games)
func save_settings(settings: Dictionary) -> bool:
	var config := ConfigFile.new()
	for section in settings:
		for key in settings[section]:
			config.set_value(section, key, settings[section][key])
	return config.save(SETTINGS_PATH) == OK


func load_settings() -> Dictionary:
	var config := ConfigFile.new()
	if config.load(SETTINGS_PATH) != OK:
		return _get_default_settings()

	var settings := {}
	for section in config.get_sections():
		settings[section] = {}
		for key in config.get_section_keys(section):
			settings[section][key] = config.get_value(section, key)
	return settings


func _get_default_settings() -> Dictionary:
	return {
		"audio": {
			"master_volume": 1.0,
			"music_volume": 0.8,
			"sfx_volume": 1.0,
		},
		"display": {
			"fullscreen": false,
			"vsync": true,
		},
	}
