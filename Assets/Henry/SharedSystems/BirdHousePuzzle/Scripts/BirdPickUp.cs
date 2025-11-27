using UnityEngine;

public class BirdPickUp : MonoBehaviour
{
    [Header("Targets (filled at runtime or in Inspector)")]
    public Transform playerTarget;       // XR Camera or stop point
    public Transform pageHoldPoint;      // child on bird where page attaches
    public Transform homePoint;          // where bird should return (perch). Optional.

    [Header("Flight Settings")]
    public float flySpeed       = 1.5f;
    public float turnSpeed      = 5f;
    public float arriveDistance = 0.05f;
    public float hoverDistance  = 0.6f;   // how far in front of player
    public float hoverHeight    = 0.2f;   // how high above player

    [Header("Hover Wobble")]
    public float wobbleRadius    = 0.05f;  // how big the wobble is
    public float wobbleFrequency = 2f;     // how fast it wobbles

    Vector3 _wobbleSideDir;

    Rigidbody _rb;
    Collider  _col;

    Transform _pageTransform;
    bool _inFlight       = false;
    bool _goingToPlayer  = false;
    bool _goingHome      = false;

    // backup home pose if no homePoint is provided
    Vector3    _initialPos;
    Quaternion _initialRot;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        // random horizontal wobble direction per bird
        Vector3 rnd = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _wobbleSideDir = rnd.sqrMagnitude > 0.0001f ? rnd.normalized : Vector3.right;

        // remember starting position as fallback home
        _initialPos = transform.position;
        _initialRot = transform.rotation;
    }

    /// <summary>
    /// Called by NewPageSpawner when the page is spawned.
    /// </summary>
    public void StartDelivery(Transform pageTransform)
    {
        if (pageTransform == null)
        {
            Debug.LogWarning("[BirdPickUp] StartDelivery called with null page.");
            return;
        }

        _pageTransform  = pageTransform;
        _inFlight       = true;
        _goingToPlayer  = false;
        _goingHome      = false;

        SetPhysicsForFlight(true);
    }

    /// <summary>
    /// Called by spawner/origin to tell bird where the player is.
    /// </summary>
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }

    /// <summary>
    /// Called by spawner to give the bird a snap point for the page.
    /// (Usually set on the prefab in Inspector.)
    /// </summary>
    public void SetPageSnapPoint(Transform snap)
    {
        pageHoldPoint = snap;
    }

    /// <summary>
    /// Called by spawner to define bird's home perch in the scene.
    /// </summary>
    public void SetHomePoint(Transform t)
    {
        homePoint = t;
    }

    /// <summary>
    /// Called by PagePickup when the player takes the page.
    /// Bird should fly back home.
    /// </summary>
    public void ReturnHome()
    {
        // clear any page delivery, go into "go home" mode
        _pageTransform  = null;
        _goingToPlayer  = false;
        _goingHome      = true;
        _inFlight       = true;

        SetPhysicsForFlight(true);
    }

    void Update()
    {
        if (!_inFlight)
            return;

        // 1) Going to page
        if (!_goingToPlayer && !_goingHome)
        {
            if (_pageTransform == null)
            {
                // Page disappeared – abort and stop flight
                Debug.LogWarning("[BirdPickUp] Page was destroyed during flight.");
                StopFlight();
                return;
            }

            Vector3 targetPos = _pageTransform.position + Vector3.up * 0.05f;
            bool reached = FlyStep(targetPos);

            if (reached)
            {
                // Attach page to the bird
                if (pageHoldPoint != null)
                {
                    // 1) Remember world scale BEFORE parenting
                    Vector3 worldScaleBefore = _pageTransform.lossyScale;

                    // 2) Parent to bird
                    _pageTransform.SetParent(pageHoldPoint, worldPositionStays: false);
                    _pageTransform.localPosition = Vector3.zero;
                    _pageTransform.localRotation = Quaternion.identity;

                    // 3) Restore original world scale
                    Vector3 parentScale  = pageHoldPoint.lossyScale;
                    Vector3 desiredScale = worldScaleBefore;

                    _pageTransform.localScale = new Vector3(
                        parentScale.x != 0 ? desiredScale.x / parentScale.x : desiredScale.x,
                        parentScale.y != 0 ? desiredScale.y / parentScale.y : desiredScale.y,
                        parentScale.z != 0 ? desiredScale.z / parentScale.z : desiredScale.z
                    );

                    // 4) Notify PagePickup so it can freeze physics while riding
                    var pagePickup = _pageTransform.GetComponent<PagePickup>();
                    if (pagePickup != null)
                    {
                        pagePickup.OnAttachedToBird(this);
                    }
                }

                _goingToPlayer = true;  // next phase: deliver to player
            }
        }
        // 2) Delivering to the player
        else if (_goingToPlayer)
        {
            if (playerTarget == null)
            {
                Debug.LogWarning("[BirdPickUp] No playerTarget set, stopping flight to player.");
                StopFlight();
                return;
            }

            // Always use LIVE position of the player each frame
            Vector3 playerPos =
                playerTarget.position +
                playerTarget.forward * hoverDistance +
                Vector3.up * hoverHeight;

            bool reached = FlyStep(playerPos);

            if (reached)
            {
                // Arrived in front of player – hover there with physics off until page is taken
                _inFlight = false;
                // physics stays off so it just floats
            }
        }
        // 3) Going home (after page is grabbed)
        else if (_goingHome)
        {
            Vector3 homePos   = homePoint ? homePoint.position  : _initialPos;
            Quaternion homeRot = homePoint ? homePoint.rotation : _initialRot;

            bool reached = FlyStep(homePos);

            if (reached)
            {
                _goingHome = false;
                _inFlight  = false;

                // Snap final rotation
                transform.rotation = homeRot;

                // Option: keep physics off so it perches solidly
                SetPhysicsForFlight(false);
            }
        }
    }

    /// <summary>
    /// One flight step towards a world-space position.
    /// Returns true if we reached the target (within arriveDistance).
    /// </summary>
    bool FlyStep(Vector3 targetWorldPos)
    {
        Vector3 toTarget = targetWorldPos - transform.position;
        float dist = toTarget.magnitude;

        if (dist < arriveDistance)
            return true;

        if (dist > 0.0001f)
        {
            // Add wobble so it's not a straight line
            Vector3 wobble = GetWobbleOffset();
            Vector3 dir    = (toTarget + wobble).normalized;

            Vector3 move = dir * flySpeed * Time.deltaTime;

            if (move.magnitude > dist)
                move = dir * dist;

            transform.position += move;

            Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
            if (flatDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }

        return false;
    }

    Vector3 GetWobbleOffset()
    {
        float t = Time.time * wobbleFrequency;

        // sideways wobble + slight vertical bob
        Vector3 side  = _wobbleSideDir * Mathf.Sin(t) * wobbleRadius;
        Vector3 upBob = Vector3.up * Mathf.Cos(t) * (wobbleRadius * 0.5f);

        return side + upBob;
    }

    void SetPhysicsForFlight(bool inFlight)
    {
        if (_rb)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
#else
            _rb.velocity       = Vector3.zero;
#endif
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity      = !inFlight;
            _rb.isKinematic     = inFlight;
        }

        if (_col)
        {
            // Make collider a trigger while flying so we don't get stuck on planes
            _col.isTrigger = inFlight;
        }
    }

    void StopFlight()
    {
        _inFlight      = false;
        _goingToPlayer = false;
        _goingHome     = false;
        SetPhysicsForFlight(false);
    }
}
