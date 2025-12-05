using UnityEngine;

public class InteractionRaycast : MonoBehaviour
{
    [Header("射線設定")]
    [Tooltip("射線起點（留空會自動尋找控制器）")]
    [SerializeField] private Transform rayOrigin;

    [Tooltip("射線最大距離")]
    [SerializeField] private float rayDistance = 3f;

    [Tooltip("可互動物體的Layer")]
    [SerializeField] private LayerMask interactableLayer = -1;

    [Header("輸入設定")]
    [Tooltip("互動按鈕")]
    [SerializeField] private OVRInput.Button interactButton = OVRInput.Button.PrimaryIndexTrigger;

    [Tooltip("使用哪個控制器")]
    [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("調試")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private bool debugMode = true;

    // 當前檢測到的物體
    private GameObject currentObject;
    private GameObject currentHintText;

    // 自動找到的控制器Transform
    private Transform autoRayOrigin;

    void Start()
    {
        // 如果沒有手動設定Ray Origin，自動尋找
        if (rayOrigin == null)
        {
            FindControllerTransform();
        }
    }

    /// <summary>
    /// 自動尋找控制器Transform
    /// </summary>
    private void FindControllerTransform()
    {
        if (debugMode)
        {
            Debug.Log("[InteractionRaycast] Searching for controller transform...");
        }

        // 尋找 OVRCameraRig
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();

        if (cameraRig != null)
        {
            // 根據設定的控制器選擇對應的anchor
            if (controller == OVRInput.Controller.RTouch)
            {
                autoRayOrigin = cameraRig.rightHandAnchor;
                if (debugMode)
                {
                    Debug.Log("[InteractionRaycast] Found Right Hand Anchor");
                }
            }
            else if (controller == OVRInput.Controller.LTouch)
            {
                autoRayOrigin = cameraRig.leftHandAnchor;
                if (debugMode)
                {
                    Debug.Log("[InteractionRaycast] Found Left Hand Anchor");
                }
            }
        }
        else
        {
            Debug.LogError("[InteractionRaycast] OVRCameraRig not found in scene!");
        }

        // 如果還是沒找到，嘗試用相機作為備用
        if (autoRayOrigin == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                autoRayOrigin = mainCamera.transform;
                Debug.LogWarning("[InteractionRaycast] Using Main Camera as fallback");
            }
        }
    }

    void Update()
    {
        CheckForInteractable();
        HandleInteraction();
    }

    /// <summary>
    /// 獲取當前使用的射線起點
    /// </summary>
    private Transform GetRayOrigin()
    {
        // 優先使用手動設定的
        if (rayOrigin != null)
            return rayOrigin;

        // 使用自動找到的
        if (autoRayOrigin != null)
            return autoRayOrigin;

        // 都沒有就返回null
        return null;
    }

    /// <summary>
    /// 檢測可互動物體
    /// </summary>
    private void CheckForInteractable()
    {
        Transform origin = GetRayOrigin();

        if (origin == null)
        {
            // 只在第一次報錯
            if (Time.frameCount % 300 == 0) // 每5秒報一次錯
            {
                Debug.LogError("[InteractionRaycast] Ray Origin not assigned and auto-find failed!");
            }
            return;
        }

        Ray ray = new Ray(origin.position, origin.forward);
        RaycastHit hit;

        // 發射射線
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // 如果是新物體
            if (currentObject != hitObject)
            {
                // 先隱藏舊的HintText
                HideCurrentHint();

                // 更新當前物體
                currentObject = hitObject;

                // 顯示新的HintText
                ShowHintForObject(hitObject);
            }
        }
        else
        {
            // 射線沒打到任何東西
            HideCurrentHint();
        }
    }

    /// <summary>
    /// 顯示物體的HintText
    /// </summary>
    private void ShowHintForObject(GameObject obj)
    {
        // 直接搜尋名為 "HintText" 的子物體
        Transform hintTransform = obj.transform.Find("HintText");

        // 如果直接子物體找不到，深度搜尋
        if (hintTransform == null)
        {
            hintTransform = FindDeepChild(obj.transform, "HintText");
        }

        if (hintTransform != null)
        {
            currentHintText = hintTransform.gameObject;
            currentHintText.SetActive(true);

            if (debugMode)
            {
                Debug.Log($"[InteractionRaycast] Showing HintText for: {obj.name}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"[InteractionRaycast] No HintText found on: {obj.name}");
            }
        }
    }

    /// <summary>
    /// 深度搜尋子物體（遞迴）
    /// </summary>
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// 隱藏當前的HintText
    /// </summary>
    private void HideCurrentHint()
    {
        if (currentHintText != null)
        {
            currentHintText.SetActive(false);

            if (debugMode)
            {
                Debug.Log($"[InteractionRaycast] Hiding HintText for: {currentObject?.name}");
            }
        }

        currentObject = null;
        currentHintText = null;
    }

    /// <summary>
    /// 處理互動輸入
    /// </summary>
    private void HandleInteraction()
    {
        if (currentObject != null &&
            OVRInput.GetDown(interactButton, controller))
        {
            if (debugMode)
            {
                Debug.Log($"[InteractionRaycast] Trigger pressed on: {currentObject.name}");
            }

            TriggerInteraction(currentObject);
        }
    }

    /// <summary>
    /// 觸發互動
    /// </summary>
    private void TriggerInteraction(GameObject obj)
    {
        // 查找Animator
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null)
        {
            animator = obj.GetComponentInParent<Animator>();
            if (animator == null)
            {
                animator = obj.GetComponentInChildren<Animator>();
            }
        }

        if (animator != null)
        {
            bool isOpen = animator.GetBool("open");
            animator.SetBool("open", !isOpen);

            if (debugMode)
            {
                Debug.Log($"[InteractionRaycast] Animator triggered: open = {!isOpen}");
            }
        }
    }

    /// <summary>
    /// 場景中繪製射線（調試用）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugRay) return;

        Transform origin = GetRayOrigin();
        if (origin != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin.position, origin.forward * rayDistance);
        }
    }
}