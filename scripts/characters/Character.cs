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

	// Blood effect settings
	[Export] public bool BloodEnabled { get; set; } = true;
	[Export] public Color BloodColor { get; set; } = new Color(0.7f, 0.0f, 0.0f, 1.0f);
	[Export] public int BloodParticleCount { get; set; } = 12;

	// Save system
	[Export] public bool Saveable { get; set; } = true;
	[Export] public string SaveableIdOverride { get; set; } = "";

	protected State _state = State.Idle;
	protected int Health;

	public bool IsDead => _state == State.Dead;

	/// <summary>
	/// Unique identifier for save system. Auto-generated from scene path + node path,
	/// or uses SaveableIdOverride if set.
	/// </summary>
	public string SaveableId
	{
		get
		{
			if (!string.IsNullOrEmpty(SaveableIdOverride))
				return SaveableIdOverride;

			var scenePath = GetTree().CurrentScene.SceneFilePath;
			var nodePath = GetPathTo(GetTree().CurrentScene);
			return $"{scenePath}::{nodePath}";
		}
	}

	// References
	protected AnimatedSprite2D Sprite;
	protected Control HealthBar;
	protected EventBus _eventBus;
	protected SaveManager _saveManager;

	public override void _Ready()
	{
		Health = MaxHealth;
		_eventBus = GetNode<EventBus>("/root/EventBus");
		_saveManager = GetNode<SaveManager>("/root/SaveManager");
		Sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		HealthBar = GetNodeOrNull<Control>("HealthBar");

		if (Sprite != null)
		{
			Sprite.AnimationFinished += OnAnimationFinished;
		}

		// Check pending save data - if dead, don't spawn
		if (Saveable)
		{
			var pendingData = _saveManager.GetPendingData(SaveableId);
			if (pendingData != null && (bool)pendingData["is_dead"])
			{
				QueueFree();
				return;
			}
		}

		CharacterReady();
	}

	// Override in subclass for additional setup
	protected virtual void CharacterReady() { }

	public void TakeDamage(int amount)
	{
		if (_state == State.Dead) return;

		Health -= amount;
		OnHealthChanged();
		OnDamageTaken(amount);
		SpawnBloodEffect();

		if (HealthBar != null)
		{
			HealthBar.Call("UpdateHealth", Health, MaxHealth);
		}

		if (Health <= 0)
		{
			Die();
		}
		else
		{
			_state = State.Hurt;
			Sprite?.Play(Anim.PulseRed);
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
		_state = State.Dead;
		Velocity = Vector2.Zero;
		Sprite?.Play(Anim.Die);

		// Register death with save system
		if (Saveable)
		{
			_saveManager.RegisterCharacterData(SaveableId, GatherSaveData());
		}

		OnDied();
	}

	public void Heal(int amount)
	{
		if (_state == State.Dead) return;

		Health = Mathf.Min(Health + amount, MaxHealth);
		OnHealthChanged();

		if (HealthBar != null)
		{
			HealthBar.Call("UpdateHealth", Health, MaxHealth);
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

	#region Save System

	/// <summary>
	/// Gathers save data for this character. Override to add custom data.
	/// </summary>
	public virtual Godot.Collections.Dictionary GatherSaveData()
	{
		return new Godot.Collections.Dictionary
		{
			{ "id", SaveableId },
			{ "position_x", GlobalPosition.X },
			{ "position_y", GlobalPosition.Y },
			{ "health", Health },
			{ "is_dead", IsDead }
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
		Health = (int)data["health"];
		OnHealthChanged();

		return true;
	}

	#endregion
}
