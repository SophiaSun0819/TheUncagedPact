using UnityEngine;

public class PhysicalButtonZone : MonoBehaviour
{
    public PhysicalButton button;
    public ZoneToggle toggle;       // add this

    bool _controllerInside = false;

    void OnTriggerEnter(Collider other)
    {
        _controllerInside = true;
        if (toggle) toggle.SetInside(true);   // add this
    }

    void OnTriggerExit(Collider other)
    {
        _controllerInside = false;
        if (toggle) toggle.SetInside(false);  // add this
    }

    public void OnButtonInput()
    {
        if (_controllerInside)
            button.Press();
    }
}
