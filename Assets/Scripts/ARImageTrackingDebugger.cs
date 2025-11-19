using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARImageTrackingDebugger : MonoBehaviour
{
    [Header("必要組件")]
    public ARTrackedImageManager trackedImageManager;

    [Header("調試設定")]
    public bool enableVerboseLogging = true;
    public bool logEveryFrame = false;
    public float logInterval = 1f; // 每秒記錄一次

    private int frameCount = 0;
    private float nextLogTime = 0f;

    void Start()
    {
        Debug.Log("==========================================");
        Debug.Log("Meta XR Image Tracking Debugger 開始");
        Debug.Log("==========================================");

        CheckComponents();
        CheckImageLibrary();

        Debug.Log("==========================================");
        Debug.Log("診斷完成 - 請查看上方訊息");
        Debug.Log("==========================================");
    }

    void Update()
    {
        frameCount++;

        if (logEveryFrame && Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + logInterval;
            LogCurrentState();
        }
    }

    void CheckComponents()
    {
        Debug.Log("--- 檢查組件 ---");

        // 檢查 ARTrackedImageManager
        if (trackedImageManager == null)
        {
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        }

        if (trackedImageManager == null)
        {
            Debug.LogError("❌ 找不到 ARTrackedImageManager！");
            Debug.LogError("   解決方法：");
            Debug.LogError("   1. Hierarchy 右鍵 → XR → AR Tracked Image Manager");
            Debug.LogError("   2. 或使用 Building Block: Tracked Image");
        }
        else
        {
            Debug.Log($"✅ 找到 ARTrackedImageManager: {trackedImageManager.gameObject.name}");
            Debug.Log($"   - Enabled: {trackedImageManager.enabled}");
            Debug.Log($"   - GameObject Active: {trackedImageManager.gameObject.activeInHierarchy}");
        }

        Debug.Log("");
    }

    void CheckImageLibrary()
    {
        Debug.Log("--- 檢查 Reference Image Library ---");

        if (trackedImageManager == null)
        {
            Debug.LogError("❌ ARTrackedImageManager 為 null，無法檢查");
            return;
        }

        var referenceLibrary = trackedImageManager.referenceLibrary;

        if (referenceLibrary == null)
        {
            Debug.LogError("❌ Reference Library 未設定！");
            Debug.LogError("   解決方法：");
            Debug.LogError("   1. Project 右鍵 → Create → XR → Reference Image Library");
            Debug.LogError("   2. 添加圖片到 Library");
            Debug.LogError("   3. 在 ARTrackedImageManager 設定 Serialized Library");
            return;
        }

        Debug.Log($"✅ Reference Library: {referenceLibrary}");
        Debug.Log($"   圖片數量: {referenceLibrary.count}");

        if (referenceLibrary.count == 0)
        {
            Debug.LogError("❌ Library 中沒有圖片！");
            Debug.LogError("   請添加至少一張參考圖片");
        }
        else
        {
            Debug.Log("   === 圖片詳細資訊 ===");
            for (int i = 0; i < referenceLibrary.count; i++)
            {
                var refImage = referenceLibrary[i];
                Debug.Log($"   [{i + 1}] 名稱: {refImage.name}");
                Debug.Log($"       尺寸: {refImage.size.x}m x {refImage.size.y}m");
                Debug.Log($"       Texture: {(refImage.texture != null ? "✅ 已設定" : "❌ 未設定")}");

                if (refImage.size.x <= 0 || refImage.size.y <= 0)
                {
                    Debug.LogWarning($"       ⚠️ 警告：尺寸未設定或為 0！");
                    Debug.LogWarning($"       請在 Library 中設定 Physical Size");
                }

                if (refImage.texture == null)
                {
                    Debug.LogError($"       ❌ 錯誤：圖片紋理未設定！");
                }

                Debug.Log("");
            }
        }

        Debug.Log($"   Max Moving Images: {trackedImageManager}");
        Debug.Log($"   Tracked Image Prefab: {(trackedImageManager.trackedImagePrefab != null ? trackedImageManager.trackedImagePrefab.name : "未設定")}");
        Debug.Log("");
    }

    void LogCurrentState()
    {
        if (trackedImageManager == null) return;

        int trackedCount = trackedImageManager.trackables.count;

        Debug.Log($"[Frame {frameCount}] === 當前狀態 ===");
        Debug.Log($"   追蹤中的圖片數量: {trackedCount}");

        if (trackedCount > 0)
        {
            foreach (var trackedImage in trackedImageManager.trackables)
            {
                Debug.Log($"   📷 {trackedImage.referenceImage.name}");
                Debug.Log($"      Tracking State: {trackedImage.trackingState}");
                Debug.Log($"      Position: {trackedImage.transform.position}");
                Debug.Log($"      Active: {trackedImage.gameObject.activeInHierarchy}");
            }
        }
        else
        {
            Debug.Log("   (目前沒有偵測到任何圖片)");
        }

        Debug.Log("");
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("✅ 已訂閱 trackedImagesChanged 事件");
        }
        else
        {
            Debug.LogError("❌ 無法訂閱事件：trackedImageManager 為 null");
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            Debug.Log("已取消訂閱 trackedImagesChanged 事件");
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        Debug.Log("========================================");
        Debug.Log($"🎯 trackedImagesChanged 事件觸發！");
        Debug.Log($"   時間: {Time.time:F2}s");
        Debug.Log($"   Frame: {frameCount}");

        bool hasChanges = false;

        // 新增的圖片
        if (eventArgs.added.Count > 0)
        {
            hasChanges = true;
            Debug.Log($"✅ 新偵測到 {eventArgs.added.Count} 張圖片:");
            foreach (var image in eventArgs.added)
            {
                Debug.Log($"   + 圖片名稱: {image.referenceImage.name}");
                Debug.Log($"     Position: {image.transform.position}");
                Debug.Log($"     Rotation: {image.transform.rotation.eulerAngles}");
                Debug.Log($"     Tracking State: {image.trackingState}");
                Debug.Log($"     Size: {image.size}");
            }
        }

        // 更新的圖片
        if (eventArgs.updated.Count > 0)
        {
            hasChanges = true;
            Debug.Log($"🔄 更新 {eventArgs.updated.Count} 張圖片:");
            foreach (var image in eventArgs.updated)
            {
                Debug.Log($"   ~ 圖片名稱: {image.referenceImage.name}");
                Debug.Log($"     Tracking State: {image.trackingState}");
                Debug.Log($"     Position: {image.transform.position}");
            }
        }

        // 移除的圖片
        if (eventArgs.removed.Count > 0)
        {
            hasChanges = true;
            Debug.Log($"❌ 移除 {eventArgs.removed.Count} 張圖片:");
            foreach (var image in eventArgs.removed)
            {
                Debug.Log($"   - 圖片名稱: {image.referenceImage.name}");
            }
        }

        if (!hasChanges)
        {
            Debug.Log("   ℹ️ 事件觸發但沒有變化");
        }

        Debug.Log("========================================");
    }

    /// <summary>
    /// 手動觸發檢查（可在 Inspector 按鈕調用）
    /// </summary>
    [ContextMenu("手動檢查狀態")]
    public void ManualCheck()
    {
        Debug.Log("\n=== 手動檢查 ===");
        CheckComponents();
        CheckImageLibrary();
        LogCurrentState();
    }
}
