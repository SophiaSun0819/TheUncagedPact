using UnityEngine;

public class SoundBall : MonoBehaviour
{
    [Header("ID")]
    public int soundID = 0;

    [Header("Disable on Lock")]
    public MonoBehaviour[] disableOnLock;    // Grabbable, GrabInteractable, etc.

    bool _isLocked = false;
    Vector3 _originalLocalScale;

    // For out-of-bounds reset
    Vector3 _homePosition;
    Quaternion _homeRotation;
    Transform _homeParent;

    public bool IsLocked => _isLocked;

    void Awake()
    {
        // Save local scale (safe if all parents are scale = 1)
        _originalLocalScale = transform.localScale;

        // Save initial spawn as "home"
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
        _homeParent   = transform.parent;
    }

    /// <summary>
    /// Called when the bird is placed correctly on its perch.
    /// Freezes it, disables grabbing & collisions.
    /// </summary>
    public void Lock()
    {
        if (_isLocked) return;
        _isLocked = true;

        // Restore scale (in case grab parenting messed it up)
        transform.localScale = _originalLocalScale;

        // Stop all physics
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity       = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.useGravity      = false;
            rb.isKinematic     = true;
        }

        // Disable collider so other birds can't knock it off
        var col = GetComponent<Collider>();
        if (col)
        {
            col.enabled = false;
        }

        // Disable grab components (Grabbable, GrabInteractable, etc.)
        if (disableOnLock != null)
        {
            foreach (var mb in disableOnLock)
            {
                if (mb) mb.enabled = false;
            }
        }
    }

    /// <summary>
    /// Called by OutOfBoundsZone when the bird falls off the puzzle.
    /// Resets back to its original spawn position.
    /// </summary>
    public void ResetToHome()
    {
        // If it's already locked on a perch, don't move it back
        if (_isLocked) return;

        // Restore parent
        if (_homeParent != null)
            transform.SetParent(_homeParent, true);

        // Reset pose & scale
        transform.position   = _homePosition;
        transform.rotation   = _homeRotation;
        transform.localScale = _originalLocalScale;

        // Reset physics so it can be grabbed/used again
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity       = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.useGravity      = true;
            rb.isKinematic     = false;
        }

        // Make sure collider & grab scripts are active for free birds
        var col = GetComponent<Collider>();
        if (col)
        {
            col.enabled = true;
        }

        if (disableOnLock != null && !_isLocked)
        {
            foreach (var mb in disableOnLock)
            {
                if (mb) mb.enabled = true;
            }
        }
    }
}
