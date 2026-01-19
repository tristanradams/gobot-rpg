class_name Player
extends "res://scripts/characters/character.gd"

# Player-specific stats
@export var attack_damage: int = 25
@export var attack_range: float = 50.0


func _character_ready() -> void:
	speed = 200.0
	max_health = 100
	_health = max_health


func _physics_process(_delta: float) -> void:
	if _state == State.DEAD:
		return

	# Handle attack input
	if Input.is_action_just_pressed("ui_accept") and _state != State.ATTACKING:
		_attack()
		return

	# Skip movement during attack/hurt
	if _state in [State.ATTACKING, State.HURT]:
		velocity = Vector2.ZERO
		move_and_slide()
		return

	# Movement
	var direction := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	velocity = direction * speed
	move_and_slide()

	_update_sprite(direction)


func _update_sprite(direction: Vector2) -> void:
	update_sprite_direction(direction)

	if direction == Vector2.ZERO:
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
