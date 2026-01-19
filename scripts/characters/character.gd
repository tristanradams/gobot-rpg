class_name Character
extends CharacterBody2D

# Animation names
const Anim = {
	IDLE = "idle",
	WALK = "walk",
	SWING_SWORD = "swing_sword",
	PULSE_RED = "pulse_red",
	DIE = "die",
}

# Stats
@export var speed: float = 100.0
@export var max_health: int = 100

# State
enum State { IDLE, WALKING, ATTACKING, HURT, DEAD, CHASING }
var _state: State = State.IDLE
var _health: int = max_health

# References
var sprite: AnimatedSprite2D
var health_bar: Control
var _event_bus: Node


func _ready() -> void:
	_health = max_health
	_event_bus = get_node("/root/EventBus")
	sprite = get_node_or_null("AnimatedSprite2D")
	health_bar = get_node_or_null("HealthBar")
	
	if sprite:
		sprite.animation_finished.connect(_on_animation_finished)
	
	_character_ready()


# Override in subclass for additional setup
func _character_ready() -> void:
	pass


func take_damage(amount: int) -> void:
	if _state == State.DEAD:
		return

	_health -= amount
	_on_health_changed()
	
	if health_bar:
		health_bar.update_health(_health, max_health)

	if _health <= 0:
		_die()
	else:
		_state = State.HURT
		if sprite:
			sprite.play(Anim.PULSE_RED)


func _die() -> void:
	_state = State.DEAD
	velocity = Vector2.ZERO
	if sprite:
		sprite.play(Anim.DIE)
	_on_died()


func heal(amount: int) -> void:
	if _state == State.DEAD:
		return

	_health = min(_health + amount, max_health)
	_on_health_changed()
	if health_bar:
		health_bar.update_health(_health, max_health)


# Override in subclass for custom behavior
func _on_health_changed() -> void:
	pass


# Override in subclass for custom behavior
func _on_died() -> void:
	pass


func _on_animation_finished() -> void:
	if not sprite:
		return

	match sprite.animation:
		Anim.SWING_SWORD:
			_on_attack_finished()
			_state = State.IDLE
		Anim.PULSE_RED:
			_state = State.IDLE
		Anim.DIE:
			_on_death_animation_finished()


# Override in subclass for attack hit detection
func _on_attack_finished() -> void:
	pass


# Override in subclass for death cleanup
func _on_death_animation_finished() -> void:
	pass


func update_sprite_direction(direction: Vector2) -> void:
	if sprite and direction.x != 0:
		sprite.flip_h = direction.x < 0
