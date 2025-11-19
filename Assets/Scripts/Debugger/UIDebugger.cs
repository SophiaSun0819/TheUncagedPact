using UnityEngine;

/// <summary>
/// UI 系統測試和除錯腳本
/// 掛在任何物件上（建議 GameManager）
/// </summary>
public class UIDebugger : MonoBehaviour
{
    [Header("自動測試")]
    [SerializeField] private bool autoTestOnStart = true;
    [SerializeField] private float autoTestDelay = 2f;
    
    [Header("手動測試按鍵")]
    [SerializeField] private KeyCode testPromptKey = KeyCode.T;
    [SerializeField] private KeyCode testSuccessKey = KeyCode.Y;
    [SerializeField] private KeyCode testErrorKey = KeyCode.U;
    [SerializeField] private KeyCode testClueKey = KeyCode.I;
    
    void Start()
    {
        Debug.Log("=== UIDebugger 啟動 ===");
        
        if (autoTestOnStart)
        {
            Invoke(nameof(RunDiagnostics), autoTestDelay);
        }
    }
    
    void Update()
    {
        // 按 T - 測試一般提示
        if (Input.GetKeyDown(testPromptKey))
        {
            TestPrompt();
        }
        
        // 按 Y - 測試成功訊息
        if (Input.GetKeyDown(testSuccessKey))
        {
            TestSuccess();
        }
        
        // 按 U - 測試錯誤訊息
        if (Input.GetKeyDown(testErrorKey))
        {
            TestError();
        }
        
        // 按 I - 測試線索
        if (Input.GetKeyDown(testClueKey))
        {
            TestClue();
        }
        
        // 按 ESC - 關閉 UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIPromptManager.Instance != null)
            {
                UIPromptManager.Instance.Hide();
                Debug.Log("✅ 已關閉 UI");
            }
        }
    }
    
    /// <summary>
    /// 執行完整診斷
    /// </summary>
    void RunDiagnostics()
    {
        Debug.Log("========================================");
        Debug.Log("開始 UI 系統診斷...");
        Debug.Log("========================================");
        
        // 1. 檢查 UIPromptManager
        Debug.Log("\n【檢查 1】UIPromptManager 實例");
        if (UIPromptManager.Instance == null)
        {
            Debug.LogError("❌ UIPromptManager.Instance 是 NULL！");
            Debug.LogError("→ 請檢查：");
            Debug.LogError("  1. PromptCanvas 是否存在且啟用");
            Debug.LogError("  2. UIPromptManager 腳本是否掛載");
            Debug.LogError("  3. 是否有編譯錯誤");
            return;
        }
        else
        {
            Debug.Log("✅ UIPromptManager.Instance 存在");
        }
        
        // 2. 檢查 GameManager
        Debug.Log("\n【檢查 2】GameManager 實例");
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager.Instance 是 NULL");
            Debug.LogWarning("→ GameManager 未初始化");
        }
        else
        {
            Debug.Log("✅ GameManager.Instance 存在");
        }
        
        // 3. 測試 UI 顯示
        Debug.Log("\n【檢查 3】測試 UI 顯示功能");
        try
        {
            UIPromptManager.Instance.ShowPrompt(
                "🎉 診斷成功！",
                "UI 系統運作正常！\n\n如果你看到這個訊息，代表一切正常。",
                "💡 按 ESC 關閉 | 按 T/Y/U/I 測試其他 UI"
            );
            Debug.Log("✅ UI 顯示功能正常");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UI 顯示失敗：{e.Message}");
            Debug.LogError($"→ 錯誤堆疊：{e.StackTrace}");
        }
        
        Debug.Log("\n========================================");
        Debug.Log("診斷完成！");
        Debug.Log("========================================");
        Debug.Log("\n快捷鍵：");
        Debug.Log($"  {testPromptKey} - 測試一般提示");
        Debug.Log($"  {testSuccessKey} - 測試成功訊息");
        Debug.Log($"  {testErrorKey} - 測試錯誤訊息");
        Debug.Log($"  {testClueKey} - 測試線索");
        Debug.Log("  ESC - 關閉 UI");
    }
    
    /// <summary>
    /// 測試一般提示
    /// </summary>
    void TestPrompt()
    {
        Debug.Log($"按下 {testPromptKey} 鍵 - 測試一般提示");
        
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowPrompt(
                "📢 一般提示",
                "這是一個普通的提示訊息\n\n支援多行文字\n可以顯示各種資訊",
                "💡 提示：這是 Hint 文字"
            );
            Debug.Log("✅ 已顯示一般提示");
        }
        else
        {
            Debug.LogError("❌ UIPromptManager.Instance 是 NULL");
        }
    }
    
    /// <summary>
    /// 測試成功訊息
    /// </summary>
    void TestSuccess()
    {
        Debug.Log($"按下 {testSuccessKey} 鍵 - 測試成功訊息");
        
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowSuccess(
                "✨ 成功！",
                "恭喜！你已經成功觸發成功訊息！\n\n綠色主題的 UI",
                "進度：1/4"
            );
            Debug.Log("✅ 已顯示成功訊息");
        }
        else
        {
            Debug.LogError("❌ UIPromptManager.Instance 是 NULL");
        }
    }
    
    /// <summary>
    /// 測試錯誤訊息
    /// </summary>
    void TestError()
    {
        Debug.Log($"按下 {testErrorKey} 鍵 - 測試錯誤訊息");
        
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowError(
                "❌ 錯誤！",
                "這是一個錯誤訊息範例\n\n紅色主題的 UI",
                "💡 提示：重試看看"
            );
            Debug.Log("✅ 已顯示錯誤訊息");
        }
        else
        {
            Debug.LogError("❌ UIPromptManager.Instance 是 NULL");
        }
    }
    
    /// <summary>
    /// 測試線索
    /// </summary>
    void TestClue()
    {
        Debug.Log($"按下 {testClueKey} 鍵 - 測試線索");
        
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowClue(
                "📜 Ancient Inscription",
                "B is for BIRD\nThe one who seeks the sky\nThe one who yearns to fly",
                "🔍 First letter: B"
            );
            Debug.Log("✅ 已顯示線索");
        }
        else
        {
            Debug.LogError("❌ UIPromptManager.Instance 是 NULL");
        }
    }
    
    /// <summary>
    /// 檢查場景設定
    /// </summary>
    [ContextMenu("檢查場景設定")]
    void CheckSceneSetup()
    {
        Debug.Log("=== 檢查場景設定 ===");
        
        // 檢查 PromptCanvas
        var canvas = GameObject.Find("PromptCanvas");
        if (canvas == null)
        {
            Debug.LogError("❌ 找不到 PromptCanvas 物件");
        }
        else
        {
            Debug.Log($"✅ 找到 PromptCanvas（Active: {canvas.activeSelf}）");
            
            // 檢查 UIPromptManager 組件
            var uiManager = canvas.GetComponent<UIPromptManager>();
            if (uiManager == null)
            {
                Debug.LogError("❌ PromptCanvas 上沒有 UIPromptManager 組件");
            }
            else
            {
                Debug.Log("✅ UIPromptManager 組件已掛載");
            }
        }
        
        // 檢查 PromptPanel
        var panel = GameObject.Find("PromptPanel");
        if (panel == null)
        {
            Debug.LogError("❌ 找不到 PromptPanel 物件");
        }
        else
        {
            Debug.Log($"✅ 找到 PromptPanel（Active: {panel.activeSelf}）");
            if (panel.activeSelf)
            {
                Debug.LogWarning("⚠️ PromptPanel 應該初始為隱藏（Active = False）");
            }
            
            // 檢查 CanvasGroup
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("❌ PromptPanel 沒有 CanvasGroup 組件");
            }
            else
            {
                Debug.Log("✅ PromptPanel 有 CanvasGroup 組件");
            }
        }
        
        // 檢查 GameManager
        var gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            Debug.LogWarning("⚠️ 找不到 GameManager 物件");
        }
        else
        {
            Debug.Log($"✅ 找到 GameManager（Active: {gameManager.activeSelf}）");
        }
        
        Debug.Log("=== 檢查完成 ===");
    }
}
