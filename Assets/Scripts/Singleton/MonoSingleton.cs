using UnityEngine;

/// <summary>
/// MonoBehaviour 單例模式基類
/// 繼承此類可以快速創建單例
/// </summary>
/// <typeparam name="T">繼承的類型</typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// 單例實例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 嘗試在場景中找到現有實例
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        // 如果場景中沒有，創建新的 GameObject
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).ToString()} (Singleton)";

                        Debug.Log($"[MonoSingleton] Created new instance of {typeof(T)}");
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// 在 Awake 中設定單例
    /// 子類重寫 Awake 時需要調用 base.Awake()
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 應用程式退出時標記
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// 物件銷毀時清理
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
