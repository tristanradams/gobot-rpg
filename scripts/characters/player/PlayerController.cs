using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.player;

public partial class PlayerController : Character
{
	// Player-specific stats
	[Export] public int AttackDamage { get; set; } = 25;
	[Export] public float AttackRange { get; set; } = 50.0f;

	// Platformer physics
	[Export] public float JumpVelocity { get; set; } = -300.0f;
	[Export] public float CoyoteTime { get; set; } = 0.12f;
	[Export] public float JumpBufferTime { get; set; } = 0.12f;
	[Export] public float MaxDropDistance { get; set; } = 100.0f;

	// Jump timing state
	private float _coyoteTimer;
	private float _jumpBufferTimer;

	// Drop-through state
	private PhysicsBody2D _dropThroughPlatform;
	private float _dropThroughTimer;
	private const float DropThroughDuration = 0.25f;

	// Sound effects
	[Export] public AudioStream AttackSfx { get; set; }
	[Export] public AudioStream HurtSfx { get; set; }

	private AudioManager _audioManager;
	private GameManager _gameManager;

	protected override void CharacterReady()
	{
		Speed = 120.0f;
		MaxHealth = 100;
		Health = MaxHealth;
		_audioManager = GetNode<AudioManager>("/root/AudioManager");
		_gameManager = GetNode<GameManager>("/root/GameManager");
		_gameManager.Player = this;

		ApplyPendingSaveData();
	}

	private void ApplyPendingSaveData()
	{
		var pendingData = _saveManager.GetPendingData(SaveableId);
		if (pendingData == null) return;

		ApplySaveData(pendingData);
		_saveManager.ClearPendingData(SaveableId);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_state == State.Dead) return;

		var velocity = Velocity;
		var onFloor = IsOnFloor();

		// Update coyote timer
		if (onFloor)
		{
			_coyoteTimer = CoyoteTime;
		}
		else
		{
			_coyoteTimer -= (float)delta;
		}

		// Update jump buffer timer
		if (Input.IsActionJustPressed("jump"))
		{
			_jumpBufferTimer = JumpBufferTime;
		}
		else
		{
			_jumpBufferTimer -= (float)delta;
		}

		// Apply gravity
		if (!onFloor)
		{
			velocity.Y += Gravity * (float)delta;
		}

		// Handle jump with coyote time and jump buffer
		var canCoyoteJump = _coyoteTimer > 0.0f;
		var hasBufferedJump = _jumpBufferTimer > 0.0f;
		var pressingDown = Input.IsActionPressed("ui_down");

		// Drop-through platform (down + jump while on floor)
		if (hasBufferedJump && onFloor && pressingDown)
		{
			if (TryDropThrough())
			{
				_jumpBufferTimer = 0.0f;
			}
		}
		else if (hasBufferedJump && canCoyoteJump)
		{
			velocity.Y = JumpVelocity;
			_state = State.Jumping;
			_coyoteTimer = 0.0f;
			_jumpBufferTimer = 0.0f;
		}

		// Update drop-through timer and restore collision
		UpdateDropThrough((float)delta);

		// Handle attack input
		if (Input.IsActionJustPressed("attack") && _state != State.Attacking && IsOnFloor())
		{
			Attack();
		}

		// Horizontal movement (only left/right for side-scroller)
		var direction = Input.GetAxis("ui_left", "ui_right");

		if (_state != State.Attacking)
		{
			velocity.X = direction * Speed;
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateSprite(direction);
	}

	private void UpdateSprite(float direction)
	{
		if (direction != 0)
		{
			Sprite.FlipH = direction < 0;
		}

		// Airborne states take priority
		if (!IsOnFloor())
		{
			if (Velocity.Y < 0)
			{
				_state = State.Jumping;
				Sprite.Play(Anim.Jump);
			}
			else
			{
				_state = State.Falling;
				Sprite.Play(Anim.Fall);
			}
		}
		else if (_state == State.Attacking)
		{
			return; // Don't interrupt attack animation
		}
		else if (direction == 0)
		{
			_state = State.Idle;
			Sprite.Play(Anim.Idle);
		}
		else
		{
			_state = State.Walking;
			Sprite.Play(Anim.Walk);
		}
	}

	private void Attack()
	{
		_state = State.Attacking;
		Sprite.Play(Anim.SwingSword);
		if (AttackSfx != null) _audioManager.PlaySfx(AttackSfx);
	}

	protected override void OnHealthChanged()
	{
		_eventBus.EmitSignal(EventBus.SignalName.PlayerHealthChanged, Health, MaxHealth);
	}

	protected override void OnDamageTaken(int amount)
	{
		if (HurtSfx != null) _audioManager.PlaySfx(HurtSfx);
	}

	protected override void OnDied()
	{
		_eventBus.EmitSignal(EventBus.SignalName.PlayerDied);
	}

	protected override void OnAttackFinished()
	{
		TryHitEnemies();
	}

	private void TryHitEnemies()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");

		foreach (var enemy in enemies)
		{
			if (enemy is not Node2D enemyNode) continue;
			var distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
			if (distance > AttackRange) continue;
			if (enemy.HasMethod("TakeDamage"))
			{
				enemy.Call("TakeDamage", AttackDamage);
			}
		}
	}

	private bool TryDropThrough()
	{
		// Get the platform we're standing on
		var floorCollision = GetLastSlideCollision();

		if (floorCollision?.GetCollider() is not PhysicsBody2D platform)
			return false;

		// Check if there's a floor below within MaxDropDistance
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(
			GlobalPosition,
			GlobalPosition + new Vector2(0, MaxDropDistance),
			CollisionMask,
			[GetRid(), platform.GetRid()]
		);
		var result = spaceState.IntersectRay(query);

		if (result.Count == 0)
			return false; // No platform below within range

		// Start drop-through
		_dropThroughPlatform = platform;
		_dropThroughTimer = DropThroughDuration;
		AddCollisionExceptionWith(platform);
		return true;
	}

	private void UpdateDropThrough(float delta)
	{
		if (_dropThroughPlatform == null) return;

		_dropThroughTimer -= delta;

		if (_dropThroughTimer > 0.0f) return;
		RemoveCollisionExceptionWith(_dropThroughPlatform);
		_dropThroughPlatform = null;
	}
}
