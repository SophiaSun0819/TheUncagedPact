using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RestoreWalls : MonoBehaviour
{
    public AnchorPrefabSpawner spawner;

    public void Restore()
    {
        if (spawner == null)
        {
            Debug.LogError("Spawner not assigned.");
            return;
        }

        // 遍历清除所有生成的 prefab
        foreach (var go in spawner.AnchorPrefabSpawnerObjects.Values)
        {
            if (go != null)
                Destroy(go);
        }

        // 清空记录
        spawner.AnchorPrefabSpawnerObjects.Clear();

        Debug.Log("All wall prefabs removed — restored to passthrough walls.");
    }

 
}
