
using UnityEngine;
using System.Collections;

public class SetTransparent : MonoBehaviour
{
    [Header("要替换成的材质（例如你的 customerShader 材质）")]
    public Material replacementMaterial;

    [Header("偵測設定")]
    public float checkInterval = 0.5f;
    public int maxChecks = 30;

    [Header("調試")]
    public bool debugMode = true;

    private int _checkCount = 0;
    private bool _hasFoundWalls = false;

    [Header("set level1 complete manually")]

    public bool setLevelComplete=false;
    private bool coroutineStarted = false;



    private void Start()
    {
        // if (debugMode)
        //     Debug.Log("[ReplaceMeshMaterial] 開始替換牆壁材質");

        // // 立即執行一次
        // // ForceReplaceMaterials();

        // // 持續檢查
        // StartCoroutine(ContinuousCheck());
    }
    private void Update()
{
    if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            setLevelComplete = true;
            Debug.Log("Left Trigger pressed → level complete!");
        }
        
    if (setLevelComplete && !coroutineStarted)
    {
        coroutineStarted = true;
        StartCoroutine(ContinuousCheck());
    }
}
//      private void OnEnable()
//     {
//         // 订阅事件
//        GameManager.OnLevel1Complete +=  OnLevel1Completed;
//     }

//     private void OnDisable()
//     {
//         // 取消订阅事件
//          GameManager.OnLevel1Complete  -=  OnLevel1Completed;
//     }

//     private void OnLevel1Completed()
// {
//     StartCoroutine(ContinuousCheck());
// }


    public void ManualReplace()
    {
        if (debugMode)
            Debug.Log("[ReplaceMeshMaterial] 手動觸發替換");

        int count = ForceReplaceMaterials();

        if (debugMode)
            Debug.Log($"[ReplaceMeshMaterial] 手動替換了 {count} 個物件");
    }

    private IEnumerator ContinuousCheck()
    {
        while (_checkCount < maxChecks)
        {
            yield return new WaitForSeconds(checkInterval);

            int changedCount = ForceReplaceMaterials();

            if (changedCount > 0)
            {
                _hasFoundWalls = true;

                if (debugMode)
                    Debug.Log($"[ReplaceMeshMaterial] 第 {_checkCount} 次檢查：替換了 {changedCount} 個物件");
            }

            _checkCount++;

            if (_hasFoundWalls && _checkCount > 10)
                break;
        }

        if (debugMode)
            Debug.Log("[ReplaceMeshMaterial] 監控結束");
    }

    /// <summary>
    /// 找到所有 Meta/Room Mesh → 替換成 replacementMaterial
    /// </summary>
    private int ForceReplaceMaterials()
    {
        if (replacementMaterial == null)
        {
            Debug.LogWarning("[ReplaceMeshMaterial] ⚠ replacementMaterial 未指定，無法替換！");
            return 0;
        }

        int changedCount = 0;
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer renderer in allRenderers)
        {
            string matName = renderer.sharedMaterial?.name.ToLower();
            if (matName == null) continue;

            // 判定是否為 Room Mesh/Effect Mesh
            if (matName.Contains("room") ||
                matName.Contains("wall") ||
                matName.Contains("scene") ||
                matName.Contains("plane") ||
                matName.Contains("effect") ||
                matName.Contains("anchor") ||
                matName.Contains("mesh"))
            {
                renderer.material = replacementMaterial;
                changedCount++;
            }
        }

        return changedCount;
    }
}
