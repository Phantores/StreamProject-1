using UnityEngine;
public class GameManager : Singleton<GameManager>
{
    public enum GameState {MainMenu, InGame, Cutscene, Pause, Loading}
    public GameState State { get; private set; } = GameState.MainMenu;

    protected override void Awake()
    {
        base.Awake();
        State = GameState.InGame;
    }

    public void SetState(GameState state)
    {
        State = state;
    }

    public void SetState(string state)
    {
        switch(state)
        {
            case "MainMenu": State = GameState.MainMenu; break;
            case "InGame": State = GameState.InGame; break;
            case "Cutscene": State = GameState.Cutscene; break;
            case "Pause": State = GameState.Pause; break;
            case "Loading": State = GameState.Loading; break;
            default: 
                throw new WrongStringExcpetion();
        }
    }
}

public class WrongStringExcpetion : System.Exception{}
