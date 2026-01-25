using System.Linq;
using Godot;
using RpgCSharp.scripts.autoload;
using RpgCSharp.scripts.characters.player;

namespace RpgCSharp.scripts.characters;

public partial class Character : HasGlobals
{
    // Animation names
    protected static class CommonAnimation
    {
        public const string Idle = "idle";
        public const string Walk = "walk";
        public const string Jump = "jump";
        public const string Attack = "attack";
        public const string Defend = "defend";
        public const string Die = "die";
    }

    // State
    protected enum State
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

    /// <summary>
    /// Attack animations to recognize in OnAnimationFinished. Override in subclass to customize.
    /// </summary>
    protected virtual string[] AttackAnimations => [CommonAnimation.Attack];

    // Stats
    [Export] public float Speed { get; set; } = 100.0f;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float Gravity { get; set; } = 800.0f;

    // Blood effect settings
    [Export] public bool BloodEnabled { get; set; } = true;
    [Export] public Color BloodColor { get; set; } = new Color(0.7f, 0.0f, 0.0f, 1.0f);
    [Export] public int BloodParticleCount { get; set; } = 12;

    // Save system
    [Export] public bool Savable { get; set; } = true;
    [Export] public string SavableIdOverride { get; set; } = "";

    protected State CurrentState = State.Idle;
    protected int CurrentHealth;

    /// <summary>
    /// Unique identifier for save system. Auto-generated from scene path + node path,
    /// or uses SavableIdOverride if set.
    /// </summary>
    public string SavableId
    {
        get
        {
            if (!string.IsNullOrEmpty(SavableIdOverride))
                return SavableIdOverride;

            var scenePath = GetTree().CurrentScene.SceneFilePath;
            var nodePath = GetPathTo(GetTree().CurrentScene);
            return $"{scenePath}::{nodePath}";
        }
    }

    // References
    protected AnimatedSprite2D Sprite;
    protected Control HealthBar;

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = MaxHealth;
        Sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        HealthBar = GetNodeOrNull<Control>("HealthBar");

        if (Sprite != null)
        {
            Sprite.AnimationFinished += OnAnimationFinished;
        }

        // Check pending save data - if dead, don't spawn
        if (Savable)
        {
            var pendingData = SaveManager.GetPendingData(SavableId);
            if (pendingData != null && (bool)pendingData["is_dead"])
            {
                QueueFree();
                return;
            }
        }

        CharacterReady();
    }

    // Override in subclass for additional setup
    protected virtual void CharacterReady()
    {
    }

    public void TakeDamage(int amount)
    {
        if (CurrentState == State.Dead) return;

        CurrentHealth -= amount;
        OnHealthChanged();
        OnDamageTaken(amount);
        SpawnBloodEffect();

        HealthBar?.Call("UpdateHealth", CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            CurrentState = State.Hurt;
        }
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

        // Simple square texture for particles
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

    protected void Die()
    {
        CurrentState = State.Dead;
        Velocity = Vector2.Zero;
        Sprite?.Play(CommonAnimation.Die);

        // Register death with save system
        if (Savable)
        {
            SaveManager.RegisterCharacterData(SavableId, GatherSaveData());
        }

        OnDied();
    }

    public void Heal(int amount)
    {
        if (CurrentState == State.Dead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged();
        HealthBar?.Call("UpdateHealth", CurrentHealth, MaxHealth);
    }

    // Override in subclass for custom behavior
    protected virtual void OnHealthChanged()
    {
    }

    // Override in subclass for hurt sound effects
    protected virtual void OnDamageTaken(int amount)
    {
    }

    // Override in subclass for custom behavior
    protected virtual void OnDied()
    {
    }

    private void OnAnimationFinished()
    {
        if (Sprite == null) return;

        var currentAnim = Sprite.Animation;

        // Check if it's an attack animation
        if (AttackAnimations.Any(attackAnim => currentAnim == attackAnim))
        {
            OnAttackFinished();
            CurrentState = State.Idle;
            return;
        }

        // Handle other animations
        if (currentAnim == CommonAnimation.Die)
        {
            OnDeathAnimationFinished();
        }
    }

    // Override in subclass for attack hit detection
    protected virtual void OnAttackFinished()
    {
    }

    // Override in subclass for death cleanup
    protected virtual void OnDeathAnimationFinished()
    {
    }

    protected void UpdateSpriteDirection(Vector2 direction)
    {
        if (Sprite != null && direction.X != 0)
        {
            Sprite.FlipH = direction.X < 0;
        }
    }

    #region Save System

    /// <summary>
    /// Gathers save data for this character. Override to add custom data.
    /// </summary>
    public virtual Godot.Collections.Dictionary GatherSaveData()
    {
        return new Godot.Collections.Dictionary
        {
            { "id", SavableId },
            { "position_x", GlobalPosition.X },
            { "position_y", GlobalPosition.Y },
            { "health", CurrentHealth },
            { "is_dead", CurrentState == State.Dead }
        };
    }

    /// <summary>
    /// Applies loaded save data to this character. Override to handle custom data.
    /// Returns false if character should be removed (e.g., was dead).
    /// </summary>
    public virtual bool ApplySaveData(Godot.Collections.Dictionary data)
    {
        // If character was dead in save, remove it
        if ((bool)data["is_dead"])
        {
            QueueFree();
            return false;
        }

        // Apply position
        var posX = (float)data["position_x"];
        var posY = (float)data["position_y"];
        GlobalPosition = new Vector2(posX, posY);

        // Apply health
        CurrentHealth = (int)data["health"];
        OnHealthChanged();

        return true;
    }

    #endregion
}