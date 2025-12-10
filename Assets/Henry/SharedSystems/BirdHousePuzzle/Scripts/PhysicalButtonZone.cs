using UnityEngine;

public class PhysicalButtonZone : MonoBehaviour
{
    [Header("References")]
    public PhysicalButton button;   // still here if you need it for other stuff
    public ZoneToggle toggle;       // this will now point to your VFX object

    [Header("Who can activate this zone")]
    public string controllerTag = "Controller";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(controllerTag)) return;

        if (toggle != null)
            toggle.SetInside(true);   // VFX ON
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(controllerTag)) return;

        if (toggle != null)
            toggle.SetInside(false);  // VFX OFF
    }
}
