using Godot;
using Godot.Collections;
using RpgCSharp.scripts.autoload;
using RpgCSharp.scripts.interfaces;

namespace RpgCSharp.scripts.characters;

public abstract partial class Character : CharacterBody2D, IHasGlobals, ISavable
{
    [Export] public string CharacterName { get; set; }
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

    public virtual void Save()
    {
        SaveManager.RegisterCharacterData(SaveId, GatherSaveData());
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
            { CommonCharacterSaveKeys.Id, SaveId },
            { CommonCharacterSaveKeys.PositionX, GlobalPosition.X },
            { CommonCharacterSaveKeys.PositionY, GlobalPosition.Y }
        };
    }

    public virtual bool ApplySaveData(Dictionary data)
    {
        var posX = (float)data[CommonCharacterSaveKeys.PositionX];
        var posY = (float)data[CommonCharacterSaveKeys.PositionY];
        GlobalPosition = new Vector2(posX, posY);
        return true;
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

    protected static class CommonCharacterAnimation
    {
        public const string Idle = "idle";
        public const string Walk = "walk";
        public const string Run = "run";
    }

    protected static class CommonCharacterSaveKeys
    {
        public const string Id = "id";
        public const string PositionX = "position_x";
        public const string PositionY = "position_y";
    }
}