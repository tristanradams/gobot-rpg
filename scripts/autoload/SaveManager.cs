using Godot;
using Godot.Collections;
using RpgCSharp.scripts.characters;

namespace RpgCSharp.scripts.autoload;

public partial class SaveManager : Node
{
    private const string SaveDir = "user://saves/";
    private const string SaveExtension = ".save";
    private const string SettingsPath = "user://settings.cfg";

    private GameManager _gameManager;

    // Pending data to apply when scene loads, keyed by SaveId
    private Dictionary<string, Dictionary> _pendingCharacterData = new();

    public override void _Ready()
    {
        EnsureSaveDirectory();
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }

    private static void EnsureSaveDirectory()
    {
        DirAccess.MakeDirRecursiveAbsolute(SaveDir);
    }

    public bool SaveGame(int slot = 0)
    {
        var saveData = GatherSaveData();
        if (saveData == null)
        {
            GD.PushError("Failed to gather save data");
            return false;
        }

        var path = GetSavePath(slot);

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError($"Failed to open save file: {path}");
            return false;
        }

        file.StoreVar(saveData);
        GD.Print($"Game saved to slot {slot}");
        return true;
    }

    public bool LoadGame(int slot = 0)
    {
        var path = GetSavePath(slot);

        if (!FileAccess.FileExists(path))
        {
            GD.PushError($"Save file not found: {path}");
            return false;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError($"Failed to open save file: {path}");
            return false;
        }

        var saveData = (Dictionary)file.GetVar();
        ApplySaveData(saveData);
        GD.Print($"Game loaded from slot {slot}");
        return true;
    }

    public bool DeleteSave(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (FileAccess.FileExists(path)) return DirAccess.RemoveAbsolute(path) == Error.Ok;
        return false;
    }

    public bool HasSave(int slot = 0)
    {
        return FileAccess.FileExists(GetSavePath(slot));
    }

    private static string GetSavePath(int slot)
    {
        return $"{SaveDir}slot_{slot}{SaveExtension}";
    }

    /// <summary>
    ///     Gets pending save data for a character by its SaveId.
    ///     Returns null if no pending data exists.
    /// </summary>
    public Dictionary GetPendingData(string saveId)
    {
        return _pendingCharacterData.TryGetValue(saveId, out var data) ? data : null;
    }

    /// <summary>
    ///     Removes pending data for a character after it's been applied.
    /// </summary>
    public void ClearPendingData(string saveId)
    {
        _pendingCharacterData.Remove(saveId);
    }

    /// <summary>
    ///     Clears all pending data (used when starting a new game).
    /// </summary>
    public void ClearAllPendingData()
    {
        _pendingCharacterData.Clear();
    }

    /// <summary>
    ///     Stores a character's data in pending data so it persists to the next save.
    /// </summary>
    public void RegisterCharacterData(string saveId, Dictionary data)
    {
        _pendingCharacterData[saveId] = data;
    }

    private Dictionary GatherSaveData()
    {
        // Start with any pending data (includes dead characters)
        var characterData = new Dictionary<string, Dictionary>(_pendingCharacterData);

        // Gather/update data from all live Savable characters
        foreach (var node in GetTree().GetNodesInGroup("player"))
            if (node is Character savable)
                characterData[savable.SaveId] = savable.GatherSaveData();

        foreach (var node in GetTree().GetNodesInGroup("enemies"))
            if (node is Character savable)
                characterData[savable.SaveId] = savable.GatherSaveData();

        // Convert to array for storage
        var characters = new Array<Dictionary>();
        foreach (var data in characterData.Values) characters.Add(data);

        return new Dictionary
        {
            { "timestamp", Time.GetUnixTimeFromSystem() },
            { "scene", GetTree().CurrentScene.SceneFilePath },
            { "characters", characters }
        };
    }

    private void ApplySaveData(Dictionary data)
    {
        // Store character data to apply after scene loads
        _pendingCharacterData.Clear();

        var characters = (Array)data["characters"];
        foreach (var charData in characters)
        {
            var dict = (Dictionary)charData;
            var id = (string)dict["id"];
            _pendingCharacterData[id] = dict;
        }

        // Change to the saved scene
        var scenePath = (string)data["scene"];
        GetTree().ChangeSceneToFile(scenePath);
    }

    public bool SaveSettings(Dictionary settings)
    {
        var config = new ConfigFile();
        foreach (var section in settings.Keys)
        {
            var sectionDict = (Dictionary)settings[section];
            foreach (var key in sectionDict.Keys) config.SetValue(section.ToString(), key.ToString(), sectionDict[key]);
        }

        return config.Save(SettingsPath) == Error.Ok;
    }

    public Dictionary LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok) return GetDefaultSettings();

        var settings = new Dictionary();
        foreach (var section in config.GetSections())
        {
            var sectionDict = new Dictionary();
            foreach (var key in config.GetSectionKeys(section)) sectionDict[key] = config.GetValue(section, key);
            settings[section] = sectionDict;
        }

        return settings;
    }

    private static Dictionary GetDefaultSettings()
    {
        return new Dictionary
        {
            {
                "audio", new Dictionary
                {
                    { "master_volume", 1.0f },
                    { "music_volume", 0.8f },
                    { "sfx_volume", 1.0f }
                }
            },
            {
                "display", new Dictionary
                {
                    { "fullscreen", false },
                    { "vsync", true }
                }
            }
        };
    }
}