using UnityEngine;

public class MaskEraser_LocalDecal : MonoBehaviour
{
    [Header("References")]
    public Material eraseMaterial;        // 半透明材质（柔边笔刷用）
    public OVRInput.Controller hand = OVRInput.Controller.RTouch;
    public LayerMask targetLayer;         // 要擦除的层

    [Header("Brush Settings")]
    public float brushRadius = 0.05f;     // 每个贴片大小
    public float brushInterval = 0.02f;   // 两次绘制的最小距离

    private Vector3 lastHitPoint;

    void Update()
    {
        // 从手柄发出射线
        Vector3 origin = OVRInput.GetLocalControllerPosition(hand);
        Quaternion rot = OVRInput.GetLocalControllerRotation(hand);
        Vector3 dir = rot * Vector3.forward;

        if (OVRInput.Get(OVRInput.Button.One))
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, 3f, targetLayer))
            {
                // 控制笔刷间隔，避免太密集
                if (Vector3.Distance(hit.point, lastHitPoint) > brushInterval)
                {
                    SpawnDecal(hit);
                    lastHitPoint = hit.point;
                }
            }
        }
    }

    void SpawnDecal(RaycastHit hit)
    {
        // 在碰撞点生成一个小的透明贴片
        GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decal.transform.position = hit.point + hit.normal * 0.001f;  // 稍微浮出表面
        decal.transform.rotation = Quaternion.LookRotation(-hit.normal);
        decal.transform.localScale = Vector3.one * brushRadius;

        // 设置透明材质
        var r = decal.GetComponent<MeshRenderer>();
        r.material = eraseMaterial;

        // 删除 collider，防止干扰下一次射线
        Destroy(decal.GetComponent<Collider>());

        // 可选：让贴片几秒后自动消失
        // Destroy(decal, 5f);
    }
}
