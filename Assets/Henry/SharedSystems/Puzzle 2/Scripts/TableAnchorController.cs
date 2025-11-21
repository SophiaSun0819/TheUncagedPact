using UnityEngine;

public class TableAnchorController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject tableAnchor;   // your Table Anchor (parent for puzzles)
    public Transform rayOrigin;      // right-hand controller or Main Camera
    public LayerMask placementMask = ~0; // Default is fine if you have colliders

    [Header("Tuning")]
    public float rotateStep = 10f;   // yaw adjust per tap
    public float nudgeStep  = 0.02f; // small XY nudge

    bool placed;

    void Update()
    {
        // Ray from controller/camera to find placement point
        if (!placed && Physics.Raycast(rayOrigin.position, rayOrigin.forward, out var hit, 6f, placementMask))
        {
            // OPTIONAL: draw a tiny gizmo preview
            Debug.DrawRay(hit.point, Vector3.up * 0.2f, Color.cyan);
            // Press A (OVR) or Space (keyboard) to place
            if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.One))
            {
                tableAnchor.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(Vector3.forward, Vector3.up));
                placed = true;
            }
        }

        if (!placed) return;

        // Optional rotate/nudge after placement (Q/E and arrow keys as editor fallbacks)
        if (Input.GetKeyDown(KeyCode.Q)) tableAnchor.transform.Rotate(0f, -rotateStep, 0f, Space.World);
        if (Input.GetKeyDown(KeyCode.E)) tableAnchor.transform.Rotate(0f,  rotateStep, 0f, Space.World);

        if (Input.GetKeyDown(KeyCode.LeftArrow))  tableAnchor.transform.position += -tableAnchor.transform.right * nudgeStep;
        if (Input.GetKeyDown(KeyCode.RightArrow)) tableAnchor.transform.position +=  tableAnchor.transform.right * nudgeStep;
        if (Input.GetKeyDown(KeyCode.UpArrow))    tableAnchor.transform.position +=  tableAnchor.transform.forward * nudgeStep;
        if (Input.GetKeyDown(KeyCode.DownArrow))  tableAnchor.transform.position += -tableAnchor.transform.forward * nudgeStep;
    }
}
