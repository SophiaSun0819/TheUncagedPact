using TMPro;
using UnityEngine;

public class checkPassword : MonoBehaviour
{
    [Header("密碼設定")]
    public string correctPsw = "6307";

    [Header("UI 顯示")]
    public TextMeshPro[] pswTexts = new TextMeshPro[4];
    public TextMeshPro pswResult;

    [Header("輸入來源")]
    public passthroughCropCamera sender;

    [Header("音效設定（可選）")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    [Header("調試")]
    public bool debugMode = true;

    private int writeIndex;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        if (sender != null && sender.setPswDigit != null)
        {
            sender.setPswDigit.AddListener(SetPsw);
        }
    }

    void OnDisable()
    {
        if (sender != null && sender.setPswDigit != null)
            sender.setPswDigit.RemoveListener(SetPsw);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            CheckPsw();
        }
    }

    void SetPsw(int digit)
    {
        digit = Mathf.Clamp(digit, 0, 9);
        pswTexts[writeIndex].text = digit.ToString();
        writeIndex = Mathf.Min(writeIndex + 1, pswTexts.Length);

        if (debugMode)
        {
            Debug.Log($"[checkPassword] 輸入數字: {digit}, 當前位置: {writeIndex}");
        }
    }

    void CheckPsw()
    {
        string inputPsw = "";
        for (int i = 0; i < pswTexts.Length; i++)
        {
            inputPsw += string.IsNullOrEmpty(pswTexts[i].text) ? "" : pswTexts[i].text;
        }

        if (debugMode)
        {
            Debug.Log($"[checkPassword] 檢查密碼: {inputPsw}");
        }

        if (inputPsw == correctPsw)
        {
            pswResult.text = "correct";
            PlaySound(correctSound);

            if (debugMode)
            {
                Debug.Log("[checkPassword] 密碼正確！通知 GameManager");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPasswordComplete();
            }
            else
            {
                Debug.LogError("[checkPassword] GameManager.Instance is NULL!");
            }
        }
        else
        {
            pswResult.text = "incorrect";
            PlaySound(incorrectSound);

            if (debugMode)
            {
                Debug.Log($"[checkPassword] 密碼錯誤: {inputPsw} != {correctPsw}");
            }

            ClearAll();
        }
    }

    void ClearAll()
    {
        for (int i = 0; i < pswTexts.Length; i++)
        {
            pswTexts[i].text = "-";
        }
        writeIndex = 0;

        if (debugMode)
        {
            Debug.Log("[checkPassword] 清空密碼輸入");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ResetPassword()
    {
        ClearAll();
        pswResult.text = "";

        if (debugMode)
        {
            Debug.Log("[checkPassword] 密碼系統已重置");
        }
    }
}
