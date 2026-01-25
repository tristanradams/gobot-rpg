using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.interfaces;

public interface IHasGlobals
{
    AudioManager AudioManager { get; set; }
    EventBus EventBus { get; set; }
    GameManager GameManager { get; set; }
    SaveManager SaveManager { get; set; }
}