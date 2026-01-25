using Godot;

namespace RpgCSharp.scripts.ui;

public partial class HealthBar : ProgressBar
{
    [Export] public float FadeDelay { get; set; } = 2.0f;
    [Export] public float FadeDuration { get; set; } = 0.5f;

    private Tween _fadeTween;

    public override void _Ready()
    {
        Modulate = new Color(Modulate, 0.0f);
        FillMode = (int)FillModeEnum.BeginToEnd;
    }

    public void UpdateHealth(int current, int maximum)
    {
        MaxValue = maximum;
        Value = current;
        ShowBar();
    }

    private void ShowBar()
    {
        // Cancel any existing fade
        _fadeTween?.Kill();

        // Show immediately
        Modulate = new Color(Modulate, 1.0f);

        // Start fade after delay
        _fadeTween = CreateTween();
        _fadeTween.TweenInterval(FadeDelay);
        _fadeTween.TweenProperty(this, "modulate:a", 0.0f, FadeDuration);
    }
}