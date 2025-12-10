using UnityEngine;
using UnityEngine.Events;

public class FollowTransform : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
