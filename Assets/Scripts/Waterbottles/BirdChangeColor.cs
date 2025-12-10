using UnityEngine;
using UnityEngine.Events;

public class BirdChangeColor : MonoBehaviour
{
    [Header("Set Bird Color")]
    [Tooltip("The color the bird should change to after the water puzzle is complete.")]
    public Color birdColor = Color.blue;

    private Renderer birdRenderer;

    [Header("Link to Water Level Controller (Task Event Source)")]
    [Tooltip("Reference to the script that controls the water puzzle and fires onWaterBottleComplete.")]
    public ShaderWaterLevelController waterLevelController;

    [Header("Change Color Trigger")]
    [Tooltip("Trigger that the bird must enter (e.g., near the pitcher rim). Should be tagged ChangeColorTrigger.")]
    public GameObject changeColorTrigger;

    [Header("VO Events (Optional)")]
    [Tooltip("Optional VO/event fired when the water puzzle is complete and the bird is now allowed to change color.")]
    public UnityEvent onColorChangeAvailable;   // e.g. VO: “The water’s high enough now…”

    [Tooltip("Optional VO/event fired when the bird actually changes color.")]
    public UnityEvent onBirdColored;            // e.g. VO: “It looks so much brighter now.”

    // Only true once the water puzzle is finished
    private bool changeColor = false;

    private void Start()
    {
        birdRenderer = GetComponent<Renderer>();

        if (waterLevelController != null)
        {
            // Listen for future completion
            waterLevelController.onWaterBottleComplete.AddListener(SetChangeColorTrigger);

            // Handle case where water was already done BEFORE this bird spawned
            if (waterLevelController.waterBottleComplete)
            {
                SetChangeColorTrigger();
            }
        }
        else
        {
            Debug.LogWarning("[BirdChangeColor] No waterLevelController assigned. Bird will never get permission to change color.");
        }
    }

    /// <summary>
    /// Called when the water puzzle completes (event) OR immediately at Start() if already complete.
    /// </summary>
    private void SetChangeColorTrigger()
    {
        changeColor = true;
        onColorChangeAvailable?.Invoke();
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("BirdChangeColor: OnTriggerEnter");

        // If water puzzle not complete yet, ignore
        if (!changeColor)
        {
            Debug.Log("BirdChangeColor: changeColor is false, ignoring trigger.");
            return;
        }

        if (collision.gameObject.CompareTag("ChangeColorTrigger"))
        {
            Debug.Log("BirdChangeColor: ChangeColorTrigger entered, changing bird color.");

            if (birdRenderer != null)
            {
                birdRenderer.material.color = birdColor;
                onBirdColored?.Invoke();
            }
            else
            {
                Debug.LogWarning("[BirdChangeColor] No Renderer found on bird.");
            }
        }
    }
}
