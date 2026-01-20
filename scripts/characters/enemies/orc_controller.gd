class_name Orc
extends "res://scripts/characters/character.gd"

# Orc-specific stats
@export var damage: int = 10
@export var detection_range: float = 150.0
@export var attack_range: float = 30.0
@export var jump_velocity: float = -280.0
@export var jump_threshold: float = 20.0  # How far above player must be to trigger jump

# References
var _player: Node2D = null


func _character_ready() -> void:
	speed = 80.0
	max_health = 50
	_health = max_health
	_find_player()


func _physics_process(delta: float) -> void:
	if _state == State.DEAD:
		return

	# Apply gravity
	if not is_on_floor():
		velocity.y += gravity * delta

	if not _player:
		_find_player()
		move_and_slide()
		return

	# Skip horizontal movement during attack/hurt
	if _state in [State.ATTACKING, State.HURT]:
		velocity.x = 0
		move_and_slide()
		return

	var distance_to_player: float = global_position.distance_to(_player.global_position)
	var direction_to_player: float = sign(_player.global_position.x - global_position.x)

	# Attack if in range
	if distance_to_player <= attack_range and is_on_floor():
		_attack()
		move_and_slide()
		return

	# Chase if player detected (horizontal only for side-scroller)
	if distance_to_player <= detection_range:
		_state = State.CHASING
		velocity.x = direction_to_player * speed
		_update_sprite(direction_to_player)

		# Jump if player is above and orc is on floor
		var player_above: float = global_position.y - _player.global_position.y
		if player_above > jump_threshold and is_on_floor():
			velocity.y = jump_velocity
	else:
		_state = State.IDLE
		velocity.x = 0
		sprite.play(Anim.IDLE)

	move_and_slide()


func _update_sprite(direction: float) -> void:
	if direction != 0:
		sprite.flip_h = direction < 0

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
