using Godot;

namespace RpgCSharp.scripts.characters.enemies;

public partial class JumpingOrc : JumpingEnemy
{
	protected override void OnEnemyInitialized()
	{
		Speed = 80.0f;
		MaxHealth = 50;
		AttackRange = 30;
		BloodColor = new Color(0.24f, 0.87f, 0.63f); // MediumSeaGreen
		CurrentHealth = MaxHealth;
	}
}
