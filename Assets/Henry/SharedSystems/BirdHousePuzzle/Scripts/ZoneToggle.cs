using UnityEngine;

public class ZoneToggle : MonoBehaviour
{
    [Header("Object to toggle (visual only!)")]
    public GameObject target;                // e.g. ButtonVisual child

    [Header("Linked press zone (optional)")]
    public PhysicalButtonZone linkedZone;    // drag the same zone here

    bool _inside = false;

    public void Toggle()
    {
        // Only allow toggle if controller is actually inside
        if (!_inside) return;

        if (target)
        {
            bool newActive = !target.activeSelf;
            target.SetActive(newActive);

            // If we just turned visuals OFF, also force the zone to exit
            if (!newActive && linkedZone != null)
            {
                linkedZone.ForceExit();
            }
        }
    }

    public void SetInside(bool inside)
    {
        _inside = inside;
    }
}
