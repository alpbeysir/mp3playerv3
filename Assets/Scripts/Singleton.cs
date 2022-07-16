using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T Instance => _instance ? _instance : NotFound();
    
    private static T NotFound()
    {
        Debug.LogError(typeof(T) + " not found!");
        return null;
    }

    protected void Awake()
    {
        if (!_instance) _instance = (T)this;
        else Destroy(gameObject);
    }


    [SerializeField] private bool _loggingEnabled = true;

    protected void Log(object message)
    {
        if (_loggingEnabled && Application.isEditor) Debug.Log("[" + GetType().Name + "] " + message);
    }
    protected void LogError(object message)
    {
        if (_loggingEnabled && Application.isEditor) Debug.LogError("[" + GetType().Name + "] " + message);
    }
}
