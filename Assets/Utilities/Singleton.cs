using UnityEngine;
 
/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    static bool m_ShuttingDown = false;
    static readonly object m_Lock = new object();
    static T m_Instance;
    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance {
        get {
            if (m_ShuttingDown) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed. Returning null.");
                return null;
            }
            lock (m_Lock) {
                if (m_Instance == null) {
                    m_Instance = (T)FindObjectOfType(typeof(T));
                    if (m_Instance == null) CreateMonoBehaviourInstance(ref m_Instance);
                }
                return m_Instance;
            }
        }
    }
    static void CreateMonoBehaviourInstance<V>(ref V instance) where V : MonoBehaviour {
        var singletonObject = new GameObject();
        instance = singletonObject.AddComponent<V>();
        singletonObject.name = typeof(V).ToString() + " (Singleton)";
        DontDestroyOnLoad(singletonObject);
    }
    void OnApplicationQuit() => m_ShuttingDown = true;
    void OnDestroy() => m_ShuttingDown = true;
}