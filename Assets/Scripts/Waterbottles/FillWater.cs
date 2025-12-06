using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class ShaderWaterLevelController : MonoBehaviour
{
    [Header("ShaderSettings")]
    public MeshRenderer waterMeshRenderer;

    public string shaderWaterLevelPropertyName = "_Fill";

    [Header("WaterLevel")]
    public float initialLevel = 0.0f;

    public float targetLevel = 1.0f;

    public int requiredEyesToFill = 3;

    [Header("Animation")]
    public float raiseDuration = 1.0f;

    [Header("Audio")]
    public AudioSource splashAudio;

    private Material waterMaterial;
    private int shaderPropertyID;
    private int currentEyeCount = 0;
    private bool taskCompleted;
    public UnityEvent onWaterBottleComplete;
    // Allow birds to check if water was already completed
    public bool IsComplete => taskCompleted;



    void Start()
    {
        // 1. 获取 MeshRenderer 上的 Material 实例
        if (waterMeshRenderer != null)
        {
            waterMaterial = waterMeshRenderer.material;
            shaderPropertyID = Shader.PropertyToID(shaderWaterLevelPropertyName);

            waterMaterial.SetFloat(shaderPropertyID, initialLevel);
        }
        else
        {
            Debug.LogError("[Water] waterMeshRenderer missing!");
            enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Eye"))
            return;

        Rigidbody eyeRb = other.attachedRigidbody;
        Collider eyeCollider = other;

        if (eyeCollider != null && eyeCollider.enabled)
        {
            // Stop physics
            eyeCollider.enabled = false;

            if (eyeRb != null)
            {
#if UNITY_6000_0_OR_NEWER
                eyeRb.linearVelocity = Vector3.zero;
#else
                eyeRb.velocity = Vector3.zero;
#endif
                eyeRb.angularVelocity = Vector3.zero;
                eyeRb.isKinematic = true;
                eyeRb.useGravity = false;
            }

            // Play splash
            if (splashAudio != null)
                splashAudio.Play();

            // Count eyes
            currentEyeCount++;
            Debug.Log($"[Water] Eye dropped! {currentEyeCount}/{requiredEyesToFill}");

            float progress = Mathf.Clamp01((float)currentEyeCount / requiredEyesToFill);
            float newTargetLevel = Mathf.Lerp(initialLevel, targetLevel, progress);

            StartCoroutine(AnimateWaterLevel(newTargetLevel));
            CheckTaskCompletion();
        }
    }

    void CheckTaskCompletion()
    {
        if (!taskCompleted && currentEyeCount >= requiredEyesToFill)
        {
            taskCompleted = true;
            Debug.Log("[Water] Complete!");
            onWaterBottleComplete?.Invoke();
        }
    }

    IEnumerator AnimateWaterLevel(float newTargetLevel)
    {
        float elapsedTime = 0;
        float startLevel = waterMaterial.GetFloat(shaderPropertyID);

        while (elapsedTime < raiseDuration)
        {
            float t = elapsedTime / raiseDuration;
            float currentLevel = Mathf.Lerp(startLevel, newTargetLevel, t);
            waterMaterial.SetFloat(shaderPropertyID, currentLevel);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        waterMaterial.SetFloat(shaderPropertyID, newTargetLevel);
    }
}