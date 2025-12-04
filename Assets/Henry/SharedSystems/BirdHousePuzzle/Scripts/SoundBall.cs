using UnityEngine;

public class SoundBall : MonoBehaviour
{
    [Header("ID")]
    public int soundID = 0;

    [Header("Disable on Lock")]
    public MonoBehaviour[] disableOnLock;    // Grabbable, GrabInteractable, etc.

    bool _isLocked = false;

    // We still remember the original WORLD scale for reset,
    // but we won't touch scale when locking.
    Vector3 _originalWorldScale;

    // For out-of-bounds reset
    Vector3 _homePosition;
    Quaternion _homeRotation;
    Transform _homeParent;

    public bool IsLocked => _isLocked;

    void Awake()
    {
        _originalWorldScale = transform.lossyScale;

        // Save initial spawn as "home"
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
        _homeParent   = transform.parent;
    }

    // Helper: apply a desired WORLD scale even if we have a scaled parent
    void ApplyWorldScale(Vector3 worldScale)
    {
        Transform p = transform.parent;
        if (p == null)
        {
            transform.localScale = worldScale;
        }
        else
        {
            Vector3 ps = p.lossyScale;
            transform.localScale = new Vector3(
                ps.x != 0 ? worldScale.x / ps.x : worldScale.x,
                ps.y != 0 ? worldScale.y / ps.y : worldScale.y,
                ps.z != 0 ? worldScale.z / ps.z : worldScale.z
            );
        }
    }

    /// <summary>
    /// Called when the bird is placed correctly on its perch.
    /// Freezes it, disables grabbing & collisions.
    /// </summary>
    public void Lock()
    {
        if (_isLocked) return;
        _isLocked = true;

        // 🚫 IMPORTANT: Do NOT change scale here.
        // The bird was already unparented and snapped by SoundBox,
        // so its size is already correct.

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

        // Reset pose
        transform.position = _homePosition;
        transform.rotation = _homeRotation;
        ApplyWorldScale(_originalWorldScale);

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
