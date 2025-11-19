using UnityEngine;

/// <summary>
/// 遊戲管理器（適配 Effect Mesh 版本）
/// 繼承 MonoSingleton 實現單例模式
/// </summary>
public class GameManager : MonoSingleton<GameManager>
{
    [Header("Setting")]
    [SerializeField] private int totalWallsRequired = 4;

    [Header("Processed")]
    private int paintedWallCount = 0;
    private bool levelCompleted = false;

    [Header("AudioEffect")]
    [SerializeField] private AudioClip levelCompleteSound;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    protected override void Awake()
    {
        base.Awake();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (debugMode)
        {
            Debug.Log("[GameManager] Awake completed!");
        }
    }

    void Start()
    {
        // 延遲顯示歡迎訊息（等待 UI 初始化）
        Invoke(nameof(ShowWelcomeMessage), 1f);
    }

    /// <summary>
    /// 顯示歡迎訊息
    /// </summary>
    private void ShowWelcomeMessage()
    {
        // 檢查 UIPromptManager 是否存在且正確初始化
        if (UIPromptManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] UIPromptManager.Instance 是 NULL");
            return;
        }

        // 延遲一點時間，確保 UI 完全初始化
        StartCoroutine(ShowWelcomeMessageDelayed());
    }

    private System.Collections.IEnumerator ShowWelcomeMessageDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (UIPromptManager.Instance != null)
        {
            try
            {
                UIPromptManager.Instance.ShowPromptWithStyle(
                    "THE UNCAGED PACT",
                    "A bird trapped in a cage of lies. Dreams of freedom in the skies. Help it escape... Find the true color of liberty",
                    "Point at the walls and press trigger to discover clues",
                    PromptStyle.Default
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] Display welcome message failed: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] UIPromptManager not initialized yet");
        }

        if (debugMode)
        {
            Debug.Log("[GameManager] Game started");
        }
    }

    /// <summary>
    /// 當牆壁被成功塗色時調用（通用版本）
    /// </summary>
    public void OnWallPainted(string wallName = "")
    {
        paintedWallCount++;

        if (debugMode)
        {
            Debug.Log($"[GameManager] 進度: {paintedWallCount}/{totalWallsRequired} ({wallName})");
        }

        // 檢查是否完成
        if (paintedWallCount >= totalWallsRequired)
        {
            OnLevelComplete();
        }
    }

    /// <summary>
    /// 關卡完成
    /// </summary>
    private void OnLevelComplete()
    {
        if (levelCompleted) return;

        levelCompleted = true;

        if (debugMode)
        {
            Debug.Log("[GameManager] Level complete!");
        }

        // 播放完成音效
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        // 顯示勝利訊息
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowSuccess(
                "FREEDOM ACHIEVED!",
                "The blue bird soars free at last\nNo longer bound by the past\n\n\"Thank you for breaking my cage\nAnd letting me start a new page\"",
                "Tutorial Complete! Ready for the real escape?"
            );
        }

        // TODO: 觸發勝利動畫、關卡轉換等
        // StartCoroutine(VictorySequence());
    }

    /// <summary>
    /// 取得已塗色的牆壁數量
    /// </summary>
    public int GetPaintedWallCount()
    {
        return paintedWallCount;
    }

    /// <summary>
    /// 取得總牆壁數量
    /// </summary>
    public int GetTotalWallCount()
    {
        return totalWallsRequired;
    }

    /// <summary>
    /// 檢查是否完成
    /// </summary>
    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }

    /// <summary>
    /// 重置關卡
    /// </summary>
    public void ResetLevel()
    {
        paintedWallCount = 0;
        levelCompleted = false;

        // 重置牆壁互動系統
        var wallInteraction = FindFirstObjectByType<EffectMeshWallInteraction>();
        if (wallInteraction != null)
        {
            wallInteraction.ResetAllWalls();
        }

        // 重置顏色變換器
        var colorChanger = FindFirstObjectByType<ForceWallColorChanger>();
        if (colorChanger != null)
        {
            colorChanger.wallColor = Color.white;
            colorChanger.ManualChange();
        }

        if (debugMode)
        {
            Debug.Log("[GameManager] 關卡已重置");
        }

        ShowWelcomeMessage();
    }
}