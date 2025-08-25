using System;
using Player;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    PlayerController player = null;
    public enum LoadMode { ToGame, ToMenu, ToCutscene}
    LoadMode flag = LoadMode.ToGame;

    public void SetPlayer(PlayerController player) {  this.player = player; }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(Scene scene, LoadMode loadMode)
    {
        try
        {
            GameManager.Instance.SetState("Loading");
            switch (loadMode)
            {
                case LoadMode.ToGame:
                    flag = LoadMode.ToGame;
                    break;
                case LoadMode.ToMenu:
                    flag = LoadMode.ToMenu;
                    break;
                case LoadMode.ToCutscene:
                    flag = LoadMode.ToCutscene;
                    break;
                default:
                    flag = LoadMode.ToGame;
                    break;
            }

            SceneManager.LoadScene(scene.name);
        }
        catch (ArgumentException e)
        {
            UnityEngine.Debug.LogError("Wrong string exception: " + e.Message);
            return;
        }

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            switch(flag)
            {
                case LoadMode.ToMenu:
                    GameManager.Instance.SetState("MainMenu");
                    break;
                case LoadMode.ToGame:
                    GameManager.Instance.SetState("InGame");
                    break;
                case LoadMode.ToCutscene:
                    GameManager.Instance.SetState("Cutscene");
                    break;
            }
        }
        catch (ArgumentException e)
        {
            UnityEngine.Debug.LogError("Wrong string exception: " + e.Message);
            return;
        }
    }
}
