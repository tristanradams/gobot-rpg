using System.Linq;
using Godot;
using Godot.Collections;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters;

public static class CommonFightingCharacterAnimation
{
    public const string Attack = "attack";
    public const string Die = "die";
}

public static class CommonFightingSaveKey
{
    public const string Health = "health";
    public const string IsDead = "is_dead";
}

public enum FightingCharacterState
{
    Idle,
    Walking,
    Running,
    Attacking,
    Hurt,
    Dead,
    Chasing,
    Jumping,
    Falling
}

public abstract partial class FightingCharacter : Character
{
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int CurrentHealth { get; set; } = 100;
    [Export] public float Speed { get; set; } = 100.0f;
    [Export] public float Gravity { get; set; } = 800.0f;
    [Export] public float JumpVelocity { get; set; } = -300.0f;
    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public int AttackRange { get; set; } = 50;

    [Export] public bool BloodEnabled { get; set; } = true;
    [Export] public Color BloodColor { get; set; } = new(0.7f, 0.0f, 0.0f);
    [Export] public int BloodParticleCount { get; set; } = 12;

    public Control HealthBar { get; set; }
    public AudioStream AttackSfx { get; set; }
    public AudioStream HurtSfx { get; set; }

    protected FightingCharacterState CurrentState = FightingCharacterState.Idle;

    protected virtual string[] AttackAnimations { get; set; } = [CommonFightingCharacterAnimation.Attack];

    protected override void OnInitialized()
    {
        CurrentHealth = MaxHealth;
        HealthBar = GetNodeOrNull<Control>("HealthBar");
        AttackSfx = GetNodeOrNull<AudioStream>("AudioStream");
        HurtSfx = GetNodeOrNull<AudioStream>("AudioStream");

        Sprite.AnimationFinished += OnAnimationFinished;

        OnFightingCharacterInitialized();
    }

    protected virtual void OnFightingCharacterInitialized()
    {
    }

    protected virtual void OnHealthChanged()
    {
    }

    protected virtual void OnDamageTaken(int amount)
    {
    }

    protected virtual void OnDied()
    {
    }

    protected virtual void OnAttackFinished()
    {
    }

    protected virtual void OnDeathAnimationFinished()
    {
    }

    public virtual void Attack()
    {
        Velocity = Vector2.Zero;
        CurrentState = FightingCharacterState.Attacking;
        Sprite.Play(AttackAnimations[0]);
        if (AttackSfx != null) AudioManager.PlaySfx(AttackSfx);
    }

    public void TakeDamage(int amount, Node source = null)
    {
        if (CurrentState == FightingCharacterState.Dead) return;

        CurrentHealth -= amount;
        OnHealthChanged();
        OnDamageTaken(amount);
        SpawnBloodEffect();
        if (HurtSfx != null) AudioManager.PlaySfx(HurtSfx);
        HealthBar?.Call("UpdateHealth", CurrentHealth, MaxHealth);

        EventBus.EmitSignal(EventBus.SignalName.DamageDealt, this, amount, source);

        if (CurrentHealth <= 0)
            Die();
        else
            CurrentState = FightingCharacterState.Hurt;
    }

    public void Die()
    {
        CurrentState = FightingCharacterState.Dead;
        Velocity = Vector2.Zero;
        Sprite.Play(CommonFightingCharacterAnimation.Die);
        Save();
        OnDied();
    }

    public void Heal(int amount)
    {
        if (CurrentState == FightingCharacterState.Dead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged();
        HealthBar?.Call("UpdateHealth", CurrentHealth, MaxHealth);
    }

    private void SpawnBloodEffect()
    {
        if (!BloodEnabled)
            return;

        var particles = new GpuParticles2D();
        particles.Emitting = false;
        particles.OneShot = true;
        particles.Explosiveness = 1.0f;
        particles.Amount = BloodParticleCount;

        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 60.0f;
        material.InitialVelocityMin = 80.0f;
        material.InitialVelocityMax = 150.0f;
        material.Gravity = new Vector3(0, 400, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 4.0f;
        material.Color = BloodColor;
        particles.ProcessMaterial = material;

        var img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        img.Fill(Colors.White);
        var texture = ImageTexture.CreateFromImage(img);
        particles.Texture = texture;

        particles.Lifetime = 0.5;
        particles.Finished += particles.QueueFree;

        GetParent().AddChild(particles);
        particles.GlobalPosition = GlobalPosition;
        particles.Emitting = true;
    }

    private void OnAnimationFinished()
    {
        if (Sprite == null) return;

        var currentAnim = Sprite.Animation.ToString();

        if (AttackAnimations.Contains(currentAnim))
        {
            OnAttackFinished();
            CurrentState = FightingCharacterState.Idle;
            return;
        }

        if (currentAnim == CommonFightingCharacterAnimation.Die)
            OnDeathAnimationFinished();
    }

    public override Dictionary GatherSaveData()
    {
        var data = base.GatherSaveData();
        data[CommonFightingSaveKey.Health] = CurrentHealth;
        data[CommonFightingSaveKey.IsDead] = CurrentState == FightingCharacterState.Dead;
        return data;
    }

    public override bool ApplySaveData(Dictionary data)
    {
        if ((bool)data[CommonFightingSaveKey.IsDead])
        {
            QueueFree();
            return false;
        }

        base.ApplySaveData(data);
        CurrentHealth = (int)data[CommonFightingSaveKey.Health];
        OnHealthChanged();
        return true;
    }
}