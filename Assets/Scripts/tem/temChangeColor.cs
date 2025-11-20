using UnityEngine;
using System.Collections;
public class temChangeColor : MonoBehaviour
{
    [Header("顏色設定")]
    public Color wallColor = Color.white;

    [Header("偵測設定")]
    public float checkInterval = 0.5f;
    public int maxChecks = 30; // 檢查 15 秒

    [Header("調試")]
    public bool debugMode = true;

    private int _checkCount = 0;
    private bool _hasFoundWalls = false;


    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("[ForceWallColorChanger] 開始強制修改牆壁顏色");
        }

        // 立即執行一次
        ForceChangeAllWallColors();

        // 開始持續檢查
        StartCoroutine(ContinuousCheck());
    }

    /// <summary>
    /// 手動觸發顏色變更（可在 Quest 中按按鈕調用）
    /// </summary>
    public void ManualChange()
    {
        if (debugMode)
        {
            Debug.Log("[ForceWallColorChanger] 手動觸發顏色變更");
        }

        int count = ForceChangeAllWallColors();

        if (debugMode)
        {
            Debug.Log($"[ForceWallColorChanger] 手動改變了 {count} 個物件");
        }
    }

    private IEnumerator ContinuousCheck()
    {
        while (_checkCount < maxChecks)
        {
            yield return new WaitForSeconds(checkInterval);

            int changedCount = ForceChangeAllWallColors();

            if (changedCount > 0)
            {
                _hasFoundWalls = true;
                if (debugMode)
                {
                    Debug.Log($"[ForceWallColorChanger] 第 {_checkCount} 次檢查：改變了 {changedCount} 個網格");
                }
            }

            _checkCount++;

            // 如果已經找到牆壁，減少檢查頻率
            if (_hasFoundWalls && _checkCount > 10)
            {
                break;
            }
        }

        if (debugMode)
        {
            Debug.Log("[ForceWallColorChanger] 監控結束");
        }
    }

    private int ForceChangeAllWallColors()
    {
        int changedCount = 0;

        // 找到場景中所有的 MeshRenderer
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer renderer in allRenderers)
        {
            // 檢查是否為牆壁相關的物件
            string objName = renderer.gameObject.name.ToLower();

            // 跳過不相關的物件
            if (objName.Contains("cube") ||
                objName.Contains("controller") ||
                objName.Contains("hand") ||
                objName.Contains("ui"))
            {
                continue;
            }

            // 如果名稱包含 wall, effect, mesh 等關鍵字
            if (objName.Contains("wall") ||
                objName.Contains("effect") ||
                objName.Contains("mesh") ||
                objName.Contains("anchor") ||
                objName.Contains("plane"))
            {
                ChangeRendererColor(renderer);
                changedCount++;
            }
            // 或者檢查材質名稱
            else if (renderer.material != null)
            {
                string matName = renderer.material.name.ToLower();
                if (matName.Contains("room") ||
                    matName.Contains("wall") ||
                    matName.Contains("effect"))
                {
                    ChangeRendererColor(renderer);
                    changedCount++;
                }
            }
        }

        return changedCount;
    }

    private void ChangeRendererColor(MeshRenderer renderer)
    {
        if (renderer == null || renderer.material == null) return;

        try
        {
            // 創建材質實例（避免改到原始材質）
            Material mat = renderer.material;

            // 設定顏色
            mat.color = wallColor;

            // 嘗試設定為不透明（某些 Shader 可能不支援）
            try
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            catch
            {
                // 某些 Shader 不支援這些屬性，忽略錯誤
            }

            // 嘗試設定其他常見的顏色屬性
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", wallColor);
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", wallColor);
            }
            if (mat.HasProperty("_MainColor"))
            {
                mat.SetColor("_MainColor", wallColor);
            }

            if (debugMode)
            {
                Debug.Log($"[ForceWallColorChanger] ✅ 改變了 {renderer.gameObject.name} 的顏色");
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[ForceWallColorChanger] 無法改變 {renderer.gameObject.name}: {e.Message}");
            }
        }
    }
}
