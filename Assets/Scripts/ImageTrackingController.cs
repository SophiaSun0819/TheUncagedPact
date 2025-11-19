using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ImageTrackingController : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    // 設定圖片名稱與對應的 prefab
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private GameObject darkPrefab;

    // 使用 List 來儲存映射，避免在 Awake 中初始化 Dictionary
    [System.Serializable]
    public class ImagePrefabMapping
    {
        public string imageName;
        public GameObject prefab;
    }

    [SerializeField] private List<ImagePrefabMapping> imageMappings = new List<ImagePrefabMapping>();

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("[ImageTracking] 已訂閱圖像追蹤事件");
        }
        else
        {
            Debug.LogError("[ImageTracking] ARTrackedImageManager 未設定！");
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 處理新偵測到的圖片
        foreach (var trackedImage in eventArgs.added)
        {
            OnImageAdded(trackedImage);
        }

        // 處理更新的圖片
        foreach (var trackedImage in eventArgs.updated)
        {
            OnImageUpdated(trackedImage);
        }

        // 處理移除的圖片
        foreach (var trackedImage in eventArgs.removed)
        {
            OnImageRemoved(trackedImage);
        }
    }

    void OnImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        Debug.Log($"[ImageTracking] 偵測到圖片: {imageName}");

        // 檢查是否已經生成過
        if (spawnedPrefabs.ContainsKey(imageName))
        {
            Debug.Log($"[ImageTracking] {imageName} 已經生成過了");
            return;
        }

        // 從 List 中找到對應的 prefab
        GameObject prefabToSpawn = GetPrefabForImage(imageName);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[ImageTracking] 找不到 {imageName} 對應的 Prefab");
            return;
        }

        // 生成物件
        GameObject spawned = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
        spawned.transform.parent = trackedImage.transform; // 讓物件跟著圖片移動
        spawnedPrefabs[imageName] = spawned;

        Debug.Log($"[ImageTracking] ✅ 在 {imageName} 位置生成了物件");
    }

    void OnImageUpdated(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedPrefabs.ContainsKey(imageName))
        {
            GameObject spawned = spawnedPrefabs[imageName];
            if (spawned != null)
            {
                // 更新物件位置以跟隨圖片
                spawned.transform.position = trackedImage.transform.position;
                spawned.transform.rotation = trackedImage.transform.rotation;
            }
        }
    }

    void OnImageRemoved(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        Debug.Log($"[ImageTracking] 圖片 {imageName} 不再追蹤");

        // 可選：當圖片消失時隱藏物件
        if (spawnedPrefabs.ContainsKey(imageName))
        {
            GameObject spawned = spawnedPrefabs[imageName];
            if (spawned != null)
            {
                spawned.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 根據圖片名稱找到對應的 Prefab
    /// </summary>
    GameObject GetPrefabForImage(string imageName)
    {
        foreach (var mapping in imageMappings)
        {
            if (mapping.imageName == imageName)
            {
                return mapping.prefab;
            }
        }

        // 向下兼容舊的 Inspector 設定
        if (imageName == "BlueSky" && bluePrefab != null)
        {
            return bluePrefab;
        }
        if (imageName == "DarkSky" && darkPrefab != null)
        {
            return darkPrefab;
        }

        return null;
    }
}