class_name HealthBar
extends ProgressBar


@export var fade_delay: float = 2.0
@export var fade_duration: float = 0.5

var _fade_tween: Tween = null


func _ready() -> void:
	modulate.a = 0.0
	fill_mode = ProgressBar.FILL_BEGIN_TO_END


func update_health(current: int, maximum: int) -> void:
	max_value = maximum
	value = current
	show_bar()


func show_bar() -> void:
	# Cancel any existing fade
	if _fade_tween:
		_fade_tween.kill()

	# Show immediately
	modulate.a = 1.0

	# Start fade after delay
	_fade_tween = create_tween()
	_fade_tween.tween_interval(fade_delay)
	_fade_tween.tween_property(self, "modulate:a", 0.0, fade_duration)
