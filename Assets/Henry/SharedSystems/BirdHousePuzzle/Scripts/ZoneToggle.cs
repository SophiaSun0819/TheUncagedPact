using UnityEngine;

public class ZoneToggle : MonoBehaviour
{
    [Header("Object to toggle")]
    public GameObject target;

    private bool _inside = false;

    public void Toggle()
    {
        if (!_inside) return;   // ⛔ prevent toggle unless controller is inside
        if (target) target.SetActive(!target.activeSelf);
    }

    public void SetInside(bool inside)
    {
        _inside = inside;
    }
}
