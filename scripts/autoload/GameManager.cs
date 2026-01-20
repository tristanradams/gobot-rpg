using Godot;

namespace RpgCSharp.scripts.autoload;

public partial class GameManager : Node
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Dialogue,
        Cutscene
    }

    private EventBus _eventBus;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public GameState PreviousState { get; private set; } = GameState.Menu;

    public Node Player { get; set; }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _eventBus = GetNode<EventBus>("/root/EventBus");
    }

    public void ChangeState(GameState newState)
    {
        PreviousState = CurrentState;
        CurrentState = newState;
        _eventBus.EmitSignal(EventBus.SignalName.GameStateChanged, (int)newState);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
            GetTree().Paused = true;
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
            GetTree().Paused = false;
        }
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    public void ReturnToMenu()
    {
        GetTree().Paused = false;
        ChangeState(GameState.Menu);
    }
}