using UnityEngine;
using System.Collections;
using Oculus.Interaction;

/// <summary>
/// 修改版 CubeColorTransfer
/// 當碰到牆壁時，直接修改牆壁的 Renderer 材質顏色
/// 不依賴 WallColorReceiver 組件
/// </summary>
public class CubeColorTransfer : MonoBehaviour
{
    [Header("顏色設定")]
    [Tooltip("Cube 的顏色")]
    public Color cubeColor = Color.red;

    [Header("碰撞設定")]
    [Tooltip("是否需要在被抓取時才能改變牆面顏色")]
    public bool requireGrabbed = true;

    [Header("視覺效果")]
    [Tooltip("顏色過渡速度（0 = 立即改變）")]
    public float transitionSpeed = 2f;

    [Header("調試")]
    [Tooltip("顯示調試訊息")]
    public bool debugMode = true;

    private Renderer _cubeRenderer;
    private Grabbable _grabbable;
    private bool _isGrabbed = false;

    private void Start()
    {
        // 獲取 Renderer 並設定顏色
        _cubeRenderer = GetComponent<Renderer>();
        if (_cubeRenderer != null)
        {
            if (cubeColor == Color.clear || cubeColor.a == 0)
            {
                cubeColor = _cubeRenderer.material.color;
            }
            else
            {
                _cubeRenderer.material.color = cubeColor;
            }
        }

        // 獲取 Grabbable 組件並訂閱事件
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] 初始化完成，顏色: {cubeColor}");
        }
    }

    /// <summary>
    /// 手動設定是否被抓取
    /// </summary>
    public void SetGrabbed(bool grabbed)
    {
        _isGrabbed = grabbed;
    }

    /// <summary>
    /// 手動設定 Cube 顏色
    /// </summary>
    public void SetCubeColor(Color newColor)
    {
        cubeColor = newColor;
        if (_cubeRenderer != null)
        {
            _cubeRenderer.material.color = newColor;
        }
    }

    private void OnDestroy()
    {
        // 取消訂閱事件
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    // 處理 Grabbable 的指針事件
    private void HandlePointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                _isGrabbed = true;
                if (debugMode)
                {
                    Debug.Log("[CubeColorTransfer] Cube 被抓取");
                }
                break;
            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                _isGrabbed = false;
                if (debugMode)
                {
                    Debug.Log("[CubeColorTransfer] Cube 被釋放");
                }
                break;
        }
    }

    // 物理碰撞
    private void OnCollisionEnter(Collision collision)
    {
        if (requireGrabbed && !_isGrabbed)
        {
            return;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] 碰撞到: {collision.gameObject.name}");
        }

        // 嘗試改變碰撞物件的顏色
        TryChangeObjectColor(collision.gameObject);
    }

    // Trigger 碰撞
    private void OnTriggerEnter(Collider other)
    {
        if (requireGrabbed && !_isGrabbed)
        {
            return;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] 觸發碰撞: {other.gameObject.name}");
        }

        // 嘗試改變碰撞物件的顏色
        TryChangeObjectColor(other.gameObject);
    }

    /// <summary>
    /// 嘗試改變物件的顏色
    /// </summary>
    private void TryChangeObjectColor(GameObject obj)
    {
        if (obj == null) return;

        string objName = obj.name.ToLower();

        // 檢查是否為牆壁或相關物件
        bool isWall = objName.Contains("wall") ||
                      objName.Contains("effect") ||
                      objName.Contains("mesh") ||
                      objName.Contains("anchor") ||
                      objName.Contains("plane");

        if (!isWall && debugMode)
        {
            Debug.Log($"[CubeColorTransfer] {obj.name} 不是牆壁，跳過");
            return;
        }

        // 獲取 Renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = obj.GetComponentInChildren<Renderer>();
        }

        if (renderer != null)
        {
            ChangeRendererColor(renderer, obj.name);
        }
        else if (debugMode)
        {
            Debug.LogWarning($"[CubeColorTransfer] {obj.name} 沒有 Renderer");
        }
    }

    /// <summary>
    /// 改變 Renderer 的顏色
    /// </summary>
    private void ChangeRendererColor(Renderer renderer, string objectName)
    {
        if (renderer == null || renderer.material == null) return;

        try
        {
            if (transitionSpeed > 0)
            {
                // 使用協程進行平滑過渡
                StartCoroutine(SmoothColorTransition(renderer, cubeColor));
            }
            else
            {
                // 立即改變顏色
                renderer.material.color = cubeColor;
            }

            // 嘗試設定其他顏色屬性
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.SetColor("_Color", cubeColor);
            }
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", cubeColor);
            }

            if (debugMode)
            {
                Debug.Log($"[CubeColorTransfer] ✅ 改變了 {objectName} 的顏色為 {cubeColor}");
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogError($"[CubeColorTransfer] 改變顏色時出錯: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 平滑顏色過渡
    /// </summary>
    private IEnumerator SmoothColorTransition(Renderer renderer, Color targetColor)
    {
        if (renderer == null || renderer.material == null) yield break;

        Color startColor = renderer.material.color;
        float elapsedTime = 0f;
        float duration = 1f / transitionSpeed;

        while (elapsedTime < duration)
        {
            if (renderer == null || renderer.material == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            renderer.material.color = currentColor;

            yield return null;
        }

        // 確保最終顏色正確
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = targetColor;
        }
    }

}