using UnityEngine;

public class TableCalibrator : MonoBehaviour
{
    public Transform tableAnchor;      // drag your Table Anchor
    public Transform rayOrigin;        // drag right-hand controller or main camera
    public LayerMask placementMask;    // set to Default for now
    public GameObject ghost;           // small plane/cube as a preview (child of this object)

    public float rotateStep = 10f;
    public float nudgeStep = 0.02f;

    bool placed;

    void Update()
    {
        if (placed) return;

        // Ray from controller/camera forward
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out var hit, 5f, placementMask))
        {
            ghost.SetActive(true);
            ghost.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90,0,0));
        }
        else ghost.SetActive(false);

        // A button (keyboard fallback: Space) to place
        if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.One))
        {
            if (ghost.activeSelf)
            {
                tableAnchor.SetPositionAndRotation(ghost.transform.position, ghost.transform.rotation);
                placed = true;
                ghost.SetActive(false);
            }
        }

        // Optional quick rotate with Q/E (or use thumbstick later)
        if (Input.GetKeyDown(KeyCode.Q)) tableAnchor.Rotate(0, -rotateStep, 0, Space.World);
        if (Input.GetKeyDown(KeyCode.E)) tableAnchor.Rotate(0,  rotateStep, 0, Space.World);
    }
}
