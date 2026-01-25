using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.characters;

public partial class HasGlobals : CharacterBody2D
{
    protected AudioManager AudioManager;
    protected EventBus EventBus;
    protected GameManager GameManager;
    protected SaveManager SaveManager;

    public override void _Ready()
    {
        AudioManager = GetNode<AudioManager>("/root/AudioManager");
        EventBus = GetNode<EventBus>("/root/EventBus");
        GameManager = GetNode<GameManager>("/root/GameManager");
        SaveManager = GetNode<SaveManager>("/root/SaveManager");
    }
}