using UnityEngine;
using System.Collections;

/// <summary>
/// 牆壁透明化腳本
/// 支援兩種觸發方式：
/// 1. 自動：Level 1 完成後自動觸發
/// 2. 手動：按左手 Trigger 按鍵手動觸發
/// </summary>
public class SetTransparent : MonoBehaviour
{
    [Header("要替換成的材質（例如你的 customerShader 材質）")]
    public Material replacementMaterial;

    [Header("偵測設定")]
    public float checkInterval = 0.5f;
    public int maxChecks = 30;

    [Header("調試")]
    public bool debugMode = true;

    [Header("觸發模式")]
    [Tooltip("啟用自動觸發（Level 1 完成時）")]
    public bool enableAutoTrigger = true;

    [Tooltip("啟用手動觸發（左手 Trigger 按鍵）")]
    public bool enableManualTrigger = true;

    private int _checkCount = 0;
    private bool _hasFoundWalls = false;
    private bool coroutineStarted = false;

    private void Start()
    {
        // 如果啟用自動觸發，訂閱事件
        if (enableAutoTrigger)
        {
            StartCoroutine(WaitAndSubscribe());
        }
    }

    private void Update()
    {
        // 如果啟用手動觸發，監聽左手 Trigger
        if (enableManualTrigger)
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            {
                if (debugMode)
                {
                    Debug.Log("[SetTransparent] 🎮 Left Trigger pressed → Manual trigger!");
                }

                TriggerTransparency();
            }
        }
    }

    /// <summary>
    /// 等待 GameManager 初始化並訂閱事件
    /// </summary>
    private IEnumerator WaitAndSubscribe()
    {
        // 等待 GameManager 初始化
        while (GameManager.Instance == null)
        {
            yield return null;
        }

        // 訂閱事件
        GameManager.OnLevel1Complete += OnLevel1Complete;

        if (debugMode)
        {
            Debug.Log("[SetTransparent] Successfully subscribed to Level1Complete event");
        }
    }

    private void OnDestroy()
    {
        // 取消訂閱事件（避免記憶體洩漏）
        if (enableAutoTrigger && GameManager.Instance != null)
        {
            GameManager.OnLevel1Complete -= OnLevel1Complete;

            if (debugMode)
            {
                Debug.Log("[SetTransparent] Unsubscribed from Level1Complete event");
            }
        }
    }

    /// <summary>
    /// Level 1 完成時自動觸發（事件回調）
    /// </summary>
    private void OnLevel1Complete()
    {
        if (debugMode)
        {
            Debug.Log("[SetTransparent] Level 1 complete! Auto-triggering transparency...");
        }

        TriggerTransparency();
    }

    /// <summary>
    /// 統一的觸發入口（避免重複執行）
    /// </summary>
    private void TriggerTransparency()
    {
        if (coroutineStarted)
        {
            if (debugMode)
            {
                Debug.Log("[SetTransparent] Already processing, ignoring duplicate trigger");
            }
            return;
        }

        coroutineStarted = true;

        if (debugMode)
        {
            Debug.Log("[SetTransparent] ========================================");
            Debug.Log("[SetTransparent] Starting transparency process...");
            Debug.Log("[SetTransparent] ========================================");
        }

        StartCoroutine(ContinuousCheck());
    }

    /// <summary>
    /// 手動觸發替換（在 Inspector 右鍵選單中使用）
    /// </summary>
    [ContextMenu("Manual Replace (Test)")]
    public void ManualReplace()
    {
        if (debugMode)
        {
            Debug.Log("[SetTransparent] 🔧 Manual test trigger from context menu");
        }

        // 立即執行一次替換
        int count = ForceReplaceMaterials();

        if (debugMode)
        {
            Debug.Log($"[SetTransparent] 手動改變了 {count} 個物件");
        }
    }

    private IEnumerator ContinuousCheck()
    {
        while (_checkCount < maxChecks)
        {
            yield return new WaitForSeconds(checkInterval);

            int changedCount = ForceReplaceMaterials();

            if (changedCount > 0)
            {
                _hasFoundWalls = true;

                if (debugMode)
                {
                    Debug.Log($"[SetTransparent] 第 {_checkCount} 次檢查：替換了 {changedCount} 個物件");
                }
            }

            _checkCount++;

            if (_hasFoundWalls && _checkCount > 10)
            {
                break;
            }
        }

        if (debugMode)
        {
            Debug.Log("[SetTransparent] ========================================");
            Debug.Log("[SetTransparent] 監控結束");
            Debug.Log("[SetTransparent] ========================================");
        }
    }

    /// <summary>
    /// 找到所有 Meta/Room Mesh → 替換成 replacementMaterial
    /// </summary>
    private int ForceReplaceMaterials()
    {
        if (replacementMaterial == null)
        {
            Debug.LogWarning("[SetTransparent] ⚠ replacementMaterial 未指定，無法替換！");
            return 0;
        }

        int changedCount = 0;
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer renderer in allRenderers)
        {
            if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            string matName = renderer.sharedMaterial?.name.ToLower();
            if (matName == null) continue;

            // 判定是否為 Room Mesh/Effect Mesh
            if (matName.Contains("room") ||
                matName.Contains("wall") ||
                matName.Contains("scene") ||
                matName.Contains("plane") ||
                matName.Contains("effect") ||
                matName.Contains("anchor") ||
                matName.Contains("mesh"))
            {
                renderer.material = replacementMaterial;
                changedCount++;

                if (debugMode)
                {
                    Debug.Log($"[SetTransparent] 替換材質: {renderer.gameObject.name}");
                }
            }
        }

        return changedCount;
    }

    /// <summary>
    /// 重置狀態（用於測試）
    /// </summary>
    [ContextMenu("Reset State")]
    public void ResetState()
    {
        _checkCount = 0;
        _hasFoundWalls = false;
        coroutineStarted = false;

        if (debugMode)
        {
            Debug.Log("[SetTransparent] State reset");
        }
    }
}