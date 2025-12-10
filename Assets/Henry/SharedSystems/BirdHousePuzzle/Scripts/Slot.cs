using UnityEngine;

public class Slot : MonoBehaviour
{
    public int requiredPieceId = 0;
    public Transform snapPoint;
    public Puzzle4_HouseController controller;

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
        // Only react to house pieces
        var piece = other.GetComponentInParent<HousePiece>();
        if (!piece) return;
        if (piece.isLocked) return;

        // Don't snap while still being grabbed
        var rb = piece.GetComponent<Rigidbody>();
        if (rb && rb.isKinematic) return;

        controller.TryPlacePiece(this, piece);
    }
}
