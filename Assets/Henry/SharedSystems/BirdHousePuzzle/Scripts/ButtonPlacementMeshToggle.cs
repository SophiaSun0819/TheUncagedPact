using UnityEngine;

public class ButtonPlacementMeshToggle : MonoBehaviour
{
    [Header("Mesh to show/hide for alignment")]
    public GameObject meshTarget;

    private bool _visible = true;

    public void ToggleMesh()
    {
        _visible = !_visible;
        if (meshTarget) meshTarget.SetActive(_visible);
    }

    public void ShowMesh()
    {
        _visible = true;
        if (meshTarget) meshTarget.SetActive(true);
    }

    public void HideMesh()
    {
        _visible = false;
        if (meshTarget) meshTarget.SetActive(false);
    }
}
