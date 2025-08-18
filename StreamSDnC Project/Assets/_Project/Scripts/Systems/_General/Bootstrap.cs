using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "_MainMenu";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        InitializeServices();

        if (!SceneManager.GetSceneByName(mainSceneName).isLoaded)
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    private void InitializeServices()
    {
        Debug.Log("Bootstrap: Initializing global services.");
    }
}
