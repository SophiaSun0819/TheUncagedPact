using UnityEngine;

public class ZoneToggle : MonoBehaviour
{
    [Header("Object to toggle (VFX mesh, glow, etc.)")]
    public GameObject target;

    private bool _inside = false;

    // Called from PhysicalButtonZone
    public void SetInside(bool inside)
    {
        _inside = inside;
        if (target)
            target.SetActive(_inside);   // ON while inside, OFF when outside
    }

    // Optional: keep this if you ever want a manual toggle from UI / debug
    public void Toggle()
    {
        if (target)
            target.SetActive(!target.activeSelf);
    }
}
