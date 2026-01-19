class_name Orc
extends "res://scripts/characters/character.gd"

# Orc-specific stats
@export var damage: int = 10
@export var detection_range: float = 150.0
@export var attack_range: float = 30.0

# References
var _player: Node2D = null


func _character_ready() -> void:
	speed = 80.0
	max_health = 50
	_health = max_health
	_find_player()


func _physics_process(_delta: float) -> void:
	if _state == State.DEAD:
		return

	if not _player:
		_find_player()
		return

	# Skip movement during attack/hurt
	if _state in [State.ATTACKING, State.HURT]:
		velocity = Vector2.ZERO
		move_and_slide()
		return

	var distance_to_player := global_position.distance_to(_player.global_position)
	var direction_to_player := global_position.direction_to(_player.global_position)

	# Attack if in range
	if distance_to_player <= attack_range:
		_attack()
		return

	# Chase if player detected
	if distance_to_player <= detection_range:
		_state = State.CHASING
		velocity = direction_to_player * speed
		_update_sprite(direction_to_player)
	else:
		_state = State.IDLE
		velocity = Vector2.ZERO
		sprite.play(Anim.IDLE)

	move_and_slide()


func _update_sprite(direction: Vector2) -> void:
	update_sprite_direction(direction)

	if _state == State.CHASING:
		sprite.play(Anim.WALK)


func _attack() -> void:
	_state = State.ATTACKING
	velocity = Vector2.ZERO
	sprite.play(Anim.SWING_SWORD)


func _find_player() -> void:
	_player = get_tree().get_first_node_in_group("player")


func _on_died() -> void:
	_event_bus.enemy_defeated.emit(self)


func _on_attack_finished() -> void:
	_try_hit_player()


func _on_death_animation_finished() -> void:
	queue_free()


func _try_hit_player() -> void:
	if not _player:
		return

	var distance := global_position.distance_to(_player.global_position)
	if distance <= attack_range * 1.5:
		if _player.has_method("take_damage"):
			_player.take_damage(damage)
