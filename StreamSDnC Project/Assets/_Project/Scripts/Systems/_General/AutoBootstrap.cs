using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        if (Object.FindFirstObjectByType<Bootstrap>() == null)
        {
            // Load Bootstrap scene additively
            SceneManager.LoadScene("_Bootstrap", LoadSceneMode.Additive);
        }
    }
}
