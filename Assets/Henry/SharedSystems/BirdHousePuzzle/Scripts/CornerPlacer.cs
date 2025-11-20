using UnityEngine;

public class CornerPlacer : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform cornerAnchor;   // CornerAnchor_BirdHouse
    public Transform rayOrigin;      // Camera / center eye
    public float distance = 0.8f;
    public float heightOffset = 0.0f;

    void Start()
    {
        PlaceOnce();
        enabled = false; // <- IMPORTANT: script stops running after Start
    }

    void PlaceOnce()
    {
        if (!cornerAnchor || !rayOrigin) return;

        Vector3 flatForward = Vector3.ProjectOnPlane(rayOrigin.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;

        Vector3 pos = rayOrigin.position;
        pos += flatForward * distance;
        pos.y = rayOrigin.position.y + heightOffset;

        cornerAnchor.position = pos;
        cornerAnchor.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    }
}
