using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class MirrorSpawner : MonoBehaviour
{
    [Header("Mirror Settings")]
    public GameObject mirrorPrefab;

    [Header("Spawn Settings")]
    public float offsetFromWall = 0.05f;
    public float maxRaycastDistance = 10f;
    public float spawnDelay = 3f;

    [Header("Height Settings")]
    [Tooltip("Mirror spawn height (meters from floor)")]
    public float spawnHeight = 1.5f;
    [Tooltip("Use player's eye height instead of fixed height")]
    public bool usePlayerEyeHeight = false;
    [Tooltip("Offset from player eye height (if enabled)")]
    public float eyeHeightOffset = 0f;

    [Header("Filter Settings")]
    [Tooltip("Ignore objects with these names when raycasting")]
    public string[] ignoreObjectNames = new string[] { "Cube", "Sphere", "Hand", "Controller" };

    [Header("Debug")]
    public bool debugMode = true;

    private GameObject spawnedMirror;
    private UIPromptManager mirrorUIManager;

    void Start()
    {
        if (mirrorPrefab == null)
        {
            Debug.LogError("[MirrorSpawner] Mirror prefab not assigned!");
            return;
        }

        Invoke(nameof(SpawnMirrorOnFrontWall), spawnDelay);
    }

    private void SpawnMirrorOnFrontWall()
    {
        if (debugMode)
        {
            Debug.Log("[MirrorSpawner] ========================================");
            Debug.Log("[MirrorSpawner] Attempting to spawn mirror...");
        }

        Transform playerTransform = Camera.main.transform;
        if (playerTransform == null)
        {
            Debug.LogError("[MirrorSpawner] Camera.main not found!");
            return;
        }

        Vector3 playerPosition = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;

        if (debugMode)
        {
            Debug.Log($"[MirrorSpawner] Player position: {playerPosition}");
            Debug.Log($"[MirrorSpawner] Player forward: {playerForward}");
        }

        // 使用 RaycastAll 找到所有物件
        RaycastHit[] hits = Physics.RaycastAll(playerPosition, playerForward, maxRaycastDistance);

        if (debugMode)
        {
            Debug.Log($"[MirrorSpawner] Found {hits.Length} objects in raycast");
        }

        if (hits.Length == 0)
        {
            Debug.LogWarning("[MirrorSpawner] No objects found! Try increasing maxRaycastDistance");
            return;
        }

        // 按距離排序
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // 找第一個牆壁
        foreach (RaycastHit hit in hits)
        {
            string hitName = hit.collider.gameObject.name;

            if (debugMode)
            {
                Debug.Log($"[MirrorSpawner] Checking: {hitName} (distance: {hit.distance:F2}m)");
            }

            // 檢查是否要忽略
            bool shouldIgnore = false;
            foreach (string ignoreName in ignoreObjectNames)
            {
                if (hitName.ToLower().Contains(ignoreName.ToLower()))
                {
                    if (debugMode)
                    {
                        Debug.Log($"[MirrorSpawner]   -> Ignoring '{hitName}' (matches filter '{ignoreName}')");
                    }
                    shouldIgnore = true;
                    break;
                }
            }

            if (shouldIgnore) continue;

            // 檢查是否為牆壁
            if (IsWall(hit.collider.gameObject))
            {
                if (debugMode)
                {
                    Debug.Log($"[MirrorSpawner]   -> FOUND WALL: {hitName}");
                }
                SpawnMirror(hit.point, hit.normal);
                return;
            }
            else
            {
                if (debugMode)
                {
                    Debug.Log($"[MirrorSpawner]   -> Not a wall: {hitName}");
                }
            }
        }

        Debug.LogWarning("[MirrorSpawner] No wall found after checking all objects!");
        Debug.LogWarning("[MirrorSpawner] Hint: Make sure wall objects have 'wall', 'plane', 'mesh', 'anchor', or 'effect' in their name");
    }

    private bool IsWall(GameObject obj)
    {
        string objName = obj.name.ToLower();

        // 檢查 1: 名稱必須包含 "wall" 和 "mesh" 或 "effect"
        bool hasWallInName = objName.Contains("wall");
        bool hasMeshKeyword = objName.Contains("mesh") || objName.Contains("effect");

        if (!hasWallInName || !hasMeshKeyword)
        {
            if (debugMode)
            {
                Debug.Log($"[MirrorSpawner] {obj.name} - Name check failed");
            }
            return false;
        }

        // 檢查 2: 嘗試獲取 MRUKAnchor 組件
        MRUKAnchor anchor = obj.GetComponent<MRUKAnchor>();

        // 如果當前物體沒有，嘗試在父物體找
        if (anchor == null)
        {
            anchor = obj.GetComponentInParent<MRUKAnchor>();
        }

        if (anchor != null)
        {
            // 檢查 Label 是否為 WALL_FACE
            bool isWallFace = anchor.Label == MRUKAnchor.SceneLabels.WALL_FACE;

            if (debugMode)
            {
                Debug.Log($"[MirrorSpawner] Checking {obj.name}:");
                Debug.Log($"  - Has MRUKAnchor: v");
                Debug.Log($"  - Label: {anchor.Label}");
                Debug.Log($"  - Is WALL_FACE: {(isWallFace ? "v" : "x")}");
            }

            return isWallFace;
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"[MirrorSpawner] {obj.name} has 'wall' in name but no MRUKAnchor component");
            }
            return false;
        }
    }

    private void SpawnMirror(Vector3 position, Vector3 normal)
    {
        Vector3 spawnPosition;

        // === 根據設定選擇高度計算方式 ===
        if (usePlayerEyeHeight)
        {
            // 使用玩家當前眼睛高度
            Transform playerCamera = Camera.main.transform;
            if (playerCamera != null)
            {
                float playerHeight = playerCamera.position.y;
                spawnPosition = new Vector3(position.x, playerHeight + eyeHeightOffset, position.z);

                if (debugMode)
                {
                    Debug.Log($"[MirrorSpawner] Using player eye height: {playerHeight}m + offset: {eyeHeightOffset}m");
                }
            }
            else
            {
                // 備用：使用固定高度
                spawnPosition = new Vector3(position.x, spawnHeight, position.z);
                Debug.LogWarning("[MirrorSpawner] Camera not found, using fixed height");
            }
        }
        else
        {
            // 使用固定高度
            spawnPosition = new Vector3(position.x, spawnHeight, position.z);
        }

        // 添加牆面偏移
        spawnPosition += normal * offsetFromWall;

        Quaternion spawnRotation = Quaternion.LookRotation(-normal);

        spawnedMirror = Instantiate(mirrorPrefab, spawnPosition, spawnRotation);
        spawnedMirror.name = "Mirror (Spawned)";

        if (debugMode)
        {
            Debug.Log("[MirrorSpawner] ========================================");
            Debug.Log("[MirrorSpawner] SUCCESS! Mirror spawned!");
            Debug.Log($"[MirrorSpawner] Original wall hit: {position}");
            Debug.Log($"[MirrorSpawner] Final position: {spawnPosition}");
            Debug.Log($"[MirrorSpawner] Height: {spawnPosition.y}m");
            Debug.Log($"[MirrorSpawner] Rotation: {spawnRotation.eulerAngles}");
            Debug.Log("[MirrorSpawner] ========================================");
        }

        StartCoroutine(InitializeMirrorUI());
    }

    [ContextMenu("Test Spawn Mirror")]
    public void TestSpawnMirror()
    {
        if (spawnedMirror != null)
        {
            Destroy(spawnedMirror);
        }
        SpawnMirrorOnFrontWall();
    }

    /// <summary>
    /// 初始化 Mirror UI 並註冊為 Singleton
    /// </summary>
    private IEnumerator InitializeMirrorUI()
    {
        // 等待 Mirror 的所有組件初始化
        yield return null;

        if (debugMode)
        {
            Debug.Log("[MirrorSpawner] Searching for UIPromptManager on mirror...");
        }

        // 獲取 Mirror 上的 UIPromptManager
        mirrorUIManager = spawnedMirror.GetComponentInChildren<UIPromptManager>();

        if (mirrorUIManager == null)
        {
            Debug.LogError("[MirrorSpawner] UIPromptManager not found on mirror!");

            // 調試：顯示 Mirror 結構
            if (debugMode)
            {
                Debug.Log("[MirrorSpawner] Mirror hierarchy:");
                LogHierarchy(spawnedMirror.transform, 0);
            }
            yield break;
        }

        if (debugMode)
        {
            Debug.Log("[MirrorSpawner] Found UIPromptManager, registering as singleton...");
        }

        // 註冊為 Singleton
        mirrorUIManager.RegisterAsSingleton();

        // 再等一下確保註冊完成
        yield return new WaitForSeconds(0.3f);

        // 測試：顯示歡迎訊息
        ShowWelcomeOnMirror();

        if (debugMode)
        {
            Debug.Log("[MirrorSpawner] ========================================");
        }
    }

    /// <summary>
    /// 在 Mirror 上顯示歡迎訊息
    /// </summary>
    private void ShowWelcomeOnMirror()
    {
        // 方法 1：直接使用 mirrorUIManager
        if (mirrorUIManager != null)
        {
            if (debugMode)
            {
                Debug.Log("[MirrorSpawner] Showing welcome via direct reference...");
            }

            mirrorUIManager.ShowPromptWithStyle(
                "THE UNCAGED PACT",
                "A bird trapped in a cage of lies\nDreams of freedom in the skies\n\nHelp it escape...\nFind the true color of liberty",
                "Point at the walls and press trigger to discover clues",
                PromptStyle.Default
            );
        }

        // 方法 2：通過 Singleton 訪問（驗證註冊成功）
        if (UIPromptManager.Instance != null && debugMode)
        {
            Debug.Log("[MirrorSpawner] UIPromptManager.Instance is now available!");
            Debug.Log($"[MirrorSpawner] Instance == mirrorUIManager: {UIPromptManager.Instance == mirrorUIManager}");
        }
    }

    /// <summary>
    /// 調試：顯示物件層級
    /// </summary>
    private void LogHierarchy(Transform parent, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        var components = parent.GetComponents<Component>();
        string componentNames = string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name));

        Debug.Log($"{indentStr}├─ {parent.name} [{componentNames}]");

        foreach (Transform child in parent)
        {
            LogHierarchy(child, indent + 1);
        }
    }

    /// <summary>
    /// 獲取 Mirror UI Manager（供外部使用）
    /// </summary>
    public UIPromptManager GetMirrorUIManager()
    {
        return mirrorUIManager;
    }
}