using Godot;

namespace RpgCSharp.scripts.ui;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            if (Visible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
            GetViewport().SetInputAsHandled();
        }
    }

    private void Pause()
    {
        GetTree().Paused = true;
        Show();
    }

    private void Resume()
    {
        GetTree().Paused = false;
        Hide();
    }

    private void _on_resume_button_pressed()
    {
        Resume();
    }

    private void _on_exit_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
    }
}