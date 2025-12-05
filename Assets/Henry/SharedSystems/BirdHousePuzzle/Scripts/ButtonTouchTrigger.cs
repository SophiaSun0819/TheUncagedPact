using UnityEngine;

public class ButtonTouchTrigger : MonoBehaviour
{
    [Header("Button")]
    public PhysicalButton button;

    [Header("Controller Filter")]
    public LayerMask controllerLayers;

    [Header("Debug")]
    public bool debugLogs = true;

    void OnTriggerEnter(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"[ButtonTouch] OnTriggerEnter with {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})", this);
        }

        if (!IsController(other)) return;
        if (button == null) return;

        button.Press();
    }

    bool IsController(Collider other)
    {
        int objLayer = other.gameObject.layer;
        bool layerOk = ((1 << objLayer) & controllerLayers.value) != 0;

        if (debugLogs)
        {
            Debug.Log($"[ButtonTouch] IsController? {other.name} on layer {LayerMask.LayerToName(objLayer)} -> {layerOk}", this);
        }

        return layerOk;
    }
}
