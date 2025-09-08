using UnityEngine;

public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance  == null)
            {
                _instance = FindFirstObjectByType<T>();
                if(_instance == null)
                {
                    Debug.LogError($"No {typeof(T)} instance found in this scene!");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); return;
        }
        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
