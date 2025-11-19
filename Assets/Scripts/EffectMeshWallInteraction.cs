using UnityEngine;
using Meta.XR.MRUtilityKit;

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
    [SerializeField] private float interactionDistance = 0.5f; // 互動距離（米）
    [SerializeField] private float interactionCooldown = 1f;
    [SerializeField] private LayerMask wallLayerMask = -1;
    
    [Header("調試")]
    [SerializeField] private bool debugMode = true;
    
    private Transform playerCamera;
    private float lastInteractionTime = 0f;
    private MRUK mruk;
    
    void Start()
    {
        // 獲取玩家相機
        playerCamera = Camera.main?.transform;
        if (playerCamera == null)
        {
            Debug.LogError("[EffectMeshWall] 找不到主相機！");
        }
        
        // 獲取 MRUK (Scene Understanding)
        mruk = FindObjectOfType<MRUK>();
        
        // 初始化預設線索
        InitializeDefaultClues();
        
        if (debugMode)
        {
            Debug.Log("[EffectMeshWall] 初始化完成");
            Debug.Log($"[EffectMeshWall] 設定了 {wallClues.Length} 個牆壁線索");
        }
    }
    
    void Update()
    {
        // 檢查玩家是否在看向牆壁並點擊
        CheckForWallInteraction();
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
        }
        
        if (!inputDetected) return;
        
        // 發射射線偵測牆壁
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance, wallLayerMask))
        {
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
            objName.Contains("anchor"))
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
    /// 當牆壁被點擊時
    /// </summary>
    private void OnWallClicked(RaycastHit hit)
    {
        lastInteractionTime = Time.time;
        
        // 判斷是哪一面牆
        WallDirection direction = DetermineWallDirection(hit);
        
        if (debugMode)
        {
            Debug.Log($"[EffectMeshWall] 玩家點擊了 {direction} 牆");
        }
        
        // 顯示對應的線索
        ShowClueForWall(direction);
    }
    
    /// <summary>
    /// 判斷牆壁方向
    /// </summary>
    private WallDirection DetermineWallDirection(RaycastHit hit)
    {
        // 取得牆壁的法線方向
        Vector3 normal = hit.normal;
        
        // 將法線轉換為世界空間方向
        Vector3 worldNormal = normal.normalized;
        
        // 計算與各方向的點積
        float dotNorth = Vector3.Dot(worldNormal, Vector3.forward);  // Z+
        float dotSouth = Vector3.Dot(worldNormal, Vector3.back);     // Z-
        float dotEast = Vector3.Dot(worldNormal, Vector3.right);     // X+
        float dotWest = Vector3.Dot(worldNormal, Vector3.left);      // X-
        
        // 找出最大值
        float maxDot = Mathf.Max(dotNorth, dotSouth, dotEast, dotWest);
        
        if (maxDot == dotNorth)
            return WallDirection.North;
        else if (maxDot == dotSouth)
            return WallDirection.South;
        else if (maxDot == dotEast)
            return WallDirection.East;
        else
            return WallDirection.West;
    }
    
    /// <summary>
    /// 顯示指定牆壁的線索
    /// </summary>
    private void ShowClueForWall(WallDirection direction)
    {
        // 找到對應的線索
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
            Debug.LogWarning($"[EffectMeshWall] 找不到 {direction} 牆的線索設定");
            return;
        }
        
        // 檢查是否已經完成
        if (clue.isPainted)
        {
            UIPromptManager.Instance?.ShowPrompt(
                "✅ Already Complete",
                $"This wall has already been painted with the correct color.",
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
                Debug.Log($"[EffectMeshWall] 顯示 {direction} 牆的線索");
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
            "✨ Correct!",
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
            "❌ Wrong Color!",
            errorMessage,
            $"💡 Hint: Read all four walls carefully\nWhat do the letters spell?"
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
        wallClues[0].clueTitle = "📜 Ancient Inscription";
        wallClues[0].clueContent = "B is for BIRD\nThe one who seeks the sky\nThe one who yearns to fly";
        wallClues[0].clueHint = "🔍 First letter: B";
        wallClues[0].requiredColor = Color.blue;
        
        // 東牆 - L
        if (wallClues[1] == null) wallClues[1] = new WallClue();
        wallClues[1].direction = WallDirection.East;
        wallClues[1].clueTitle = "📜 Whispers of Freedom";
        wallClues[1].clueContent = "L is for LIBERTY\nBreak free from chains\nSpread wings without restrains";
        wallClues[1].clueHint = "🔍 Second letter: L";
        wallClues[1].requiredColor = Color.blue;
        
        // 南牆 - U
        if (wallClues[2] == null) wallClues[2] = new WallClue();
        wallClues[2].direction = WallDirection.South;
        wallClues[2].clueTitle = "📜 Song of the Caged";
        wallClues[2].clueContent = "U is for UNBOUND\nNo cage can hold forever\nThe spirit that's untethered";
        wallClues[2].clueHint = "🔍 Third letter: U";
        wallClues[2].requiredColor = Color.blue;
        
        // 西牆 - E
        if (wallClues[3] == null) wallClues[3] = new WallClue();
        wallClues[3].direction = WallDirection.West;
        wallClues[3].clueTitle = "📜 Promise of Tomorrow";
        wallClues[3].clueContent = "E is for ESCAPE\nFind the color of endless skies\nWhere freedom truly lies";
        wallClues[3].clueHint = "💡 Hint: Color of the sky... B-L-U-E";
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
        
        if (debugMode)
        {
            Debug.Log("[EffectMeshWall] 所有牆壁已重置");
        }
    }
}
