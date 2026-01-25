using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.ui;

public partial class DeathMenu : Control
{
	private EventBus _eventBus;
	private GameManager _gameManager;

	public override void _Ready()
	{
		Hide();
		_eventBus = GetNode<EventBus>("/root/EventBus");
		_gameManager = GetNode<GameManager>("/root/GameManager");
		_eventBus.PlayerDied += OnPlayerDied;
	}

	private void OnPlayerDied()
	{
		_gameManager.PauseGame();
		Show();
	}

	private void OnMainMenuButtonPressed()
	{
		_gameManager.ReturnToMenu();
		GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
	}
}
