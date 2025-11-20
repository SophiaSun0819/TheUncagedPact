using UnityEngine;

public class SpawnInFrontOfPlayer : MonoBehaviour
{
    [Header("Target (HMD / Camera)")]
    public Transform target;      // drag OVRHmd or Main Camera here

    [Header("Offsets")]
    public float distance = 1.2f; // meters in front of player
    public float chestOffsetY = -0.3f; // chest is a bit below head

    void Start()
    {
        // If nothing assigned, try the main camera
        if (!target && Camera.main != null)
            target = Camera.main.transform;

        if (!target)
        {
            Debug.LogWarning("[SpawnInFrontOfPlayer] No target set.");
            return;
        }

        // Use forward direction on the horizontal plane (ignore tilt)
        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = target.forward;
        forward.Normalize();

        // Position in front of player at chest height
        Vector3 pos = target.position + forward * distance;
        pos.y += chestOffsetY;

        transform.position = pos;

        // Face the player (only Y axis)
        transform.rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
    }
}
