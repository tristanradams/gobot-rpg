using Godot;

namespace RpgCSharp.scripts.ui;

public partial class Credits : Control
{
    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
    }
}