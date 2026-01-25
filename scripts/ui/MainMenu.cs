using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.ui;

public partial class MainMenu : Control
{
	[Export] public float FadeDuration { get; set; } = 1.5f;
	[Export] public AudioStream BackgroundMusic { get; set; }

	private VBoxContainer _content;
	private Button _loadGameButton;
	private SaveManager _saveManager;

	public override void _Ready()
	{
		_content = GetNode<VBoxContainer>("VBoxContainer");
		_loadGameButton = GetNode<Button>("VBoxContainer/LoadGameButton");
		_saveManager = GetNode<SaveManager>("/root/SaveManager");

		// Disable load button if no save exists
		_loadGameButton.Disabled = !_saveManager.HasSave();

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

	private void OnNewGameButtonPressed()
	{
		_saveManager.ClearAllPendingData();
		GetTree().ChangeSceneToFile("res://scenes/levels/world/main.tscn");
	}

	private void OnLoadGameButtonPressed()
	{
		_saveManager.LoadGame();
	}

	private void OnCreditsButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/ui/menus/Credits.tscn");
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
