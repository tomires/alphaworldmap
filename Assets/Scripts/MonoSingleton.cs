using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (!instance)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }
}
