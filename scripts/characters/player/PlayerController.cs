using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.player;

public partial class PlayerController : Character
{
    public static class CustomAnimation
    {
        public const string Run = "run";
        public const string Attack = "attack";
        public const string Attack2 = "attack";
        public const string Attack3 = "attack";
    }

    // Player-specific stats
    [Export] public int AttackDamage { get; set; } = 25;
    [Export] public float AttackRange { get; set; } = 60.0f;

    // Platformer physics
    [Export] public float JumpVelocity { get; set; } = -300.0f;
    [Export] public float CoyoteTime { get; set; } = 0.12f;
    [Export] public float JumpBufferTime { get; set; } = 0.12f;
    [Export] public float MaxDropDistance { get; set; } = 100.0f;

    // Running
    [Export] public float RunSpeed { get; set; } = 200.0f;
    [Export] public float DoubleTapTime { get; set; } = 0.25f;

    // Jump timing state
    private float _coyoteTimer;
    private float _jumpBufferTimer;

    // Double-tap run state
    private float _lastLeftTapTime;
    private float _lastRightTapTime;
    private bool _isRunning;

    // Drop-through state
    private PhysicsBody2D _dropThroughPlatform;
    private float _dropThroughTimer;
    private const float DropThroughDuration = 0.25f;

    // Sound effects
    [Export] public AudioStream AttackSfx { get; set; }
    [Export] public AudioStream HurtSfx { get; set; }

    // Combo system
    [Export] public float ComboResetTime { get; set; } = 0.8f;

    private static readonly string[] ComboAttacks =
        [CommonAnimation.Attack, CustomAnimation.Attack2, CustomAnimation.Attack3];

    protected override string[] AttackAnimations => ComboAttacks;
    private int _comboIndex;
    private float _comboResetTimer;

    protected override void CharacterReady()
    {
        Speed = 120.0f;
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
        GameManager.Player = this;

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
            CurrentState = State.Jumping;
            _coyoteTimer = 0.0f;
            _jumpBufferTimer = 0.0f;
        }

        // Update drop-through timer and restore collision
        UpdateDropThrough((float)delta);

        // Update combo reset timer
        if (_comboResetTimer > 0)
        {
            _comboResetTimer -= (float)delta;
            if (_comboResetTimer <= 0)
            {
                _comboIndex = 0;
            }
        }

        // Handle attack input
        if (Input.IsActionJustPressed("attack") && CurrentState != State.Attacking && IsOnFloor())
        {
            Attack();
        }

        // Horizontal movement (only left/right for side-scroller)
        var direction = Input.GetAxis("ui_left", "ui_right");

        // Double-tap detection for running
        var currentTime = (float)Time.GetTicksMsec() / 1000.0f;

        if (Input.IsActionJustPressed("ui_left"))
        {
            if (currentTime - _lastLeftTapTime < DoubleTapTime)
            {
                _isRunning = true;
            }

            _lastLeftTapTime = currentTime;
        }

        if (Input.IsActionJustPressed("ui_right"))
        {
            if (currentTime - _lastRightTapTime < DoubleTapTime)
            {
                _isRunning = true;
            }

            _lastRightTapTime = currentTime;
        }

        // Stop running if direction released or changed
        if (direction == 0 || (_isRunning && Input.IsActionJustPressed("ui_left") && Velocity.X > 0) ||
            (_isRunning && Input.IsActionJustPressed("ui_right") && Velocity.X < 0))
        {
            _isRunning = false;
        }

        if (CurrentState == State.Attacking)
        {
            velocity.X = 0;
            _isRunning = false;
        }
        else
        {
            var currentSpeed = _isRunning ? RunSpeed : Speed;
            velocity.X = direction * currentSpeed;
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
                CurrentState = State.Jumping;
                Sprite.Play(CommonAnimation.Jump);
            }
            else
            {
                CurrentState = State.Falling;
                Sprite.Play(CommonAnimation.Jump);
            }
        }
        else if (CurrentState == State.Attacking)
        {
            return; // Don't interrupt attack animation
        }
        else if (direction == 0)
        {
            CurrentState = State.Idle;
            Sprite.Play(CommonAnimation.Idle);
        }
        else if (_isRunning)
        {
            CurrentState = State.Running;
            Sprite.Play(CustomAnimation.Run);
        }
        else
        {
            CurrentState = State.Walking;
            Sprite.Play(CommonAnimation.Walk);
        }
    }

    private void Attack()
    {
        CurrentState = State.Attacking;
        Sprite.Play(ComboAttacks[_comboIndex]);
        if (AttackSfx != null) AudioManager.PlaySfx(AttackSfx);
    }

    protected override void OnHealthChanged()
    {
        EventBus.EmitSignal(EventBus.SignalName.PlayerHealthChanged, CurrentHealth, MaxHealth);
    }

    protected override void OnDamageTaken(int amount)
    {
        if (HurtSfx != null) AudioManager.PlaySfx(HurtSfx);
    }

    protected override void OnDied()
    {
        EventBus.EmitSignal(EventBus.SignalName.PlayerDied);
    }

    protected override void OnAttackFinished()
    {
        TryHitEnemies();

        // Advance combo and reset timer
        _comboIndex = (_comboIndex + 1) % ComboAttacks.Length;
        _comboResetTimer = ComboResetTime;
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