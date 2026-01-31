using Godot;
using Godot.Collections;
using RpgCSharp.scripts.autoload;
using RpgCSharp.scripts.interfaces;

namespace RpgCSharp.scripts.characters;

public static class CommonCharacterAnimation
{
    public const string Idle = "idle";
}

public static class CommonCharacterSaveKey
{
    public const string Id = "id";
    public const string PositionX = "position_x";
    public const string PositionY = "position_y";
}

public abstract partial class Character : CharacterBody2D, IHasGlobals, ISavable
{
    public AnimatedSprite2D Sprite { get; set; }

    public AudioManager AudioManager { get; set; }
    public EventBus EventBus { get; set; }
    public GameManager GameManager { get; set; }
    public SaveManager SaveManager { get; set; }

    public string SaveId
    {
        get
        {
            var tree = GetTree();
            var scenePath = tree.CurrentScene.SceneFilePath;
            var nodePath = GetPathTo(tree.CurrentScene);
            return $"{scenePath}::{nodePath}";
        }
    }

    public override void _Ready()
    {
        AudioManager = GetNode<AudioManager>("/root/AudioManager");
        EventBus = GetNode<EventBus>("/root/EventBus");
        GameManager = GetNode<GameManager>("/root/GameManager");
        SaveManager = GetNode<SaveManager>("/root/SaveManager");
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    public virtual void Save()
    {
        SaveManager.RegisterSavableData(SaveId, GatherSaveData());
    }

    public virtual void Load()
    {
        var pendingData = SaveManager.GetPendingData(SaveId);
        if (pendingData == null) return;

        ApplySaveData(pendingData);
        SaveManager.ClearPendingData(SaveId);
    }

    public virtual Dictionary GatherSaveData()
    {
        return new Dictionary
        {
            { CommonCharacterSaveKey.Id, SaveId },
            { CommonCharacterSaveKey.PositionX, GlobalPosition.X },
            { CommonCharacterSaveKey.PositionY, GlobalPosition.Y }
        };
    }

    public virtual bool ApplySaveData(Dictionary data)
    {
        var posX = (float)data[CommonCharacterSaveKey.PositionX];
        var posY = (float)data[CommonCharacterSaveKey.PositionY];
        GlobalPosition = new Vector2(posX, posY);
        return true;
    }
}