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

    [Tooltip("How close the bird must get before switching to next state.")]
    public float arriveDistance = 0.05f;     // ← EDIT THIS TO CONTROL STOP DISTANCE

    public float hoverDistance  = 0.6f;      // how far in front of player
    public float hoverHeight    = 0.2f;      // how high above player

    [Header("Hover Wobble")]
    public float wobbleRadius    = 0.05f;  
    public float wobbleFrequency = 2f;     

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

    BirdChangeCustom _colorLogic;   // ← reference to color script

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        _colorLogic = GetComponentInChildren<BirdChangeCustom>();

        // random wobble direction
        Vector3 rnd = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _wobbleSideDir = rnd.sqrMagnitude > 0.0001f ? rnd.normalized : Vector3.right;

        // remember starting position as fallback home
        _initialPos = transform.position;
        _initialRot = transform.rotation;
    }

    // -------------------------------------------------------------------
    // *** ONLY ALLOW DELIVERY IF BIRD IS FULLY COLORED ***
    // -------------------------------------------------------------------
    public void StartDelivery(Transform pageTransform)
    {
        if (pageTransform == null)
        {
            Debug.LogWarning("[BirdPickUp] StartDelivery called with null page.");
            return;
        }

        // 🚫 BLOCK DELIVERY IF BIRD HASN'T DRUNK WATER YET
        if (_colorLogic != null && !_colorLogic.IsFullyColored)
        {
            Debug.Log("[BirdPickUp] Bird not fully colored yet — blocking delivery.");
            // (optional) trigger a VO here:
            SoundPuzzleVOController.Instance?.CueAfterWaterHomeHint();
            return;
        }

        // ✔ Bird is fully colored — allow delivery
        _pageTransform  = pageTransform;
        _inFlight       = true;
        _goingToPlayer  = false;
        _goingHome      = false;

        SetPhysicsForFlight(true);
    }

    // -------------------------------------------------------------------
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }

    public void SetPageSnapPoint(Transform snap)
    {
        pageHoldPoint = snap;
    }

    public void SetHomePoint(Transform t)
    {
        homePoint = t;
    }

    public void ReturnHome()
    {
        _pageTransform  = null;
        _goingToPlayer  = false;
        _goingHome      = true;
        _inFlight       = true;

        SetPhysicsForFlight(true);
    }

    // -------------------------------------------------------------------
    void Update()
    {
        if (!_inFlight)
            return;

        // -------------------------------------------------------------------
        // 1) Going TO the page
        // -------------------------------------------------------------------
        if (!_goingToPlayer && !_goingHome)
        {
            if (_pageTransform == null)
            {
                StopFlight();
                return;
            }

            Vector3 targetPos = _pageTransform.position + Vector3.up * 0.05f;
            bool reached = FlyStep(targetPos);

            if (reached)
            {
                // Attach page to bird
                if (pageHoldPoint != null)
                {
                    Vector3 worldScaleBefore = _pageTransform.lossyScale;

                    _pageTransform.SetParent(pageHoldPoint, worldPositionStays: false);
                    _pageTransform.localPosition = Vector3.zero;
                    _pageTransform.localRotation = Quaternion.identity;

                    Vector3 parentScale  = pageHoldPoint.lossyScale;
                    Vector3 desiredScale = worldScaleBefore;

                    _pageTransform.localScale = new Vector3(
                        desiredScale.x / parentScale.x,
                        desiredScale.y / parentScale.y,
                        desiredScale.z / parentScale.z
                    );

                    var pagePickup = _pageTransform.GetComponent<PagePickup>();
                    pagePickup?.OnAttachedToBird(this);
                }

                _goingToPlayer = true;
            }
        }

        // -------------------------------------------------------------------
        // 2) Flying TOWARD the player
        // -------------------------------------------------------------------
        else if (_goingToPlayer)
        {
            if (playerTarget == null)
            {
                StopFlight();
                return;
            }

            Vector3 playerPos =
                playerTarget.position +
                playerTarget.forward * hoverDistance +
                Vector3.up * hoverHeight;

            bool reached = FlyStep(playerPos);

            if (reached)
            {
                _inFlight = false;

                // VO: Page delivery
                SoundPuzzleVOController.Instance?.CueBirdDeliverNote();
            }
        }

        // -------------------------------------------------------------------
        // 3) Returning home
        // -------------------------------------------------------------------
        else if (_goingHome)
        {
            Vector3 homePos    = homePoint ? homePoint.position  : _initialPos;
            Quaternion homeRot = homePoint ? homePoint.rotation : _initialRot;

            bool reached = FlyStep(homePos);

            if (reached)
            {
                _goingHome = false;
                _inFlight  = false;

                transform.rotation = homeRot;

                SetPhysicsForFlight(false);
            }
        }
    }

    // -------------------------------------------------------------------
    bool FlyStep(Vector3 targetWorldPos)
    {
        Vector3 toTarget = targetWorldPos - transform.position;
        float dist = toTarget.magnitude;

        if (dist < arriveDistance)
            return true;

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

        return false;
    }

    Vector3 GetWobbleOffset()
    {
        float t = Time.time * wobbleFrequency;

        Vector3 side  = _wobbleSideDir * Mathf.Sin(t) * wobbleRadius;
        Vector3 upBob = Vector3.up * Mathf.Cos(t) * (wobbleRadius * 0.5f);

        return side + upBob;
    }

    void SetPhysicsForFlight(bool flight)
    {
        if (_rb)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
#else
            _rb.velocity       = Vector3.zero;
#endif
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity      = !flight;
            _rb.isKinematic     = flight;
        }

        if (_col)
            _col.isTrigger = flight;
    }

    void StopFlight()
    {
        _inFlight      = false;
        _goingToPlayer = false;
        _goingHome     = false;
        SetPhysicsForFlight(false);
    }
}
