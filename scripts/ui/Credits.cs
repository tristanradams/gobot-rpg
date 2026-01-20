using Godot;

namespace RpgCSharp.scripts.ui;

public partial class Credits : Control
{
    private void _on_back_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/MainMenu.tscn");
    }
}