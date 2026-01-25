using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.enemies;

public partial class OrcController : Character
{
	// References
	private Node2D _player;

	// Orc-specific stats
	[Export] public int Damage { get; set; } = 10;
	[Export] public float DetectionRange { get; set; } = 150.0f;
	[Export] public float AttackRange { get; set; } = 30.0f;
	[Export] public float JumpVelocity { get; set; } = -280.0f;
	[Export] public float JumpThreshold { get; set; } = 20.0f;
	[Export] public float HorizontalChaseThreshold { get; set; } = 15.0f;
	[Export] public float VerticalIgnoreThreshold { get; set; } = 80.0f;

	// Sound effects
	[Export] public AudioStream AttackSfx { get; set; }
	
	private float _facingDirection = 1.0f;
	
	protected override void CharacterReady()
	{
		Speed = 80.0f;
		MaxHealth = 50;
		BloodColor = new Color(0.24f, 0.87f, 0.63f); // MediumSeaGreen
		CurrentHealth = MaxHealth;
		FindPlayer();
		ApplyPendingSaveData();
	}

	private void ApplyPendingSaveData()
	{
		var pendingData = SaveManager.GetPendingData(SavableId);
		if (pendingData == null) return;

		ApplySaveData(pendingData);
		SaveManager.ClearPendingData(SavableId);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CurrentState == State.Dead) return;

		var velocity = Velocity;

		// Apply gravity
		if (!IsOnFloor()) velocity.Y += Gravity * (float)delta;

		if (_player == null)
		{
			FindPlayer();
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// Skip horizontal movement during attack/hurt
		if (CurrentState == State.Attacking || CurrentState == State.Hurt)
		{
			velocity.X = 0;
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		var distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);
		float directionToPlayer = Mathf.Sign(_player.GlobalPosition.X - GlobalPosition.X);

		// Attack if in range
		if (distanceToPlayer <= AttackRange && IsOnFloor())
		{
			Attack();
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// Chase if player detected (horizontal only for side-scroller)
		if (distanceToPlayer <= DetectionRange)
		{
			var horizontalDistance = Mathf.Abs(_player.GlobalPosition.X - GlobalPosition.X);
			var verticalDistance = Mathf.Abs(_player.GlobalPosition.Y - GlobalPosition.Y);
			var playerAbove = GlobalPosition.Y - _player.GlobalPosition.Y;

			// Only update facing direction if horizontal distance is significant
			if (horizontalDistance > HorizontalChaseThreshold)
			{
				_facingDirection = directionToPlayer;
			}

			// Stop chasing if player is unreachable (too far above/below)
			var playerUnreachable = verticalDistance > VerticalIgnoreThreshold && horizontalDistance < HorizontalChaseThreshold;

			if (playerUnreachable)
			{
				CurrentState = State.Idle;
				velocity.X = 0;
				UpdateSprite(_facingDirection);
				Sprite.Play(CommonAnimation.Idle);
			}
			else
			{
				CurrentState = State.Chasing;
				velocity.X = _facingDirection * Speed;
				UpdateSprite(_facingDirection);

				// Jump if player is above and orc is on floor
				if (playerAbove > JumpThreshold && IsOnFloor())
				{
					velocity.Y = JumpVelocity;
				}
			}
		}
		else
		{
			CurrentState = State.Idle;
			velocity.X = 0;
			Sprite.Play(CommonAnimation.Idle);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void UpdateSprite(float direction)
	{
		if (direction != 0) Sprite.FlipH = direction < 0;

		if (CurrentState == State.Chasing) Sprite.Play(CommonAnimation.Walk);
	}

	private void Attack()
	{
		CurrentState = State.Attacking;
		Velocity = Vector2.Zero;
		Sprite.Play(CommonAnimation.Attack);
		if (AttackSfx != null) AudioManager.PlaySfx(AttackSfx);
	}

	private void FindPlayer()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	protected override void OnDied()
	{
		EventBus.EmitSignal(EventBus.SignalName.EnemyDefeated, this);
	}

	protected override void OnAttackFinished()
	{
		TryHitPlayer();
	}

	protected override void OnDeathAnimationFinished()
	{
		QueueFree();
	}

	private void TryHitPlayer()
	{
		if (_player == null) return;

		var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (distance > AttackRange * 1.5f) return;
		if (_player.HasMethod("TakeDamage"))
			_player.Call("TakeDamage", Damage);
	}
}
