using UnityEngine;

public class OpenDrawer : MonoBehaviour
{
   
     [Header("target object")]
    public GameObject drawer;

    [Header("Raycast setting")]
    public Transform rayOrigin;

    
    public float rayDistance = 5f;

    private Animator targetAnimator;

    void Start()
    {
        if (drawer != null)
            targetAnimator = drawer.GetComponent<Animator>();
            targetAnimator.SetBool("open", false);
    }

    void Update()
    {
        // 1. 检测右手触发键（OVRInput）
        bool rightTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        // 2. 发射 Raycast
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // 3. 击中目标并按下右Trigger
            if (hit.collider.gameObject == drawer)
            {
                if (rightTriggerPressed)
                {
                    Debug.Log("Raycast 命中目标 + 右Trigger → open = true");
                    targetAnimator.SetBool("open", true);
                }
            }
        }
    }

    //（可选）场景中显示射线
    private void OnDrawGizmos()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance);
        }
    }
}
