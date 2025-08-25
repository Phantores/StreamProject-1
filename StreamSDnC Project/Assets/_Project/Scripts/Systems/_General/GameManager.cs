using System;
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
        if (Enum.TryParse<GameState>(state, ignoreCase: true, out var parsed))
        {
            SetState(parsed);
            return;
        }

        throw new ArgumentException(
            $"Unknown state '{state}'. Allowed: {string.Join(", ", Enum.GetNames(typeof(GameState)))}",
            nameof(state));
    }
}
