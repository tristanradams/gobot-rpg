class_name Player
extends "res://scripts/characters/character.gd"

# Player-specific stats
@export var attack_damage: int = 25
@export var attack_range: float = 50.0

# Platformer physics
@export var jump_velocity: float = -300.0


func _character_ready() -> void:
	speed = 120.0
	max_health = 100
	_health = max_health


func _physics_process(delta: float) -> void:
	if _state == State.DEAD:
		return

	# Apply gravity
	if not is_on_floor():
		velocity.y += gravity * delta

	# Handle jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_velocity
		_state = State.JUMPING

	# Handle attack input
	if Input.is_action_just_pressed("attack") and _state != State.ATTACKING and is_on_floor():
		_attack()

	# Horizontal movement (only left/right for side-scroller)
	var direction := Input.get_axis("ui_left", "ui_right")

	if _state != State.ATTACKING:
		velocity.x = direction * speed

	move_and_slide()

	_update_sprite(direction)


func _update_sprite(direction: float) -> void:
	if direction != 0:
		sprite.flip_h = direction < 0

	# Airborne states take priority
	if not is_on_floor():
		if velocity.y < 0:
			_state = State.JUMPING
			sprite.play(Anim.JUMP)
		else:
			_state = State.FALLING
			sprite.play(Anim.FALL)
	elif _state == State.ATTACKING:
		return  # Don't interrupt attack animation
	elif direction == 0:
		_state = State.IDLE
		sprite.play(Anim.IDLE)
	else:
		_state = State.WALKING
		sprite.play(Anim.WALK)


func _attack() -> void:
	_state = State.ATTACKING
	sprite.play(Anim.SWING_SWORD)


func _on_health_changed() -> void:
	_event_bus.player_health_changed.emit(_health, max_health)


func _on_died() -> void:
	_event_bus.player_died.emit()


func _on_attack_finished() -> void:
	_try_hit_enemies()


func _try_hit_enemies() -> void:
	var enemies := get_tree().get_nodes_in_group("enemies")

	for enemy in enemies:
		var distance := global_position.distance_to(enemy.global_position)
		if distance <= attack_range:
			if enemy.has_method("take_damage"):
				enemy.take_damage(attack_damage)
