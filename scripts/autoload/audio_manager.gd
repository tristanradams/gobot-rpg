extends Node

# Audio buses
const MASTER_BUS := "Master"
const MUSIC_BUS := "Music"
const SFX_BUS := "SFX"

# Audio players
var _music_player: AudioStreamPlayer
var _sfx_players: Array[AudioStreamPlayer] = []
var _sfx_pool_size: int = 8

# Current music
var _current_music: AudioStream = null


func _ready() -> void:
	_setup_music_player()
	_setup_sfx_pool()


func _setup_music_player() -> void:
	_music_player = AudioStreamPlayer.new()
	_music_player.bus = MUSIC_BUS
	add_child(_music_player)


func _setup_sfx_pool() -> void:
	for i in _sfx_pool_size:
		var player := AudioStreamPlayer.new()
		player.bus = SFX_BUS
		add_child(player)
		_sfx_players.append(player)


func play_music(stream: AudioStream, fade_in: float = 0.5) -> void:
	if stream == _current_music:
		return
	_current_music = stream
	_music_player.stream = stream
	_music_player.volume_db = -80.0 if fade_in > 0 else 0.0
	_music_player.play()
	if fade_in > 0:
		var tween := create_tween()
		tween.tween_property(_music_player, "volume_db", 0.0, fade_in)


func stop_music(fade_out: float = 0.5) -> void:
	if fade_out > 0:
		var tween := create_tween()
		tween.tween_property(_music_player, "volume_db", -80.0, fade_out)
		tween.tween_callback(_music_player.stop)
	else:
		_music_player.stop()
	_current_music = null


func play_sfx(stream: AudioStream, volume_db: float = 0.0) -> void:
	for player in _sfx_players:
		if not player.playing:
			player.stream = stream
			player.volume_db = volume_db
			player.play()
			return
	# All players busy, use the first one
	_sfx_players[0].stream = stream
	_sfx_players[0].volume_db = volume_db
	_sfx_players[0].play()


func set_master_volume(volume: float) -> void:
	AudioServer.set_bus_volume_db(
		AudioServer.get_bus_index(MASTER_BUS),
		linear_to_db(volume)
	)


func set_music_volume(volume: float) -> void:
	AudioServer.set_bus_volume_db(
		AudioServer.get_bus_index(MUSIC_BUS),
		linear_to_db(volume)
	)


func set_sfx_volume(volume: float) -> void:
	AudioServer.set_bus_volume_db(
		AudioServer.get_bus_index(SFX_BUS),
		linear_to_db(volume)
	)
