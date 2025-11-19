using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// 通用 UI 提示管理器
/// 可用於顯示各種訊息、線索、錯誤提示等
/// 支援多種樣式和動畫效果
/// </summary>
public class UIPromptManager : MonoSingleton<UIPromptManager>
{
    [Header("UI 組件引用")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button closeButton;

    [Header("動畫設定")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("音效")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;
    private AudioSource audioSource;

    [Header("預設樣式")]
    [SerializeField] private PromptStyle defaultStyle;
    [SerializeField] private PromptStyle successStyle;
    [SerializeField] private PromptStyle errorStyle;
    [SerializeField] private PromptStyle clueStyle;

    private CanvasGroup canvasGroup;
    private Coroutine currentAnimation;
    private bool isShowing = false;

    protected override void Awake()
    {
        base.Awake();

        // 如果不是場景中已有的實例，銷毀自己
        if (promptCanvas == null || promptPanel == null)
        {
            Debug.LogError("[UIPromptManager] ❌ UI 組件未設定！請在場景中手動創建 PromptCanvas 並設定所有引用。");
            Destroy(gameObject);
            return;
        }

        // 初始化組件
        canvasGroup = promptPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = promptPanel.AddComponent<CanvasGroup>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 初始隱藏
        promptPanel.SetActive(false);
        canvasGroup.alpha = 0;

        // 設定關閉按鈕
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Hide());

        Debug.Log("[UIPromptManager] 已初始化");
    }

    #region 公開方法 - 顯示不同類型的提示

    /// <summary>
    /// 顯示一般提示訊息
    /// </summary>
    public void ShowPrompt(string title, string content, string hint = "")
    {
        ShowPromptWithStyle(title, content, hint, defaultStyle);
    }

    /// <summary>
    /// 顯示成功訊息
    /// </summary>
    public void ShowSuccess(string title, string content, string hint = "")
    {
        PlaySound(successSound);
        ShowPromptWithStyle(title, content, hint, successStyle);
    }

    /// <summary>
    /// 顯示錯誤訊息
    /// </summary>
    public void ShowError(string title, string content, string hint = "")
    {
        PlaySound(errorSound);
        ShowPromptWithStyle(title, content, hint, errorStyle);
    }

    /// <summary>
    /// 顯示線索訊息
    /// </summary>
    public void ShowClue(string title, string content, string hint = "")
    {
        ShowPromptWithStyle(title, content, hint, clueStyle);
    }

    /// <summary>
    /// 顯示自訂樣式的提示
    /// </summary>
    public void ShowPromptWithStyle(string title, string content, string hint, PromptStyle style)
    {
        if (isShowing)
        {
            // 如果正在顯示，先隱藏再顯示新的
            Hide(() => ShowPromptInternal(title, content, hint, style));
        }
        else
        {
            ShowPromptInternal(title, content, hint, style);
        }
    }

    /// <summary>
    /// 隱藏提示
    /// </summary>
    public void Hide(System.Action onComplete = null)
    {
        if (!isShowing) return;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(FadeOutAnimation(onComplete));
        PlaySound(closeSound);
    }

    #endregion

    #region 私有方法

    private void ShowPromptInternal(string title, string content, string hint, PromptStyle style)
    {
        // 設定文字內容
        if (titleText != null)
        {
            titleText.text = title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }

        if (contentText != null)
        {
            contentText.text = content;
        }

        if (hintText != null)
        {
            hintText.text = hint;
            hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
        }

        // 套用樣式
        if (style != null)
        {
            ApplyStyle(style);
        }

        // 播放動畫
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(FadeInAnimation());
        PlaySound(openSound);

        isShowing = true;
    }

    private void ApplyStyle(PromptStyle style)
    {
        if (titleText != null)
        {
            titleText.color = style.titleColor;
            titleText.fontSize = style.titleFontSize;
        }

        if (contentText != null)
        {
            contentText.color = style.contentColor;
            contentText.fontSize = style.contentFontSize;
        }

        if (hintText != null)
        {
            hintText.color = style.hintColor;
            hintText.fontSize = style.hintFontSize;
        }
    }

    private IEnumerator FadeInAnimation()
    {
        promptPanel.SetActive(true);

        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = fadeInCurve.Evaluate(t);

            // 縮放動畫（可選）
            float scale = Mathf.Lerp(0.8f, 1f, fadeInCurve.Evaluate(t));
            promptPanel.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        canvasGroup.alpha = 1;
        promptPanel.transform.localScale = Vector3.one;
        currentAnimation = null;
    }

    private IEnumerator FadeOutAnimation(System.Action onComplete = null)
    {
        float elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = fadeOutCurve.Evaluate(t);

            // 縮放動畫（可選）
            float scale = Mathf.Lerp(1f, 0.8f, fadeOutCurve.Evaluate(t));
            promptPanel.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        canvasGroup.alpha = 0;
        promptPanel.SetActive(false);
        promptPanel.transform.localScale = Vector3.one;
        isShowing = false;
        currentAnimation = null;

        onComplete?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region 進階功能

    /// <summary>
    /// 顯示自動關閉的提示（持續指定秒數後自動關閉）
    /// </summary>
    public void ShowTimedPrompt(string title, string content, float duration = 3f)
    {
        ShowPrompt(title, content);
        StartCoroutine(AutoHideAfterDelay(duration));
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    /// <summary>
    /// 檢查是否正在顯示提示
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }

    #endregion
}

/// <summary>
/// UI 提示樣式設定
/// </summary>
[System.Serializable]
public class PromptStyle
{
    [Header("標題")]
    public Color titleColor = Color.white;
    public float titleFontSize = 36f;

    [Header("內容")]
    public Color contentColor = Color.white;
    public float contentFontSize = 24f;

    [Header("提示")]
    public Color hintColor = new Color(1, 1, 0.5f, 1);
    public float hintFontSize = 20f;

    // 預設樣式
    public static PromptStyle Default => new PromptStyle
    {
        titleColor = Color.white,
        contentColor = new Color(0.9f, 0.9f, 0.9f, 1f),
        hintColor = new Color(1f, 1f, 0.5f, 1f)
    };

    public static PromptStyle Success => new PromptStyle
    {
        titleColor = new Color(0.5f, 1f, 0.5f, 1f),
        contentColor = Color.white,
        hintColor = new Color(0.8f, 1f, 0.8f, 1f)
    };

    public static PromptStyle Error => new PromptStyle
    {
        titleColor = new Color(1f, 0.4f, 0.4f, 1f),
        contentColor = Color.white,
        hintColor = new Color(1f, 0.8f, 0.5f, 1f)
    };

    public static PromptStyle Clue => new PromptStyle
    {
        titleColor = new Color(0.8f, 0.6f, 1f, 1f),
        contentColor = new Color(0.95f, 0.95f, 1f, 1f),
        hintColor = new Color(0.7f, 0.9f, 1f, 1f)
    };
}