public class GameManager : Singleton<GameManager>
{
    public enum GameState {MainMenu, InGame, Cutscene, Pause}
    public GameState State { get; private set; } = GameState.MainMenu;

    protected override void Awake()
    {
        base.Awake();
        State = GameState.MainMenu;
    }

    public void ChangeState(GameState state)
    {
        State = state;
    }
}
