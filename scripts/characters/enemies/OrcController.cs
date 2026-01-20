using Godot;
using RpgCSharp.scripts.autoload;
using RpgCSharp.scripts.characters;

namespace RpgCSharp.scripts.characters.enemies;

public partial class OrcController : Character
{
    // References
    private Node2D _player;
    private AudioManager _audioManager;

    // Orc-specific stats
    [Export] public int Damage { get; set; } = 10;
    [Export] public float DetectionRange { get; set; } = 150.0f;
    [Export] public float AttackRange { get; set; } = 30.0f;
    [Export] public float JumpVelocity { get; set; } = -280.0f;
    [Export] public float JumpThreshold { get; set; } = 20.0f;

    // Sound effects
    [Export] public AudioStream AttackSfx { get; set; }

    protected override void CharacterReady()
    {
        Speed = 80.0f;
        MaxHealth = 50;
        _health = MaxHealth;
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        FindPlayer();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.Dead) return;

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
        if (_state == State.Attacking || _state == State.Hurt)
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
            _state = State.Chasing;
            velocity.X = directionToPlayer * Speed;
            UpdateSprite(directionToPlayer);

            // Jump if player is above and orc is on floor
            var playerAbove = GlobalPosition.Y - _player.GlobalPosition.Y;
            if (playerAbove > JumpThreshold && IsOnFloor()) velocity.Y = JumpVelocity;
        }
        else
        {
            _state = State.Idle;
            velocity.X = 0;
            Sprite.Play(Anim.Idle);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateSprite(float direction)
    {
        if (direction != 0) Sprite.FlipH = direction < 0;

        if (_state == State.Chasing) Sprite.Play(Anim.Walk);
    }

    private void Attack()
    {
        _state = State.Attacking;
        Velocity = Vector2.Zero;
        Sprite.Play(Anim.SwingSword);
        if (AttackSfx != null) _audioManager.PlaySfx(AttackSfx);
    }

    private void FindPlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
    }

    protected override void OnDied()
    {
        _eventBus.EmitSignal(EventBus.SignalName.EnemyDefeated, this);
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
        if (distance <= AttackRange * 1.5f)
            if (_player.HasMethod("TakeDamage"))
                _player.Call("TakeDamage", Damage);
    }
}