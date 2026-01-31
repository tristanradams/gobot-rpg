using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.player;

public static class PlayerAnimation
{
    public const string Walk = "walk";
    public const string Run = "run";
    public const string Crouch = "crouch";
    public const string CrouchWalk = "crouch_walk";
    public const string Jump = "jump";
    public const string Punch = "punch";
    public const string PunchCross = "punch_cross";
    public const string PunchJab = "punch_jab";
}

public partial class Player : FightingCharacter
{
    // Drop-through
    public float MaxDropDistance { get; set; }
    private float DropThroughDuration { get; set; }
    private PhysicsBody2D _dropThroughPlatform;

    // attack combo timer
    public float ComboResetTime { get; set; }

    protected override string[] AttackAnimations { get; set; } =
        [PlayerAnimation.PunchCross, PlayerAnimation.PunchJab, PlayerAnimation.Punch];

    private int _comboIndex;
    private float _comboResetTimer;

    // Jump timing
    public float CoyoteTime { get; set; }
    public float JumpBufferTime { get; set; }
    private float _coyoteTimer;
    private float _dropThroughTimer;
    private float _jumpBufferTimer;

    // running
    public float RunSpeed { get; set; }
    public float DoubleTapTime { get; set; }
    private bool _isRunning;
    private float _lastLeftTapTime;
    private float _lastRightTapTime;

    // crouching
    public float CrouchSpeed { get; set; }
    private bool _isCrouching;

    protected override void OnFightingCharacterInitialized()
    {
        Speed = 120.0f;
        MaxHealth = 100;
        AttackDamage = 25;
        AttackRange = 60;
        CurrentHealth = MaxHealth;
        MaxDropDistance = 100.0f;
        DropThroughDuration = 0.25f;
        ComboResetTime = 3.5f;
        CoyoteTime = 0.12f;
        JumpBufferTime = 0.12f;
        RunSpeed = 200.0f;
        DoubleTapTime = 0.25f;
        CrouchSpeed = 50.0f;
        GameManager.Player = this;
        Load();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState == FightingCharacterState.Dead) return;

        var velocity = Velocity;
        var onFloor = IsOnFloor();

        // Update coyote timer
        if (onFloor)
            _coyoteTimer = CoyoteTime;
        else
            _coyoteTimer -= (float)delta;

        // Handle crouching (only on floor)
        _isCrouching = Input.IsActionPressed("ui_down") && onFloor;

        // Update jump buffer timer
        if (Input.IsActionJustPressed("jump"))
            _jumpBufferTimer = JumpBufferTime;
        else
            _jumpBufferTimer -= (float)delta;

        // Apply gravity
        if (!onFloor) velocity.Y += Gravity * (float)delta;

        // Handle jump with coyote time and jump buffer
        var canCoyoteJump = _coyoteTimer > 0.0f;
        var hasBufferedJump = _jumpBufferTimer > 0.0f;
        var pressingDown = Input.IsActionPressed("ui_down");

        // Drop-through platform (down + jump while on floor)
        if (hasBufferedJump && onFloor && pressingDown)
        {
            if (TryDropThrough()) _jumpBufferTimer = 0.0f;
        }
        else if (hasBufferedJump && canCoyoteJump && !_isCrouching)
        {
            velocity.Y = JumpVelocity;
            CurrentState = FightingCharacterState.Jumping;
            _coyoteTimer = 0.0f;
            _jumpBufferTimer = 0.0f;
        }

        // Update drop-through timer and restore collision
        UpdateDropThrough((float)delta);

        // Update combo reset timer
        if (_comboResetTimer > 0)
        {
            _comboResetTimer -= (float)delta;
            if (_comboResetTimer <= 0) _comboIndex = 0;
        }

        // Handle attack input
        if (
            Input.IsActionJustPressed("attack") &&
            CurrentState != FightingCharacterState.Attacking &&
            IsOnFloor()
        )
        {
            Attack();
        }

        // Horizontal movement (only left/right for side-scroller)
        var direction = Input.GetAxis("ui_left", "ui_right");

        // Double-tap detection for running
        var currentTime = Time.GetTicksMsec() / 1000.0f;

        if (Input.IsActionJustPressed("ui_left"))
        {
            if (currentTime - _lastLeftTapTime < DoubleTapTime) _isRunning = true;

            _lastLeftTapTime = currentTime;
        }

        if (Input.IsActionJustPressed("ui_right"))
        {
            if (currentTime - _lastRightTapTime < DoubleTapTime) _isRunning = true;

            _lastRightTapTime = currentTime;
        }

        // Stop running if direction released, changed, or crouching
        if (direction == 0 || _isCrouching ||
            (_isRunning && Input.IsActionJustPressed("ui_left") && Velocity.X > 0) ||
            (_isRunning && Input.IsActionJustPressed("ui_right") && Velocity.X < 0))
            _isRunning = false;

        if (CurrentState == FightingCharacterState.Attacking)
        {
            velocity.X = 0;
            _isRunning = false;
        }
        else
        {
            var currentSpeed = _isCrouching ? CrouchSpeed : (_isRunning ? RunSpeed : Speed);
            velocity.X = direction * currentSpeed;
        }

        Velocity = velocity;
        MoveAndSlide();

        UpdateSprite(direction);
    }

    private void UpdateSprite(float direction)
    {
        if (direction != 0) Sprite.FlipH = direction < 0;

        // Airborne states take priority
        if (!IsOnFloor())
        {
            if (Velocity.Y < 0)
            {
                CurrentState = FightingCharacterState.Jumping;
                Sprite.Play(PlayerAnimation.Jump);
            }
            else
            {
                CurrentState = FightingCharacterState.Falling;
                Sprite.Play(PlayerAnimation.Jump);
            }
        }
        else if (CurrentState == FightingCharacterState.Attacking)
        {
        }
        else if (_isCrouching)
        {
            CurrentState = direction != 0 ? FightingCharacterState.Walking : FightingCharacterState.Idle;
            Sprite.Play(direction != 0 ? PlayerAnimation.CrouchWalk : PlayerAnimation.Crouch);
        }
        else if (direction != 0)
        {
            CurrentState = _isRunning ? FightingCharacterState.Running : FightingCharacterState.Walking;
            Sprite.Play(_isRunning ? PlayerAnimation.Run : PlayerAnimation.Walk);
        }
        else
        {
            CurrentState = FightingCharacterState.Idle;
            Sprite.Play(CommonCharacterAnimation.Idle);
        }
    }

    public override void Attack()
    {
        Velocity = Vector2.Zero;
        CurrentState = FightingCharacterState.Attacking;
        Sprite.Play(AttackAnimations[_comboIndex]);
        if (AttackSfx != null) AudioManager.PlaySfx(AttackSfx);
    }

    protected override void OnHealthChanged()
    {
    }

    protected override void OnDied()
    {
        EventBus.EmitSignal(EventBus.SignalName.PlayerDied);
    }

    protected override void OnAttackFinished()
    {
        TryHitEnemies();

        // Advance combo and reset timer
        _comboIndex = (_comboIndex + 1) % AttackAnimations.Length;
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
            if (enemy.HasMethod("TakeDamage")) enemy.Call("TakeDamage", AttackDamage, this);
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