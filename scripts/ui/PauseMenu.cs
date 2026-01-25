using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.ui;

public partial class PauseMenu : Control
{
    private const float SaveFeedbackDuration = 1.5f;

    private GameManager _gameManager;
    private SaveManager _saveManager;
    private Label _saveStatus;
    private Button _saveButton;
    private Tween _saveTween;

    public override void _Ready()
    {
        Hide();
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        _saveButton = GetNode<Button>("Panel/VBoxContainer/SaveButton");
        _saveStatus = GetNode<Label>("Panel/VBoxContainer/SaveStatus");
        _saveStatus.Hide();
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
        _gameManager.PauseGame();
        Show();
    }

    private void Resume()
    {
        _gameManager.ResumeGame();
        Hide();
    }

    private void OnResumeButtonPressed()
    {
        Resume();
    }

    private async void OnSaveButtonPressed()
    {
        _saveButton.Disabled = true;
        _saveTween?.Kill();

        // Show saving indicator
        _saveStatus.Text = "Saving...";
        _saveStatus.Modulate = Colors.White;
        _saveStatus.Show();

        // Small delay to show the saving state
        await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);

        // Perform save
        var success = _saveManager.SaveGame();

        // Show result
        _saveStatus.Text = success ? "✓ Saved" : "✗ Failed";

        // Fade out the status
        _saveTween = CreateTween();
        _saveTween.TweenInterval(SaveFeedbackDuration);
        _saveTween.TweenProperty(_saveStatus, "modulate:a", 0.0f, 0.3f);
        _saveTween.TweenCallback(Callable.From(() => _saveStatus.Hide()));

        _saveButton.Disabled = false;
    }

    private void OnExitButtonPressed()
    {
        _gameManager.ReturnToMenu();
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
    }
}