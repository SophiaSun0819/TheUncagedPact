using UnityEngine;

public class RemoveWallEffectMesh : MonoBehaviour
{
     [Header("透明度（0 = 全透明）")]
    [Range(0f, 1f)]
    public float alpha = 0f;

    [Header("调试")]
    public bool debugMode = true;

    private void Start()
    {
        MakeAllWallsTransparent();
    }

    public void MakeAllWallsTransparent()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        int count = 0;

        foreach (var renderer in renderers)
        {
            string name = renderer.gameObject.name.ToLower();

            // 判断是否为 MR 墙
            if (name.Contains("wall") ||
                name.Contains("plane") ||
                name.Contains("anchor") ||
                name.Contains("mesh") ||
                name.Contains("effect"))
            {
                MakeTransparent(renderer);
                count++;
            }
        }

        if (debugMode)
            Debug.Log("已将 " + count + " 个墙面设为透明");
    }

    private void MakeTransparent(MeshRenderer renderer)
    {
        Material mat = renderer.material;
        if (mat == null) return;

        // 切透明模式
        mat.SetFloat("_Mode", 3);  // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        // 设置透明
        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }

        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }

        if (debugMode)
            Debug.Log("透明化：" + renderer.gameObject.name);
    }
}
