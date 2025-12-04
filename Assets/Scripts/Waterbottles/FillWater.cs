using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ShaderWaterLevelController : MonoBehaviour
{
    [Header("Shader Settings")]
    // Drag the water MeshRenderer here in the Inspector
    public MeshRenderer waterMeshRenderer;

    // Name of the float property in your shader (e.g. "_Fill")
    public string shaderWaterLevelPropertyName = "_Fill";

    [Header("Water Level")]
    [Tooltip("Starting water level (shader value)")]
    public float initialLevel = 0.0f;

    [Tooltip("Target water level (shader value) when all eyes are in")]
    public float targetLevel = 1.0f;

    [Tooltip("How many Eye objects are needed to reach target level")]
    public int requiredEyesToFill = 3;

    [Header("Animation")]
    [Tooltip("Duration of one step of water rising")]
    public float raiseDuration = 1.0f;

    [Header("Audio")]
    [Tooltip("Optional: sound played when an eye drops in")]
    public AudioSource splashAudio;

    [Header("Task Event")]
    public UnityEvent onWaterBottleComplete; // fired when all eyes are in

    // Private state
    Material waterMaterial;
    int shaderPropertyID;
    int currentEyeCount = 0;
    bool taskCompleted = false;

    void Start()
    {
        if (waterMeshRenderer != null)
        {
            waterMaterial = waterMeshRenderer.material;
            shaderPropertyID = Shader.PropertyToID(shaderWaterLevelPropertyName);

            // Initialize water level
            waterMaterial.SetFloat(shaderPropertyID, initialLevel);
        }
        else
        {
            Debug.LogError("[Water] waterMeshRenderer is not assigned!");
            enabled = false;
        }
    }

    /// <summary>
    /// This is now a TRIGGER, not a collision.
    /// Make sure the water bottle collider has IsTrigger = true
    /// and the eyes have a Rigidbody + tag = "Eye".
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Only react to eyes
        if (!other.CompareTag("Eye"))
            return;

        // Get the rigidbody on the eye
        Rigidbody eyeRb = other.attachedRigidbody;
        Collider eyeCollider = other;

        // Only count if collider is still enabled (so we don't double-count)
        if (eyeCollider != null && eyeCollider.enabled)
        {
            // 1. Disable collider & freeze physics so it "sits" in the water
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

            // 2. Play splash sound
            if (splashAudio != null)
            {
                splashAudio.Play();
            }

            // 3. Increase count & update water level
            currentEyeCount++;
            Debug.Log($"[Water] Eye dropped in! {currentEyeCount}/{requiredEyesToFill}");

            float progress = Mathf.Clamp01((float)currentEyeCount / requiredEyesToFill);
            float newTargetLevel = Mathf.Lerp(initialLevel, targetLevel, progress);

            StartCoroutine(AnimateWaterLevel(newTargetLevel));

            // 4. Check if puzzle is complete
            CheckTaskCompletion();
        }
    }

    void CheckTaskCompletion()
    {
        if (!taskCompleted && currentEyeCount >= requiredEyesToFill)
        {
            taskCompleted = true;
            Debug.Log("[Water] All eyes collected – task complete!");
            onWaterBottleComplete?.Invoke();
        }
    }

    IEnumerator AnimateWaterLevel(float newTargetLevel)
    {
        float elapsedTime = 0f;
        float startLevel = waterMaterial.GetFloat(shaderPropertyID);

        while (elapsedTime < raiseDuration)
        {
            float t = elapsedTime / raiseDuration;
            float currentLevel = Mathf.Lerp(startLevel, newTargetLevel, t);
            waterMaterial.SetFloat(shaderPropertyID, currentLevel);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to final value
        waterMaterial.SetFloat(shaderPropertyID, newTargetLevel);
    }
}
