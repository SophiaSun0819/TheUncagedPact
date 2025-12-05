using System.Collections.Generic;
using Oculus.Platform;
using UnityEngine;

public class FindWindowFrameByName : MonoBehaviour
{
    public GameObject windowPrefab;  //spawn on wall_art, otherwise spawn on window
    public GameObject waterBottle; //spawn on table

    public GameObject soundPuzzle; //sound puzzle spawn nearby door
    public GameObject drawer;
    public string WINDOW_FRAME = "WINDOW_FRAME";
    public string WALL_ART = "WALL_ART";
    public string TABLE="TABLE";
    public string DOOR="DOOR_FRAME";
    public string WALL="WALL_FRME"; //spawn drawer
    public float windowOffset = 0.3f; // window offset

    public float soundPuzzleOffset=0.5f;

    public float drawerOffset=0.2f; 

    //record floor height
    private float floorYPosition = 0f; 
    private bool isFloorFound = false;
    [Header("birdHouseOnTable")]
    public bool spawnBirdHouseOnTable=false;

    [Header("drawer near table")]
    public bool drawerNearTable=false;

    void Start()
    {
       

         Invoke("FindFloorHeight",3f);
         Invoke("SpawnWindow",3f);
        
        Invoke("SpawnTable", 3f); //spawn waterbottle
        Invoke("SpawnSoundPuzzle", 3f); 
        Invoke("SpawnDrawer",3f);
        Invoke("CleanupOriginalObjects", 4.0f);

    }

    void SpawnDrawer()
{
        if (drawerNearTable)
        {
            return;
        }
    
    MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>(); 
    
    List<GameObject> drawerTargets = new List<GameObject>();
    string targetNamePart = "wall"; // 要查找的名称部分
    string targetNamePart2 = "mesh";
    
    foreach (MeshRenderer renderer in allRenderers)
    {
        // 确保 MeshRenderer 所在的 GameObject 是激活的
        if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
        {
            continue;
        }

        GameObject obj = renderer.gameObject;
        string objName = obj.name;

        // 2. 将物体名称转为小写，然后判断是否同时包含 "drawer" 和 "mesh"
        // 例如：查找 "DRAWER_EffectMesh"
        if (objName.ToLower().Contains(targetNamePart) && objName.ToLower().Contains(targetNamePart2))
        {
            // 3. 找到符合条件的 Mesh 物体
            drawerTargets.Add(obj);
        }
    }

    // // 在每一面墙上都生成drawer
    // if (drawerTargets.Count > 0)
    // {
    //     // 假设您想在所有找到的抽屉目标上生成，如果只需要一个，请使用随机选择逻辑
    //     foreach (var t in drawerTargets)
    //     {
    //         SpawnAt(t, drawer, "drawer", true); 
    //     }
    // }
    // else
    // {
    //     Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 DRAWER 目标。");
    // }

    //随机在一面墙上生成drawer
    if (drawerTargets.Count > 0)
{
    
    int count = drawerTargets.Count;
    int randomIndex = Random.Range(0, count);
    GameObject randomTarget = drawerTargets[randomIndex];
    SpawnAt(randomTarget, drawer, "drawer", true); 
    
    Debug.Log($"✅ 成功在随机选择的 wall 目标 '{randomTarget.name}' 上生成 drawer。");
}
else
{
    Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 wall目标。");
}
}

    //spawn bottle or spawn birdHouse on table (spawnBirdHouseonTable==true)
void SpawnTable()
{
    // 1. 查找场景中所有激活的 MeshRenderer 组件
    MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
    // MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
    
    List<GameObject> tableTargets = new List<GameObject>();
    string targetNamePart = "bed"; // 要查找的名称部分
    string targetNamePart2 = "mesh";
    
    foreach (MeshRenderer renderer in allRenderers)
    {
        // 确保 MeshRenderer 所在的 GameObject 是激活的，并且不是一个临时的、不相关的对象
        if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
        {
            continue;
        }

        GameObject obj = renderer.gameObject;
        string objName = obj.name;

        // 2. 将物体名称转为小写，然后判断是否包含目标字符串
        // (例如: "TABLE_EffectMesh" -> "table_effectmesh")
        if (objName.ToLower().Contains(targetNamePart)&&objName.ToLower().Contains(targetNamePart2))
        {
            // 3. 找到符合条件的 Mesh 物体
            tableTargets.Add(obj);
        }
    }

    // // 4. 在每个table上生成bottle
    // if (tableTargets.Count > 0)
    // {
    //     foreach (var t in tableTargets)
    //     {
    //         SpawnAt(t, waterBottle, "bottle",false); // 使用中文标签，与您截图保持一致
    //     }
    // }
    // else
    // {
    //     Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 TABLE 目标。");
    // }

    // 4. 随机选table生成bottle
if (tableTargets.Count > 0)
{
    int count = tableTargets.Count;
    int randomIndex = Random.Range(0, count);

    GameObject randomTarget = tableTargets[randomIndex];
    SpawnAt(randomTarget, waterBottle, "bottle", false);
    if (spawnBirdHouseOnTable)
    {
        SpawnAt(randomTarget, soundPuzzle, "soundPuzzle", false); //birdhouse spawn在table上
    }
    if (drawerNearTable)
    {
         SpawnAt(randomTarget, drawer, "drawer", true); //drawer spawn near by table
    }
    
    Debug.Log($"✅ 成功在随机选择的 TABLE 目标 '{randomTarget.name}' 上生成 waterBottle，并禁用了目标对象。");
}
else
{
    Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 TABLE 目标。");
}
    
}

//sound puzzle near door, when spawnBirdHouseOnTable==false
void SpawnSoundPuzzle()
{
        if (spawnBirdHouseOnTable)
        {
            return;
        }
    
    MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
    
    List<GameObject> doorTargets = new List<GameObject>();
    string targetNamePart = "door"; // 要查找的名称部分
    string targetNamePart2 = "mesh";
    
    foreach (MeshRenderer renderer in allRenderers)
    {
        // 确保 MeshRenderer 所在的 GameObject 是激活的，并且不是一个临时的、不相关的对象
        if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
        {
            continue;
        }

        GameObject obj = renderer.gameObject;
        string objName = obj.name;

        // 2. 将物体名称转为小写，然后判断是否同时包含 "door" 和 "mesh"
        if (objName.ToLower().Contains(targetNamePart) && objName.ToLower().Contains(targetNamePart2))
        {
            // 3. 找到符合条件的 Mesh 物体
            doorTargets.Add(obj);
        }
    }

    // // 4. 在所有的door上生成
    // if (doorTargets.Count > 0)
    // {
    //     foreach (var t in doorTargets)
    //     {
            
    //         SpawnAt(t, soundPuzzle, "soundPuzzle", true); 
    //     }
    // }
    // else
    // {
    //     Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 DOOR 目标。");
    // }

    //随机选一个door生成
    if (doorTargets.Count > 0)
{
    
    int count = doorTargets.Count;
    int randomIndex = Random.Range(0, count);
    GameObject randomTarget = doorTargets[randomIndex];
    SpawnAt(randomTarget, soundPuzzle, "soundPuzzle", true); 
    Debug.Log($"✅ 成功在随机选择的 DOOR 目标 '{randomTarget.name}' 上生成 soundPuzzle。");
}
else
{
    Debug.LogWarning($"❌ 没找到名称包含 '{targetNamePart}' 且带有 MeshRenderer 的 DOOR 目标。");
}
}
    
void SpawnWindow()
{
    MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>(); // 或使用更快的 FindObjectsByType

    List<GameObject> wallArts = new List<GameObject>();
    List<GameObject> windows = new List<GameObject>();
    
    // 假设您想找的是 WINDOW_FRAME_EffectMesh
    string targetNamePart_WallArt = "wall_art"; // 或者您之前代码中的 WALL_ART 变量
    string targetNamePart_Window = "window_frame"; // 或者您之前代码中的 WINDOW_FRAME 变量
    string targetNamePart_Mesh = "mesh"; 

    foreach (MeshRenderer renderer in allRenderers)
    {
        if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
        {
            continue;
        }

        GameObject obj = renderer.gameObject;
        string objNameLower = obj.name.ToLower();

        // 查找 WALL_ART 目标：名称包含 'wallart' 且包含 'mesh'
        if (objNameLower.Contains(targetNamePart_WallArt) && objNameLower.Contains(targetNamePart_Mesh))
        {
            wallArts.Add(obj);
        }
        
        // 查找 WINDOW_FRAME 目标：名称包含 'windowframe' 且包含 'mesh'
        // 您也可以仅使用您原始的 "window" 查找：
        if (objNameLower.Contains(targetNamePart_Window) && objNameLower.Contains(targetNamePart_Mesh))
        {
            windows.Add(obj);
        }
    }

    // ===== Priority 1: WALL_ART (WallArts 列表) =====
    if (wallArts.Count > 0)
    {
        foreach (var t in wallArts)
        {
            SpawnAt(t, windowPrefab, "window",true);
        }
        return; // 找到并生成后，退出
    }
    
    // ===== Priority 2: WINDOW_FRAME (Windows 列表) =====
    if (windows.Count > 0)
    {
        foreach (var t in windows)
        {
            SpawnAt(t, windowPrefab, "window",true);
        }
        return; // 找到并生成后，退出
    }
    
    // ===== Fallback Warning =====
    Debug.LogWarning("❌ 没找到 WALL_ART 或 WINDOW_FRAME 目标。");
}




void SpawnAt(GameObject target, GameObject spawnPrefab, string prefabName, bool needOffset)
{
    Vector3 targetPos = target.transform.position;
    Vector3 spawnPos = targetPos; // 初始位置与目标位置相同
    Quaternion finalRotation = Quaternion.identity; // 默认零旋转

    if (needOffset)
    {
        Quaternion targetRotation = target.transform.rotation;
        Quaternion rotationAdjustment = Quaternion.Euler(0, 180, 0); 
        finalRotation = targetRotation * rotationAdjustment;
        
        Vector3 offsetVector = Vector3.zero;

        if (prefabName == "window")
        {
            // 贴墙物体：进行 Z 轴偏移
            offsetVector = finalRotation * Vector3.back * windowOffset;
            spawnPos = targetPos + offsetVector; 
        }
        
        else if (prefabName == "soundPuzzle")
        {       
                //只需要水平的offset
                if (spawnBirdHouseOnTable)
                {
                    
                     Instantiate(spawnPrefab, spawnPos, finalRotation);
                    return;
                }
           
            offsetVector = finalRotation * Vector3.back * soundPuzzleOffset;
            
            spawnPos = targetPos + offsetVector; 

            if (isFloorFound) 
            {

                spawnPos.y = floorYPosition;
                spawnPos.y += 0.02f; //vertical offset
            } 
            else 
            {
                // Fallback
                spawnPos.y = 0.2f; //vertical offset
            }

            // 地面物体通常不需要目标旋转，强制使用零旋转
            finalRotation = Quaternion.identity; 
        }
        else if (prefabName == "drawer")
            {
                 //只需要水平的offset
                // if (drawerNearTable)
                // {
                    
                //      Instantiate(spawnPrefab, spawnPos, finalRotation);
                //     return;
                // }
                if (drawerNearTable)
                    {
                        // 1. 世界 Z 轴偏移 1.0f (假设您想让它远离桌子)
                        float offsetDistance = 1.0f; 
                        Vector3 offsetVectorNearTable = Vector3.back * offsetDistance;
                        
                        // 2. 计算最终位置 (targetPos/spawnPos 加上世界偏移)
                        Vector3 finalSpawnPos = targetPos + offsetVectorNearTable;
                        
                        // 3. 实例化：使用修正后的位置和零旋转 (Quaternion.identity)
                        Instantiate(spawnPrefab, finalSpawnPos, Quaternion.identity);
                        return;
                    }
                
                offsetVector = finalRotation * Vector3.back * drawerOffset; 
                spawnPos = targetPos + offsetVector; 

                if (isFloorFound) 
                {
                    // 覆盖 Y 轴，设置为地板高度
                    spawnPos.y = floorYPosition;
                    spawnPos.y += 0.7f; // Vertical offset
                } 
                else 
                {
                    // Fallback: 如果找不到地板，使用默认高度
                    spawnPos.y = 0.7f; // Vertical offset
                }
            }
        
        // ----------------------------------------------------------------------
        // 2. 实例化物体
        // ----------------------------------------------------------------------
        Instantiate(spawnPrefab, spawnPos, finalRotation);
        return;
    }

    // needOffset == false 的情况
    Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
}

void FindFloorHeight()
{
    // 使用 FindObjectsOfType<MeshRenderer>() 查找所有渲染器
    // 推荐使用 FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
    MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
    
    string targetNamePart = "floor";
    string targetNamePart2 = "mesh";

    foreach (MeshRenderer renderer in allRenderers)
    {
        if (renderer == null || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
        {
            continue;
        }

        GameObject obj = renderer.gameObject;
        string objNameLower = obj.name.ToLower();

        // 查找名称同时包含 "floor" 和 "mesh" 的对象 (例如: FLOOR_EffectMesh)
        if (objNameLower.Contains(targetNamePart) && objNameLower.Contains(targetNamePart2))
        {
            // 💡 关键点：获取地板的最低 Y 坐标
            // EffectMesh 的 MeshRenderer.bounds.min.y 提供了地板的准确高度。
            floorYPosition = renderer.bounds.min.y;
            isFloorFound = true;
            Debug.Log($"✅ 找到 FLOOR_EffectMesh，地板高度 Y = {floorYPosition}");
            return; // 找到后立即退出
        }
    }

    Debug.LogWarning("❌ 没找到 FLOOR_EffectMesh，将使用默认 Y=0 作为地板高度。");
}

 void CleanupOriginalObjects()
    {
        windowPrefab.SetActive(false);  //spawn on wall_art, otherwise spawn on window
     waterBottle.SetActive(false); //spawn on table

    soundPuzzle.SetActive(false); //sound puzzle spawn nearby door
    drawer.SetActive(false);

    }
}
