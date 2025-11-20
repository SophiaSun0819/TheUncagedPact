using UnityEngine;

public class PhysicalButtonZone : MonoBehaviour
{
    [Header("References")]
    public PhysicalButton button;

    [Header("Which controller?")]
    public bool useRightController = true;   // true = right, false = left

    bool _controllerInside;

    void OnTriggerEnter(Collider other)
    {
        // For now, assume anything that enters could be the controller.
        // (Optional: later you can filter by tag/layer if you want.)
        _controllerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        _controllerInside = false;
    }

    void Update()
    {
        if (!_controllerInside || button == null)
            return;

        var which = useRightController ? OVRInput.Controller.RTouch
                                       : OVRInput.Controller.LTouch;

        // Front trigger (index)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, which))
        {
            button.Press();
        }
    }
}
