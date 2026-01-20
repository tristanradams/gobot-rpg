using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.ui;

public partial class DeathMenu : Control
{
    private EventBus _eventBus;

    public override void _Ready()
    {
        Hide();
        _eventBus = GetNode<EventBus>("/root/EventBus");
        _eventBus.PlayerDied += OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        GetTree().Paused = true;
        Show();
    }

    private void _on_main_menu_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
    }
}