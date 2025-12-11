using UnityEngine;

public class GuideBird : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;       // set by EndSequenceManager
    public Transform doorTarget;   // set later when door spawns

    [Header("Movement")]
    public float orbitRadius = 0.6f;
    public float orbitHeight = 0.2f;
    public float orbitSpeed = 1.5f;
    public float flySpeed = 2.0f;

    bool leading = false;
    float angle;

    void Update()
    {
        if (!leading)
        {
            if (player == null) return;

            // orbit around player
            angle += orbitSpeed * Time.deltaTime;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * orbitRadius;
            offset.y = orbitHeight;

            Vector3 targetPos = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);

            // look along the orbit path
            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
                transform.forward = Vector3.Lerp(transform.forward, dir, 10f * Time.deltaTime);
        }
        else
        {
            if (doorTarget == null) return;

            // fly towards door
            Vector3 dir = (doorTarget.position - transform.position).normalized;
            transform.position += dir * flySpeed * Time.deltaTime;
            transform.forward = Vector3.Lerp(transform.forward, dir, 10f * Time.deltaTime);
        }
    }

    public void StartLeading()
    {
        leading = true;
    }
}
