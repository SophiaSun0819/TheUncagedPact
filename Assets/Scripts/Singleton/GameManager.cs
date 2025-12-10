using UnityEngine;
using System;

/// <summary>
/// Game Manager (Effect Mesh Version)
/// Inherits MonoSingleton to implement Singleton pattern
/// </summary>
public class GameManager : MonoSingleton<GameManager>
{
    [Header("Setting")]
    [SerializeField] private int totalWallsRequired = 4;

    [Header("Progress")]
    private int paintedWallCount = 0;
    private bool levelCompleted;

    [Header("Audio Effect")]
    [SerializeField] private AudioClip levelCompleteSound;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [Header("Level Transition")]
    [Tooltip("Delay in seconds before triggering next level")]
    [SerializeField] private float delayBeforeNextLevel = 3f;

    /// <summary>
    /// Action triggered when Level 1 is completed
    /// Team members can subscribe to this event in their scripts
    /// Usage: GameManager.Instance.OnLevel1Complete += YourMethod;
    /// </summary>
    public static event Action OnLevel1Complete;

    /// <summary>
    /// Action triggered when password puzzle is completed
    /// 密碼謎題完成事件 - 其他腳本可以訂閱此事件
    /// Usage: GameManager.OnPasswordPuzzleComplete += YourMethod;
    /// </summary>
    public static event Action OnPasswordPuzzleComplete;

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
        ResetLevelState();
        Invoke(nameof(ShowWelcomeMessage), 1f);
    }

    /// <summary>
    /// Display welcome message
    /// </summary>
    private void ShowWelcomeMessage()
    {
        // Check if UIPromptManager exists and is properly initialized
        if (UIPromptManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] UIPromptManager.Instance is NULL");
            return;
        }

        // Delay a bit to ensure UI is fully initialized
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
                    "A bird trapped in a cage of lies\nDreams of freedom in the skies\n\nHelp it escape...\nFind the true color of liberty",
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
            Debug.Log("[GameManager] Game started - Level 1: Blue Bird Puzzle");
        }
    }

    /// <summary>
    /// Called when a wall is successfully painted (generic version)
    /// </summary>
    public void OnWallPainted(string wallName = "")
    {
        paintedWallCount++;

        if (debugMode)
        {
            Debug.Log($"[GameManager] Progress: {paintedWallCount}/{totalWallsRequired} ({wallName})");
        }

        // Check if level is complete
        if (paintedWallCount >= totalWallsRequired)
        {
            OnLevelComplete();
        }
    }

    /// <summary>
    /// Level completion handler
    /// </summary>
    private void OnLevelComplete()
    {
        if (debugMode)
        {
            Debug.Log("[GameManager] ========================================");
            Debug.Log("[GameManager] OnLevelComplete 被呼叫");
            Debug.Log($"[GameManager] levelCompleted 為: {levelCompleted}");
        }

        if (levelCompleted)
        {
            if (debugMode)
            {
                Debug.Log("[GameManager] levelCompleted 是 true，直接返回");
                Debug.Log("[GameManager] ========================================");
            }
            return;
        }

        levelCompleted = true;

        if (debugMode)
        {
            Debug.Log("[GameManager] LEVEL 1 COMPLETE - Blue Bird Puzzle Done!");
            Debug.Log("[GameManager] ========================================");
        }

        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
            if (debugMode)
            {
                Debug.Log("[GameManager] 播放完成音效");
            }
        }

        if (debugMode)
        {
            Debug.Log("[GameManager] 準備顯示成功訊息...");
            Debug.Log($"[GameManager] UIPromptManager.Instance 是否為 null: {UIPromptManager.Instance == null}");
        }

        if (UIPromptManager.Instance != null)
        {
            try
            {
                if (debugMode)
                {
                    Debug.Log("[GameManager] 呼叫 ShowSuccess...");
                }

                UIPromptManager.Instance.ShowSuccess(
                    "FREEDOM ACHIEVED!",
                    "The blue bird soars free at last\nNo longer bound by the past\n\n\"Thank you for breaking my cage\nAnd letting me start a new page\"",
                    "Tutorial Complete! Preparing next challenge..."
                );

                if (debugMode)
                {
                    Debug.Log("[GameManager] ShowSuccess 呼叫成功");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] ShowSuccess 呼叫失败: {e.Message}");
                Debug.LogError($"[GameManager] StackTrace: {e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError("[GameManager] UIPromptManager.Instance is NULL!");
        }

        if (debugMode)
        {
            Debug.Log($"[GameManager] 準備在 {delayBeforeNextLevel} 秒後進入下一關");
        }

        Invoke(nameof(TriggerNextLevel), delayBeforeNextLevel);
    }

    /// <summary>
    /// 密碼謎題完成 - 由 checkPassword 腳本調用
    /// Called by checkPassword script when password is correct
    /// </summary>
    public void OnPasswordComplete()
    {
        if (debugMode)
        {
            Debug.Log("[GameManager] ========================================");
            Debug.Log("[GameManager] 密碼謎題完成！Password Puzzle Complete!");

            if (OnPasswordPuzzleComplete == null)
            {
                Debug.LogWarning("[GameManager] WARNING: No listeners subscribed to OnPasswordPuzzleComplete");
            }
            else
            {
                Debug.Log($"[GameManager] Broadcasting to {OnPasswordPuzzleComplete.GetInvocationList().Length} listener(s)");
            }

            Debug.Log("[GameManager] ========================================");
        }

        ShowPasswordCompleteMessage();
        // 觸發密碼完成事件
        OnPasswordPuzzleComplete?.Invoke();
    }

    /// <summary>
    /// 在鏡子 UI 顯示密碼完成訊息
    /// </summary>
    private void ShowPasswordCompleteMessage()
    {
        if (UIPromptManager.Instance != null)
        {
            try
            {
                UIPromptManager.Instance.ShowSuccess(
                    "PASSWORD UNLOCKED",
                    "The digital lock clicks open...\nSecrets once hidden are now revealed.\n\nKnowledge is the key to freedom",
                    "A new path awaits you"
                );

                if (debugMode)
                {
                    Debug.Log("[GameManager] 鏡子 UI 已顯示密碼完成訊息");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] 顯示密碼完成訊息失敗: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("[GameManager] UIPromptManager.Instance is NULL!");
        }
    }

    /// <summary>
    /// Trigger next level - Invoke Action
    /// </summary>
    private void TriggerNextLevel()
    {
        if (debugMode)
        {
            Debug.Log("[GameManager] ========================================");
            Debug.Log("[GameManager] Triggering OnLevel1Complete Action");

            if (OnLevel1Complete == null)
            {
                Debug.LogWarning("[GameManager] WARNING: No listeners subscribed to OnLevel1Complete");
            }
            else
            {
                Debug.Log($"[GameManager] Broadcasting to {OnLevel1Complete.GetInvocationList().Length} listener(s)");
            }

            Debug.Log("[GameManager] ========================================");
        }

        // Invoke Action
        OnLevel1Complete?.Invoke();
    }

    /// <summary>
    /// Manual trigger for next level (for testing)
    /// </summary>
    [ContextMenu("Manual Trigger Next Level")]
    public void ManualTriggerNextLevel()
    {
        if (debugMode)
        {
            Debug.Log("[GameManager] Manual trigger: Simulating level complete");
        }

        TriggerNextLevel();
    }

    /// <summary>
    /// Get the number of painted walls
    /// </summary>
    public int GetPaintedWallCount()
    {
        return paintedWallCount;
    }

    /// <summary>
    /// Get the total number of walls required
    /// </summary>
    public int GetTotalWallCount()
    {
        return totalWallsRequired;
    }

    /// <summary>
    /// Check if level is completed
    /// </summary>
    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }

    /// <summary>
    /// Reset level to initial state
    /// </summary>
    public void ResetLevel()
    {
        paintedWallCount = 0;
        levelCompleted = false;

        // Reset wall interaction system
        var wallInteraction = FindFirstObjectByType<EffectMeshWallInteraction>();
        if (wallInteraction != null)
        {
            wallInteraction.ResetAllWalls();
        }

        // Reset color changer
        var colorChanger = FindFirstObjectByType<ForceWallColorChanger>();
        if (colorChanger != null)
        {
            colorChanger.wallColor = Color.white;
            colorChanger.ManualChange();
        }

        if (debugMode)
        {
            Debug.Log("[GameManager] Level has been reset");
        }

        ShowWelcomeMessage();
    }

    /// <summary>
    /// 重置關卡狀態
    /// </summary>
    private void ResetLevelState()
    {
        paintedWallCount = 0;
        levelCompleted = false;

        // 重置牆壁互動系統
        var wallInteraction = FindFirstObjectByType<EffectMeshWallInteraction>();
        if (wallInteraction != null)
        {
            wallInteraction.ResetAllWalls();

            if (debugMode)
            {
                Debug.Log("[GameManager] 牆壁互動系統已重置");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] 找不到 EffectMeshWallInteraction！");
        }

        // 重置顏色變換器（如果需要的話）
        var colorChanger = FindFirstObjectByType<ForceWallColorChanger>();
        if (colorChanger != null)
        {
            colorChanger.wallColor = Color.white;
            colorChanger.ManualChange();

            if (debugMode)
            {
                Debug.Log("[GameManager] 顏色變換器已重置");
            }
        }

        if (debugMode)
        {
            Debug.Log("[GameManager] ========================================");
            Debug.Log("[GameManager] 關卡狀態已重置");
            Debug.Log($"[GameManager] paintedWallCount: {paintedWallCount}");
            Debug.Log($"[GameManager] levelCompleted: {levelCompleted}");
            Debug.Log("[GameManager] ========================================");
        }
    }
}