using Godot;

namespace RpgCSharp.scripts.characters.enemies;

public partial class BlueSnail : WalkingEnemy
{
	protected override void OnEnemyInitialized()
	{
		CanAttack = false;
		Speed = 40.0f;
		MaxHealth = 30;
		AttackDamage = 10;
		BloodColor = new Color(0.27f, 0.51f, 0.71f); // SteelBlue
		CurrentHealth = MaxHealth;
	}
}
