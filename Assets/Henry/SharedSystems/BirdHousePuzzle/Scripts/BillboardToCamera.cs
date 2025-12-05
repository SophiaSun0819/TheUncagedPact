using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        Vector3 dir = transform.position - _cam.transform.position;
        dir.y = 0f; // keep upright if you don't want tilt
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }
}
