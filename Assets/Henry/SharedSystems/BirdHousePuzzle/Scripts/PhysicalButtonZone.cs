using UnityEngine;

public class PhysicalButtonZone : MonoBehaviour
{
    [Header("References")]
    public ZoneToggle toggle;       // optional: for mesh visibility only

    [Header("UI / VFX")]
    public GameObject hintUI;
    public GameObject vfxObject;

    [Header("Controller Filter")]
    public LayerMask controllerLayers;   // ONLY the Controller layer should be checked

    [Header("Debug")]
    public bool debugLogs = true;

    bool _controllerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"[Zone] OnTriggerEnter with {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})", this);
        }

        if (!IsController(other)) return;

        _controllerInside = true;

        if (toggle) toggle.SetInside(true);
        if (hintUI)   hintUI.SetActive(true);
        if (vfxObject) vfxObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"[Zone] OnTriggerExit with {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})", this);
        }

        if (!IsController(other)) return;

        _controllerInside = false;

        if (toggle) toggle.SetInside(false);
        if (hintUI)   hintUI.SetActive(false);
        if (vfxObject) vfxObject.SetActive(false);
    }

    public void ForceExit()
    {
        _controllerInside = false;

        if (toggle) toggle.SetInside(false);
        if (hintUI)   hintUI.SetActive(false);
        if (vfxObject) vfxObject.SetActive(false);
    }

    bool IsController(Collider other)
    {
        int objLayer = other.gameObject.layer;
        bool layerOk = ((1 << objLayer) & controllerLayers.value) != 0;

        if (debugLogs)
        {
            Debug.Log($"[Zone] IsController? {other.name} on layer {LayerMask.LayerToName(objLayer)} -> {layerOk}", this);
        }

        return layerOk;
    }
}
