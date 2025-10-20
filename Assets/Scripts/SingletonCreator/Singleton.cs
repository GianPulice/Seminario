using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning($"Singleton {typeof(T).Name} instance is null! Make sure the singleton is properly initialized.");
            }
            return instance;
        }
    }
    public static bool Exists => instance != null;

    // El parametro decide si se crea un singleton que no se destruye o si se crea uno que se destruye
    protected virtual void CreateSingleton(bool dontDestroyOnLoad)
    {
        if (instance == null)
        {
            instance = this as T;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
