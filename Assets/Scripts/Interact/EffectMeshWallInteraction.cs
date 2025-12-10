using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

/// <summary>
/// Effect Mesh 牆壁互動控制器
/// 用於偵測玩家點擊/觸碰 Effect Mesh 的牆壁
/// 並顯示對應的線索
/// </summary>
public class EffectMeshWallInteraction : MonoBehaviour
{
    [System.Serializable]
    public class WallClue
    {
        [Header("牆壁識別")]
        [Tooltip("牆壁方向（用於識別）")]
        public WallDirection direction;

        [Header("線索內容")]
        [Tooltip("線索標題")]
        public string clueTitle = "Wall Clue";

        [Tooltip("線索內容（支援多行）")]
        [TextArea(3, 6)]
        public string clueContent = "Clue text here...";

        [Tooltip("提示文字（可選）")]
        public string clueHint = "";

        [Header("謎題設定")]
        [Tooltip("這面牆需要的顏色")]
        public Color requiredColor = Color.blue;

        [Tooltip("是否已經被塗色")]
        public bool isPainted = false;
    }

    public enum WallDirection
    {
        North,  // 北牆
        East,   // 東牆
        South,  // 南牆
        West    // 西牆
    }

    [Header("牆壁線索設定")]
    [SerializeField] private WallClue[] wallClues = new WallClue[4];

    [Header("互動設定")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float interactionCooldown = 1f;
    [SerializeField] private LayerMask wallLayerMask = -1;

    [Header("調試")]
    [SerializeField] private bool debugMode = true;

    [Header("線索追踪")]
    private List<int> unshownClueIndices = new List<int>();

    private Transform playerCamera;
    private float lastInteractionTime = 0f;
    private MRUK mruk;

    void Start()
    {
        playerCamera = Camera.main?.transform;

        if (playerCamera == null)
        {
            Debug.LogError("[EffectMeshWall] 找不到主相機！");
        }
        else
        {
            Debug.Log($"[EffectMeshWall] 找到相機: {playerCamera.name}");
        }

        mruk = FindObjectOfType<MRUK>();
        InitializeDefaultClues();
        ResetUnshownClues();

        if (debugMode)
        {
            Debug.Log("[EffectMeshWall] 初始化完成");
            Debug.Log($"[EffectMeshWall] 設定了 {wallClues.Length} 個牆壁線索");
        }
    }

    void Update()
    {
        CheckForWallInteraction();
    }

    /// <summary>
    /// 重置未顯示的線索列表
    /// </summary>
    private void ResetUnshownClues()
    {
        unshownClueIndices.Clear();
        for (int i = 0; i < wallClues.Length; i++)
        {
            unshownClueIndices.Add(i);
        }
    }

    /// <summary>
    /// 檢查玩家互動
    /// </summary>
    private void CheckForWallInteraction()
    {
        if (playerCamera == null) return;

        // 冷卻時間檢查
        if (Time.time - lastInteractionTime < interactionCooldown)
            return;

        // 檢查輸入（VR 手柄或滑鼠點擊）
        bool inputDetected = false;

        // VR 輸入檢查
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            inputDetected = true;
        }

        // 滑鼠點擊（用於編輯器測試）
        if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            Debug.Log("[EffectMeshWall] 偵測到 Trigger 按下！");
        }

        if (!inputDetected) return;

        Debug.Log("[EffectMeshWall] 準備發射射線...");
        Debug.Log($"[EffectMeshWall] 起點: {playerCamera.position}");
        Debug.Log($"[EffectMeshWall] 方向: {playerCamera.forward}");
        Debug.Log($"[EffectMeshWall] 距離: {interactionDistance}m");

        // 發射射線偵測牆壁
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance, wallLayerMask))
        {
            Debug.Log($"[EffectMeshWall] 擊中: {hit.collider.gameObject.name}");
            if (debugMode)
            {
                Debug.Log($"[EffectMeshWall] 射線擊中: {hit.collider.gameObject.name}");
            }

            // 檢查是否為牆壁
            if (IsWallObject(hit.collider.gameObject))
            {
                OnWallClicked(hit);
            }
        }
        else
        {
            Debug.Log("[EffectMeshWall] 沒有擊中任何東西");
        }
    }

    /// <summary>
    /// 判斷物件是否為牆壁
    /// </summary>
    private bool IsWallObject(GameObject obj)
    {
        string objName = obj.name.ToLower();

        // 檢查名稱
        if (objName.Contains("wall") ||
            objName.Contains("effectmesh") ||
            objName.Contains("plane") ||
            objName.Contains("anchor") ||
            objName.Contains("cube"))
        {
            return true;
        }

        // 檢查標籤
        if (obj.CompareTag("Wall"))
        {
            return true;
        }

        // 檢查是否有 MRUKAnchor 組件（Scene Understanding 的牆壁）
        var anchor = obj.GetComponent<MRUKAnchor>();
        if (anchor != null && anchor.Label == MRUKAnchor.SceneLabels.WALL_FACE)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 當墻壁被點擊時 - 隨機顯示線索（確保每個線索至少顯示一次）
    /// </summary>
    private void OnWallClicked(RaycastHit hit)
    {
        lastInteractionTime = Time.time;

        if (wallClues == null || wallClues.Length == 0)
        {
            Debug.LogError("[EffectMeshWall] No wall clues configured!");
            return;
        }

        // 如果所有線索都顯示過了，重置列表
        if (unshownClueIndices.Count == 0)
        {
            ResetUnshownClues();
            if (debugMode)
            {
                Debug.Log("[EffectMeshWall] 所有線索已顯示，重置列表");
            }
        }

        // 從未顯示的線索中隨機選一個
        int randomListIndex = Random.Range(0, unshownClueIndices.Count);
        int clueIndex = unshownClueIndices[randomListIndex];

        // 從列表中移除（確保短期內不會重複）
        unshownClueIndices.RemoveAt(randomListIndex);

        WallClue selectedClue = wallClues[clueIndex];

        if (debugMode)
        {
            Debug.Log($"[EffectMeshWall] =============================");
            Debug.Log($"[EffectMeshWall] 玩家點擊了牆壁: {hit.collider.gameObject.name}");
            Debug.Log($"[EffectMeshWall] 顯示線索 [{clueIndex}]: {selectedClue.clueTitle}");
            Debug.Log($"[EffectMeshWall] 剩餘未顯示線索數: {unshownClueIndices.Count}");
            Debug.Log($"[EffectMeshWall] =============================");
        }

        ShowClue(selectedClue);
    }

    /// <summary>
    /// 顯示線索
    /// </summary>
    private void ShowClue(WallClue clue)
    {
        if (clue == null)
        {
            Debug.LogWarning($"[EffectMeshWall] Clue is null!");
            return;
        }

        // 檢查是否已經完成
        if (clue.isPainted)
        {
            UIPromptManager.Instance?.ShowPrompt(
                "Already Complete",
                $"This clue has already been discovered.",
                ""
            );
            return;
        }

        // 顯示線索
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowClue(
                clue.clueTitle,
                clue.clueContent,
                clue.clueHint
            );

            if (debugMode)
            {
                Debug.Log($"[EffectMeshWall] 顯示線索: {clue.clueTitle}");
            }
        }
        else
        {
            Debug.LogError("[EffectMeshWall] 找不到 UIPromptManager！");
        }
    }

    /// <summary>
    /// 嘗試用指定顏色塗牆
    /// 從外部調用（例如從 ColorCube）
    /// </summary>
    public bool TryPaintWall(WallDirection direction, Color color)
    {
        // 找到對應的牆壁線索
        WallClue clue = null;
        foreach (var c in wallClues)
        {
            if (c.direction == direction)
            {
                clue = c;
                break;
            }
        }

        if (clue == null)
        {
            Debug.LogWarning($"[EffectMeshWall] 找不到 {direction} 牆的設定");
            return false;
        }

        // 檢查顏色是否正確
        bool isCorrect = ColorsMatch(color, clue.requiredColor);

        if (isCorrect)
        {
            clue.isPainted = true;
            ShowSuccessMessage(direction);

            // 通知 GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWallPainted(direction.ToString());
            }

            return true;
        }
        else
        {
            ShowErrorMessage(direction, color);
            return false;
        }
    }

    /// <summary>
    /// 顯示成功訊息
    /// </summary>
    private void ShowSuccessMessage(WallDirection direction)
    {
        string directionName = GetDirectionName(direction);

        UIPromptManager.Instance?.ShowSuccess(
            "Correct!",
            $"{directionName} wall has been painted!\n\nThe cage weakens...",
            $"Progress: {GetPaintedWallCount()}/4"
        );
    }

    /// <summary>
    /// 顯示錯誤訊息
    /// </summary>
    private void ShowErrorMessage(WallDirection direction, Color attemptedColor)
    {
        string directionName = GetDirectionName(direction);
        string colorName = ColorToString(attemptedColor);

        // 根據不同顏色顯示不同提示
        string errorMessage = GetColorErrorMessage(attemptedColor);

        UIPromptManager.Instance?.ShowError(
            "Wrong Color!",
            errorMessage,
            $"Hint: Read all four walls carefully\nWhat do the letters spell?"
        );
    }

    /// <summary>
    /// 根據錯誤顏色返回提示訊息
    /// </summary>
    private string GetColorErrorMessage(Color color)
    {
        if (ColorsMatch(color, Color.red))
        {
            return "Red is the color of rage and chains,\nnot the freedom the bird sustains.\n\nThink: What color is the sky?";
        }
        else if (ColorsMatch(color, Color.yellow))
        {
            return "Gold may shine but cannot free,\nThe bird that longs to truly be.\n\nHint: B-L-U-E... like the vast sky.";
        }
        else if (ColorsMatch(color, Color.green))
        {
            return "Green brings growth but not the wings,\nTo soar above all earthly things.\n\nRemember: The color of freedom!";
        }
        else
        {
            return "This is not the color of freedom.\n\nThe bird needs the color of the endless sky.";
        }
    }

    /// <summary>
    /// 初始化預設線索
    /// </summary>
    private void InitializeDefaultClues()
    {
        if (wallClues == null || wallClues.Length != 4)
        {
            wallClues = new WallClue[4];
        }

        // 北牆 - B
        if (wallClues[0] == null) wallClues[0] = new WallClue();
        wallClues[0].direction = WallDirection.North;
        wallClues[0].clueTitle = "Ancient Inscription";
        wallClues[0].clueContent = "B is for BIRD\nThe one who seeks the sky\nThe one who yearns to fly";
        wallClues[0].clueHint = "First letter: B";
        wallClues[0].requiredColor = Color.blue;

        // 東牆 - L
        if (wallClues[1] == null) wallClues[1] = new WallClue();
        wallClues[1].direction = WallDirection.East;
        wallClues[1].clueTitle = "Whispers of Freedom";
        wallClues[1].clueContent = "L is for LIBERTY\nBreak free from chains\nSpread wings without restrains";
        wallClues[1].clueHint = "Second letter: L";
        wallClues[1].requiredColor = Color.blue;

        // 南牆 - U
        if (wallClues[2] == null) wallClues[2] = new WallClue();
        wallClues[2].direction = WallDirection.South;
        wallClues[2].clueTitle = "Song of the Caged";
        wallClues[2].clueContent = "U is for UNBOUND\nNo cage can hold forever\nThe spirit that's untethered";
        wallClues[2].clueHint = "Third letter: U";
        wallClues[2].requiredColor = Color.blue;

        // 西牆 - E
        if (wallClues[3] == null) wallClues[3] = new WallClue();
        wallClues[3].direction = WallDirection.West;
        wallClues[3].clueTitle = "Promise of Tomorrow";
        wallClues[3].clueContent = "E is for ESCAPE\nFind the color of endless skies\nWhere freedom truly lies";
        wallClues[3].clueHint = "Hint: Color of the sky...";
        wallClues[3].requiredColor = Color.blue;
    }

    /// <summary>
    /// 工具方法
    /// </summary>
    private bool ColorsMatch(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    private string GetDirectionName(WallDirection direction)
    {
        switch (direction)
        {
            case WallDirection.North: return "North";
            case WallDirection.East: return "East";
            case WallDirection.South: return "South";
            case WallDirection.West: return "West";
            default: return "Unknown";
        }
    }

    private string ColorToString(Color color)
    {
        if (ColorsMatch(color, Color.red)) return "Red";
        if (ColorsMatch(color, Color.blue)) return "Blue";
        if (ColorsMatch(color, Color.green)) return "Green";
        if (ColorsMatch(color, Color.yellow)) return "Yellow";
        return "Unknown";
    }

    private int GetPaintedWallCount()
    {
        int count = 0;
        foreach (var clue in wallClues)
        {
            if (clue.isPainted) count++;
        }
        return count;
    }

    /// <summary>
    /// 重置所有牆壁（用於重新開始）
    /// </summary>
    public void ResetAllWalls()
    {
        foreach (var clue in wallClues)
        {
            clue.isPainted = false;
        }

        ResetUnshownClues();

        if (debugMode)
        {
            Debug.Log("[EffectMeshWall] 所有牆壁已重置");
        }
    }
}