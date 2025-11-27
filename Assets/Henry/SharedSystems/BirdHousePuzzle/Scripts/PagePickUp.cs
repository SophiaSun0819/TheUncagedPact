using UnityEngine;

public class PagePickup : MonoBehaviour
{
    Rigidbody _rb;
    Collider _col;

    bool _attachedToBird = false;
    BirdPickUp _carrier;   // which bird is carrying this page

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        if (_rb == null)
            Debug.LogWarning("[PagePickup] No Rigidbody found on page.", this);
        if (_col == null)
            Debug.LogWarning("[PagePickup] No Collider found on page.", this);
    }

    /// <summary>
    /// Called by BirdPickUp when the page is parented to the bird.
    /// </summary>
    public void OnAttachedToBird(BirdPickUp carrier)
    {
        _carrier        = carrier;
        _attachedToBird = true;

        if (_rb)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
#else
            _rb.velocity       = Vector3.zero;
#endif
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity      = false;
            _rb.isKinematic     = true;
        }

        if (_col)
        {
            // Optional: avoid bumping into bird
            _col.isTrigger = true;
        }
    }

    /// <summary>
    /// Called by your grab event (UnityEvent wrapper) when player grabs the page.
    /// </summary>
    public void OnGrabbed()
    {
        if (!_attachedToBird)
            return;

        _attachedToBird = false;

        // Detach from the bird
        transform.SetParent(null, true);

        // Re-enable normal physics
        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.useGravity  = true;
        }

        if (_col)
        {
            _col.isTrigger = false;
        }

        // Tell the bird to fly back home
        if (_carrier != null)
        {
            _carrier.ReturnHome();
            _carrier = null;
        }

        Debug.Log("[PagePickup] Page detached from bird and told bird to go home.", this);
    }
}
