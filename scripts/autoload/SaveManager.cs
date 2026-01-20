using Godot;

namespace RpgCSharp.scripts.autoload;

public partial class SaveManager : Node
{
    private const string SaveDir = "user://saves/";
    private const string SaveExtension = ".save";
    private const string SettingsPath = "user://settings.cfg";

    private Godot.Collections.Dictionary _currentSave = new();

    public override void _Ready()
    {
        EnsureSaveDirectory();
    }

    private void EnsureSaveDirectory()
    {
        DirAccess.MakeDirRecursiveAbsolute(SaveDir);
    }

    public bool SaveGame(int slot = 0)
    {
        var saveData = GatherSaveData();
        var path = GetSavePath(slot);

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError($"Failed to open save file: {path}");
            return false;
        }

        file.StoreVar(saveData);
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

        _currentSave = (Godot.Collections.Dictionary)file.GetVar();
        ApplySaveData(_currentSave);
        return true;
    }

    public bool DeleteSave(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (FileAccess.FileExists(path))
        {
            return DirAccess.RemoveAbsolute(path) == Error.Ok;
        }
        return false;
    }

    public bool HasSave(int slot = 0)
    {
        return FileAccess.FileExists(GetSavePath(slot));
    }

    private string GetSavePath(int slot)
    {
        return $"{SaveDir}slot_{slot}{SaveExtension}";
    }

    protected virtual Godot.Collections.Dictionary GatherSaveData()
    {
        return new Godot.Collections.Dictionary
        {
            { "timestamp", Time.GetUnixTimeFromSystem() },
            { "player", new Godot.Collections.Dictionary() },
            { "inventory", new Godot.Collections.Dictionary() },
            { "quests", new Godot.Collections.Dictionary() },
            { "world", new Godot.Collections.Dictionary() }
        };
    }

    protected virtual void ApplySaveData(Godot.Collections.Dictionary data)
    {
        // Override in subclass to apply loaded data
    }

    public bool SaveSettings(Godot.Collections.Dictionary settings)
    {
        var config = new ConfigFile();
        foreach (var section in settings.Keys)
        {
            var sectionDict = (Godot.Collections.Dictionary)settings[section];
            foreach (var key in sectionDict.Keys)
            {
                config.SetValue(section.ToString(), key.ToString(), sectionDict[key]);
            }
        }
        return config.Save(SettingsPath) == Error.Ok;
    }

    public Godot.Collections.Dictionary LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok)
        {
            return GetDefaultSettings();
        }

        var settings = new Godot.Collections.Dictionary();
        foreach (var section in config.GetSections())
        {
            var sectionDict = new Godot.Collections.Dictionary();
            foreach (var key in config.GetSectionKeys(section))
            {
                sectionDict[key] = config.GetValue(section, key);
            }
            settings[section] = sectionDict;
        }
        return settings;
    }

    private Godot.Collections.Dictionary GetDefaultSettings()
    {
        return new Godot.Collections.Dictionary
        {
            {
                "audio", new Godot.Collections.Dictionary
                {
                    { "master_volume", 1.0f },
                    { "music_volume", 0.8f },
                    { "sfx_volume", 1.0f }
                }
            },
            {
                "display", new Godot.Collections.Dictionary
                {
                    { "fullscreen", false },
                    { "vsync", true }
                }
            }
        };
    }
}