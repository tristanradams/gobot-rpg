using Godot;

namespace RpgCSharp.scripts.characters.enemies;

public static class FlyingEnemyAnimation
{
    public const string Fly = "fly";
}

public abstract partial class FlyingEnemy : Enemy
{
    [Export] public float VerticalSpeed { get; set; } = 80.0f;

    protected override void OnEnemyInitialized()
    {
        AppliesGravity = false;
    }

    protected override Vector2 ProcessChase(Vector2 velocity)
    {
        float directionToPlayer = Mathf.Sign(OffsetToPlayer.X);

        if (HorizontalDistance > HorizontalChaseThreshold)
            FacingDirection = directionToPlayer;

        CurrentState = State.Chasing;

        // Move directly toward player (both horizontal and vertical)
        velocity.X = FacingDirection * Speed;
        velocity.Y = Mathf.Sign(OffsetToPlayer.Y) * VerticalSpeed;

        UpdateSprite(FacingDirection);

        return velocity;
    }
}