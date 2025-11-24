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
    private bool levelCompleted = false;

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
    public event Action OnLevel1Complete;

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
        // Delay showing welcome message to wait for UI initialization
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
        if (levelCompleted) return;

        levelCompleted = true;

        if (debugMode)
        {
            Debug.Log("[GameManager] ========================================");
            Debug.Log("[GameManager] LEVEL 1 COMPLETE - Blue Bird Puzzle Done!");
            Debug.Log("[GameManager] ========================================");
        }

        // Play completion sound
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        // Display victory message
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowSuccess(
                "FREEDOM ACHIEVED!",
                "The blue bird soars free at last\nNo longer bound by the past\n\n\"Thank you for breaking my cage\nAnd letting me start a new page\"",
                "Tutorial Complete! Preparing next challenge..."
            );
        }

        // Trigger next level after delay
        Invoke(nameof(TriggerNextLevel), delayBeforeNextLevel);
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
}