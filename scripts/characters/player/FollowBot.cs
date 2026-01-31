using Godot;

namespace RpgCSharp.scripts.characters.player;

public partial class FollowBot : Character
{
    [Export] public int AttackDamage { get; set; } = 100;
    [Export] public float Speed { get; set; } = 200.0f;
    [Export] public float FollowDistance { get; set; } = 35.0f;
    [Export] public float Smoothing { get; set; } = 7.5f;
    [Export] public float HoverOffset { get; set; } = -10.0f;

    private bool _isFollowing;

    protected override void OnInitialized()
    {
        Sprite.Play(CommonCharacterAnimation.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        var targetPosition = GameManager.Player.GlobalPosition + new Vector2(0, HoverOffset);
        var offsetToPlayer = targetPosition - GlobalPosition;
        var distanceToPlayer = offsetToPlayer.Length();

        if (_isFollowing && distanceToPlayer <= FollowDistance)
            _isFollowing = false;
        else if (!_isFollowing && distanceToPlayer > FollowDistance)
            _isFollowing = true;

        Vector2 targetVelocity;
        if (_isFollowing)
        {
            var direction = offsetToPlayer.Normalized();
            targetVelocity = direction * Speed;
            Sprite.FlipH = direction.X < 0;
        }
        else
        {
            targetVelocity = Vector2.Zero;
        }

        // Smoothly interpolate velocity
        Velocity = Velocity.Lerp(targetVelocity, Smoothing * (float)delta);

        MoveAndSlide();
    }
}