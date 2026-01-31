using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.enemies;

public abstract partial class Enemy : FightingCharacter
{
    [Export] public bool CanAttack { get; set; } = true;
    [Export] public bool AppliesGravity { get; set; } = true;
    [Export] public float ContactDamageCooldown { get; set; } = 1.0f;
    [Export] public float ContactDamageRange { get; set; }
    [Export] public float DetectionRange { get; set; } = 150.0f;
    [Export] public float HorizontalChaseThreshold { get; set; } = 15.0f;
    [Export] public float VerticalChaseThreshold { get; set; } = 80.0f;

    protected float FacingDirection = 1.0f;

    // Updated each physics frame when Player is valid
    public Vector2 OffsetToPlayer;
    public float DistanceToPlayer;
    public float HorizontalDistance;
    public float VerticalDistance;

    private float _contactDamageTimer;
    private float _hurtTimer;
    private const float HurtDuration = 0.2f;

    protected override void OnFightingCharacterInitialized()
    {
        OnEnemyInitialized();
        InitializeContactDamageRange();
        Load();
    }

    protected virtual void OnEnemyInitialized()
    {
    }

    protected virtual void ProcessContactDamage(double delta)
    {
        if (_contactDamageTimer > 0)
        {
            _contactDamageTimer -= (float)delta;
            return;
        }

        if (DistanceToPlayer > ContactDamageRange) return;

        GameManager.Player.TakeDamage(AttackDamage, this);
        _contactDamageTimer = ContactDamageCooldown;
    }

    protected virtual Vector2 ProcessChase(Vector2 velocity)
    {
        float directionToPlayer = Mathf.Sign(OffsetToPlayer.X);

        if (HorizontalDistance > HorizontalChaseThreshold)
            FacingDirection = directionToPlayer;

        var playerUnreachable = VerticalDistance > VerticalChaseThreshold &&
                                HorizontalDistance < HorizontalChaseThreshold;

        if (playerUnreachable)
        {
            CurrentState = FightingCharacterState.Idle;
            velocity.X = 0;
            UpdateSprite(FacingDirection);
            Sprite.Play(CommonCharacterAnimation.Idle);
        }
        else
        {
            CurrentState = FightingCharacterState.Chasing;
            velocity.X = FacingDirection * Speed;
            UpdateSprite(FacingDirection);
        }

        return velocity;
    }

    protected virtual void TryHitPlayer()
    {
        if (GameManager.Player == null) return;
        if (DistanceToPlayer > AttackRange * 1.5f) return;
        GameManager.Player.TakeDamage(AttackDamage, this);
    }

    protected virtual void UpdateSprite(float direction)
    {
        if (direction != 0) Sprite.FlipH = direction < 0;

        if (CurrentState == FightingCharacterState.Chasing)
            Sprite.Play(AppliesGravity ? WalkingEnemyAnimation.Walk : FlyingEnemyAnimation.Fly);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState == FightingCharacterState.Dead) return;

        var velocity = Velocity;

        if (AppliesGravity && !IsOnFloor()) velocity.Y += Gravity * (float)delta;

        // Compute distances once per frame (before any early returns)
        OffsetToPlayer = GameManager.Player.GlobalPosition - GlobalPosition;
        DistanceToPlayer = OffsetToPlayer.Length();
        HorizontalDistance = Mathf.Abs(OffsetToPlayer.X);
        VerticalDistance = Mathf.Abs(OffsetToPlayer.Y);

        // Handle hurt state with timer
        if (CurrentState == FightingCharacterState.Hurt)
        {
            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
                CurrentState = FightingCharacterState.Idle;
        }

        if (CurrentState is FightingCharacterState.Attacking or FightingCharacterState.Hurt)
        {
            velocity.X = 0;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        if (CanAttack && DistanceToPlayer <= AttackRange && IsOnFloor())
        {
            Attack();
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        if (DistanceToPlayer <= DetectionRange)
        {
            velocity = ProcessChase(velocity);
        }
        else
        {
            CurrentState = FightingCharacterState.Idle;
            velocity.X = 0;
            Sprite.Play(CommonCharacterAnimation.Idle);
        }

        Velocity = velocity;
        MoveAndSlide();

        if (!CanAttack)
            ProcessContactDamage(delta);
    }
    
    protected override void OnDamageTaken(int amount)
    {
        _hurtTimer = HurtDuration;
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


    private void InitializeContactDamageRange()
    {
        if (ContactDamageRange > 0) return;

        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        ContactDamageRange = collisionShape?.Shape switch
        {
            RectangleShape2D rect => Mathf.Max(rect.Size.X, rect.Size.Y) / 2 + 5,
            CircleShape2D circle => circle.Radius + 5,
            _ => 15.0f
        };
    }
}