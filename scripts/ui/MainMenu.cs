using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.ui;

public partial class MainMenu : Control
{
	[Export] public float FadeDuration { get; set; } = 2.0f;
	[Export] public AudioStream BackgroundMusic { get; set; }

	private VBoxContainer _content;

	public override void _Ready()
	{
		_content = GetNode<VBoxContainer>("VBoxContainer");

		// Start content fully transparent (background stays visible)
		_content.Modulate = new Color(_content.Modulate, 0.0f);

		// Fade in content only
		var tween = CreateTween();
		tween.TweenProperty(_content, "modulate:a", 1.0f, FadeDuration);

		// Play background music
		if (BackgroundMusic != null)
		{
			var audioManager = GetNode<AudioManager>("/root/AudioManager");
			audioManager.PlayMusic(BackgroundMusic);
		}
	}

	private void _on_new_game_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/levels/world/main.tscn");
	}

	private void _on_load_game_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/levels/world/main.tscn");
	}

	private void _on_credits_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/ui/menus/Credits.tscn");
	}

	private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}
}
