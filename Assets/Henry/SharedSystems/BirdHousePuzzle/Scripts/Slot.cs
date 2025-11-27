using UnityEngine;

public class Slot : MonoBehaviour
{
    public int requiredPieceId = 0;
    public Transform snapPoint;                 // child object, scale 1
    public Puzzle4_HouseController controller;  // drag controller here

    void OnTriggerEnter(Collider other)
    {
        Try(other);
    }

    void OnTriggerStay(Collider other)
    {
        Try(other);
    }

    void Try(Collider other)
    {
        // XR Grab often puts colliders on children
        var piece = other.GetComponentInParent<HousePiece>();
        if (!piece) return;
        if (piece.isLocked) return;

        // 🔴 Don't snap while the piece is still grabbed.
        // Most grab systems set isKinematic = true while held.
        var rb = piece.GetComponent<Rigidbody>();
        if (rb && rb.isKinematic)
        {
            // still in the hand → wait until they let go
            return;
        }

        Debug.Log($"[Slot] {name} detected free piece {piece.name}");
        controller.TryPlacePiece(this, piece);
    }
}
