using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.enemies;

public abstract partial class Enemy : FightingCharacter
{
    protected bool AppliesGravity = true;
    protected float DistanceToPlayer;
    protected float FacingDirection = 1.0f;
    protected float HorizontalDistance;

    // Updated each physics frame when Player is valid
    protected Vector2 OffsetToPlayer;
    protected Node2D Player;
    protected float VerticalDistance;
    [Export] public float DetectionRange { get; set; } = 150.0f;
    [Export] public float HorizontalChaseThreshold { get; set; } = 15.0f;
    [Export] public float VerticalChaseThreshold { get; set; } = 80.0f;

    protected override void OnFightingCharacterInitialized()
    {
        FindPlayer();
        OnEnemyInitialized();
        Load();
    }

    protected virtual void OnEnemyInitialized()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState == State.Dead) return;

        var velocity = Velocity;

        if (AppliesGravity && !IsOnFloor()) velocity.Y += Gravity * (float)delta;

        if (Player == null)
        {
            FindPlayer();
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        if (CurrentState is State.Attacking or State.Hurt)
        {
            velocity.X = 0;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        // Compute distances once per frame
        OffsetToPlayer = Player.GlobalPosition - GlobalPosition;
        DistanceToPlayer = OffsetToPlayer.Length();
        HorizontalDistance = Mathf.Abs(OffsetToPlayer.X);
        VerticalDistance = Mathf.Abs(OffsetToPlayer.Y);

        if (DistanceToPlayer <= AttackRange && IsOnFloor())
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
            CurrentState = State.Idle;
            velocity.X = 0;
            Sprite.Play(CommonCharacterAnimation.Idle);
        }

        Velocity = velocity;
        MoveAndSlide();
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
            CurrentState = State.Idle;
            velocity.X = 0;
            UpdateSprite(FacingDirection);
            Sprite.Play(CommonCharacterAnimation.Idle);
        }
        else
        {
            CurrentState = State.Chasing;
            velocity.X = FacingDirection * Speed;
            UpdateSprite(FacingDirection);
        }

        return velocity;
    }

    protected virtual void UpdateSprite(float direction)
    {
        if (direction != 0) Sprite.FlipH = direction < 0;

        if (CurrentState == State.Chasing) Sprite.Play(CommonCharacterAnimation.Walk);
    }

    protected void FindPlayer()
    {
        Player = GetTree().GetFirstNodeInGroup("player") as Node2D;
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

    protected virtual void TryHitPlayer()
    {
        if (Player == null) return;
        if (DistanceToPlayer > AttackRange * 1.5f) return;
        if (Player.HasMethod("TakeDamage"))
            Player.Call("TakeDamage", AttackDamage);
    }
}