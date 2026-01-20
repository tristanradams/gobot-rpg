using Godot;
using RpgCSharp.scripts.characters;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters.player;

public partial class PlayerController : Character
{
    // Player-specific stats
    [Export] public int AttackDamage { get; set; } = 25;
    [Export] public float AttackRange { get; set; } = 50.0f;

    // Platformer physics
    [Export] public float JumpVelocity { get; set; } = -300.0f;

    // Sound effects
    [Export] public AudioStream AttackSfx { get; set; }
    [Export] public AudioStream HurtSfx { get; set; }

    private AudioManager _audioManager;

    protected override void CharacterReady()
    {
        Speed = 120.0f;
        MaxHealth = 100;
        _health = MaxHealth;
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.Dead) return;

        var velocity = Velocity;

        // Apply gravity
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }

        // Handle jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            _state = State.Jumping;
        }

        // Handle attack input
        if (Input.IsActionJustPressed("attack") && _state != State.Attacking && IsOnFloor())
        {
            Attack();
        }

        // Horizontal movement (only left/right for side-scroller)
        float direction = Input.GetAxis("ui_left", "ui_right");

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
        _eventBus.EmitSignal(EventBus.SignalName.PlayerHealthChanged, _health, MaxHealth);
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
            if (enemy is Node2D enemyNode)
            {
                float distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                if (distance <= AttackRange)
                {
                    if (enemy.HasMethod("TakeDamage"))
                    {
                        enemy.Call("TakeDamage", AttackDamage);
                    }
                }
            }
        }
    }
}