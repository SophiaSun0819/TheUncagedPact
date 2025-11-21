using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ShaderWaterLevelController : MonoBehaviour
{
    [Header("ShaderSettings")]
    // 在 Inspector 中拖拽 Water 模型的 MeshRenderer 组件到此槽位
    public MeshRenderer waterMeshRenderer;

    // Shader 中控制水位的 Float 属性的引用名称 (例如: "_Fill")
    // 请确保名称与 Shader Graph 中的属性名称一致！
    public string shaderWaterLevelPropertyName = "_Fill";

    [Header("WaterLevel")]
    [Tooltip("水体的起始水位值 (Shader 属性值)")]
    public float initialLevel = 0.0f;

    [Tooltip("水体的目标水位值 (Shader 属性值)")]
    public float targetLevel = 1.0f;

    [Tooltip("需要多少个 Eye 对象才能达到目标水位")]
    public int requiredEyesToFill = 3;

    [Header("Animation")]
    [Tooltip("水位上升动画的持续时间")]
    public float raiseDuration = 1.0f;

    [Header("Task Event")]
    public UnityEvent onWaterBottleComplete; //finish drop three eyebolls
    

    

    // 私有变量
    private Material waterMaterial;
    private int shaderPropertyID;
    private int currentEyeCount = 0;
    
    private bool taskCompleted = false; 

    void Start()
    {
        // 1. 获取 MeshRenderer 上的 Material 实例
        if (waterMeshRenderer != null)
        {
            waterMaterial = waterMeshRenderer.material;
            // 获取 Shader 属性的 ID (性能优化)
            shaderPropertyID = Shader.PropertyToID(shaderWaterLevelPropertyName);

            // 2. 初始化水位到起始值
            waterMaterial.SetFloat(shaderPropertyID, initialLevel);
        }
        else
        {
            Debug.LogError("Water MeshRenderer 槽位未设置! 无法控制水位 Shader。");
            enabled = false;
        }
    }

    // 当其他 Collider 进入水瓶的主 Collider 区域时触发
    void OnCollisionEnter(Collision collision)
    {
        // 检查碰撞到的物体是否是眼球，且未被计算过
        // 假设眼球对象的 Tag 是 "Eye"
        if (collision.gameObject.CompareTag("Eye"))
        {
            // 确保眼球有 Rigidbody，且没有被抓取 (需要根据您的抓取组件调整)
            Rigidbody eyeRb = collision.gameObject.GetComponent<Rigidbody>();

            // 假设我们通过禁用 Collider 来标记眼球已被“使用”
            Collider eyeCollider = collision.collider;
            if (eyeCollider != null && eyeCollider.enabled)
            {
                // 1. 禁用眼球的碰撞体和刚体，使其停止影响场景并防止重复计数
                eyeCollider.enabled = false;
                if (eyeRb != null) eyeRb.isKinematic = true;

                // 2. 累加眼球计数
                currentEyeCount++;
                Debug.Log($"眼球掉入! 当前计数: {currentEyeCount} / {requiredEyesToFill}");

                // 3. 计算新的目标水位
                float progress = Mathf.Clamp01((float)currentEyeCount / requiredEyesToFill);
                float newTargetLevel = Mathf.Lerp(initialLevel, targetLevel, progress);

                // 4. 启动水位上涨动画
                StartCoroutine(AnimateWaterLevel(newTargetLevel));
                 CheckTaskCompletion();
            }
        }
    }
    void CheckTaskCompletion()
    {
        if (!taskCompleted && currentEyeCount >= requiredEyesToFill)
        {
            taskCompleted = true;
            Debug.Log("任务完成！");
            onWaterBottleComplete?.Invoke(); // ← 触发事件
        }
    }

    // 平滑地改变 Shader 中的水位属性
    IEnumerator AnimateWaterLevel(float newTargetLevel)
    {
        float elapsedTime = 0;
        // 获取当前水位值作为起始值
        float startLevel = waterMaterial.GetFloat(shaderPropertyID);

        while (elapsedTime < raiseDuration)
        {
            float t = elapsedTime / raiseDuration;
            // 使用 Lerp 进行平滑插值，更新 Shader 属性
            float currentLevel = Mathf.Lerp(startLevel, newTargetLevel, t);
            waterMaterial.SetFloat(shaderPropertyID, currentLevel);

            elapsedTime += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保最终值精确
        waterMaterial.SetFloat(shaderPropertyID, newTargetLevel);
    }
}