using Godot.Collections;

namespace RpgCSharp.scripts.interfaces;

public interface ISavable
{
    public string SaveId { get; }

    void Save();
    void Load();
    Dictionary GatherSaveData();
    bool ApplySaveData(Dictionary data);
}