using UnityEngine;
 
/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    static bool m_ShuttingDown = false;
    static readonly object m_Lock = new object();
    static T m_Instance;
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static T Instance {
        get {
            if (m_ShuttingDown) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed. Returning null.");
                return null;
            }
            lock (m_Lock) {
                if (m_Instance == null) {
                    m_Instance = FindSingletonInstance<T>();
                    m_Instance ??= CreateMonoBehaviourInstance<T>();
                }
                return m_Instance;
            }
        }
    }
    static V FindSingletonInstance<V>() where V : MonoBehaviour{
        var instance = (V)FindObjectOfType(typeof(V));
        DontDestroyOnLoad(instance.gameObject);
        return instance;
    }
    static V CreateMonoBehaviourInstance<V>() where V : MonoBehaviour {
        var singletonObject = new GameObject();
        var instance = singletonObject.AddComponent<V>();
        singletonObject.name = typeof(V).ToString() + " (Singleton)";
        DontDestroyOnLoad(singletonObject);
        return instance;
    }
    void OnApplicationQuit() => m_ShuttingDown = true;
    void OnDestroy() => m_ShuttingDown = true;
}