using Godot;

namespace RpgCSharp.scripts.characters.enemies;

public abstract partial class WalkingEnemy : Enemy
{
    [Export] public float JumpThreshold { get; set; } = 20.0f;

    protected override Vector2 ProcessChase(Vector2 velocity)
    {
        velocity = base.ProcessChase(velocity);

        if (CurrentState != State.Chasing) return velocity;

        // Jump if player is above (OffsetToPlayer.Y is negative when player is above)
        if (-OffsetToPlayer.Y > JumpThreshold && IsOnFloor())
            velocity.Y = JumpVelocity;

        return velocity;
    }
}
