using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters;

public partial class Character : CharacterBody2D
{
    // Animation names
    public static class Anim
    {
        public const string Idle = "idle";
        public const string Walk = "walk";
        public const string Jump = "jump";
        public const string Fall = "fall";
        public const string SwingSword = "swing_sword";
        public const string PulseRed = "pulse_red";
        public const string Die = "die";
    }

    // State
    public enum State { Idle, Walking, Attacking, Hurt, Dead, Chasing, Jumping, Falling }

    // Stats
    [Export] public float Speed { get; set; } = 100.0f;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float Gravity { get; set; } = 800.0f;

    protected State _state = State.Idle;
    protected int _health;

    // References
    protected AnimatedSprite2D Sprite;
    protected Control HealthBar;
    protected EventBus _eventBus;

    public override void _Ready()
    {
        _health = MaxHealth;
        _eventBus = GetNode<EventBus>("/root/EventBus");
        Sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        HealthBar = GetNodeOrNull<Control>("HealthBar");

        if (Sprite != null)
        {
            Sprite.AnimationFinished += OnAnimationFinished;
        }

        CharacterReady();
    }

    // Override in subclass for additional setup
    protected virtual void CharacterReady() { }

    public void TakeDamage(int amount)
    {
        if (_state == State.Dead) return;

        _health -= amount;
        OnHealthChanged();
        OnDamageTaken(amount);

        if (HealthBar != null)
        {
            HealthBar.Call("update_health", _health, MaxHealth);
        }

        if (_health <= 0)
        {
            Die();
        }
        else
        {
            _state = State.Hurt;
            Sprite?.Play(Anim.PulseRed);
        }
    }

    protected void Die()
    {
        _state = State.Dead;
        Velocity = Vector2.Zero;
        Sprite?.Play(Anim.Die);
        OnDied();
    }

    public void Heal(int amount)
    {
        if (_state == State.Dead) return;

        _health = Mathf.Min(_health + amount, MaxHealth);
        OnHealthChanged();

        if (HealthBar != null)
        {
            HealthBar.Call("update_health", _health, MaxHealth);
        }
    }

    // Override in subclass for custom behavior
    protected virtual void OnHealthChanged() { }

    // Override in subclass for hurt sound effects
    protected virtual void OnDamageTaken(int amount) { }

    // Override in subclass for custom behavior
    protected virtual void OnDied() { }

    private void OnAnimationFinished()
    {
        if (Sprite == null) return;

        switch (Sprite.Animation)
        {
            case Anim.SwingSword:
                OnAttackFinished();
                _state = State.Idle;
                break;
            case Anim.PulseRed:
                _state = State.Idle;
                break;
            case Anim.Die:
                OnDeathAnimationFinished();
                break;
        }
    }

    // Override in subclass for attack hit detection
    protected virtual void OnAttackFinished() { }

    // Override in subclass for death cleanup
    protected virtual void OnDeathAnimationFinished() { }

    protected void UpdateSpriteDirection(Vector2 direction)
    {
        if (Sprite != null && direction.X != 0)
        {
            Sprite.FlipH = direction.X < 0;
        }
    }
}